using HidSharp;
using System;

namespace SCTB_HIDI2C_I2CDotNet
{
	internal enum ResultCode : byte
	{
		Ok = 0,
		ProtocolError = 0x81,
		InvalidCommand = 0x80,
		NotSupported = 0x82,
		HwError = 0xC0,
	}

	public enum HwError
	{
		/// Bus error
		Bus = 1 << 0,

		/// Arbitration loss
		Arbitration = 1 << 1,

		/// No ack received
		Acknowledge = 1 << 2,

		/// Overrun/underrun
		Overrun = 1 << 3,

		/// Timeout
		Timeout = 1 << 4,

		/// unknown error
		Unknown = Bus
			| Arbitration
			| Acknowledge
			| Overrun
			| Timeout,
	}

	public class HidI2C : IDisposable
	{
		#region Fields

		public static readonly uint DefaultI2CTimeout = 10;

		public const int I2C_AddressMax = 0x7F;
		public const int I2C_AddressMin = 0;

		public const int MaxTransactionPyload = 60;

		private readonly HidDevice SCTBHidI2CDevice;
		private readonly RequestBuilder RequestBuilder;
		private HidStream Stream = null;

		#endregion Fields

		#region Properties

		public uint Timeout_ms { get; set; } = DefaultI2CTimeout;

		#endregion Properties

		#region Methods

		public void Close()
		{
			if (Stream != null)
			{
				Stream.Close();
				Stream = null;
			}
		}

		public void Dispose() => Close();

		/// <summary>
		/// Use the single byte write style to get an ack bit from writing to an address with no commands.
		/// </summary>
		/// <returns></returns>
		public bool I2C_Detect(int i2c_addr)
		{
			var req = RequestBuilder.BuildScanRequest(i2c_addr);
			Stream.Write(req);

			byte[] resp;
			try
			{
				resp = Stream.Read();
			}
			catch (TimeoutException ex)
			{
				throw new Timeout(ex);
			}

			var error_code = resp[1];
			if (error_code == (byte)ResultCode.Ok)
			{
				return true;
			}
			else if ((error_code & (byte)ResultCode.HwError) == (byte)ResultCode.HwError && (error_code & ~(byte)ResultCode.HwError) == (byte)HwError.Acknowledge)
			{
				return false;
			}

			throw new UnknownError(error_code);
		}

		/// <summary>
		/// Reset I2C module
		/// </summary>
		public void I2C_Reset()
		{
			var req = RequestBuilder.BuildResetBusRequest();
			Stream.Write(req);

			byte[] resp;
			try
			{
				resp = Stream.Read();
			}
			catch (TimeoutException ex)
			{
				throw new Timeout(ex);
			}

			var error_code = resp[1];
			if (error_code != (byte)ResultCode.Ok)
			{
				throw new UnknownError(error_code);
			}
		}

		public void Read(int i2c_addr, out byte[] data, int size)
		{
			var req = RequestBuilder.BuildReadRequest(i2c_addr, size);
			Stream.Write(req);

			byte[] resp;
			try
			{
				resp = Stream.Read();
			}
			catch (TimeoutException ex)
			{
				throw new Timeout(ex);
			}

			var error_code = resp[1];
			if (error_code == (byte)ResultCode.Ok)
			{
				var len = resp[2];
				data = new byte[len];
				Array.Copy(resp, 3, data, 0, len);
			}
			else if ((error_code & (byte)ResultCode.HwError) == (byte)ResultCode.HwError)
			{
				throw new TransactionException(error_code);
			}
			else
			{
				throw new UnknownError(error_code);
			}
		}

		/// <summary>
		/// Set the i2c speed desired
		/// </summary>
		/// <param name="speed_khz">speed: in khz, will round down to 20, 100, 400, 750
		/// 750 is closer to 1000 for bytes, but slower around acks and each byte start.
		/// </param>
		public void SetSpeed(uint speed_khz = 100)
		{
			var req = RequestBuilder.BuildSetSpeedRequest(speed_khz);
			Stream.Write(req);

			byte[] resp;
			try
			{
				resp = Stream.Read();
			}
			catch (TimeoutException ex)
			{
				throw new Timeout(ex);
			}

			var err_code = resp[1];
			if (err_code != (byte)ResultCode.Ok)
			{
				throw new SpeedComtrolException(speed_khz);
			}
		}

		public bool TryOpen()
		{
			if (Stream == null)
			{
				var options = new OpenConfiguration();
				options.SetOption(OpenOption.Exclusive, true);
				if (!SCTBHidI2CDevice.TryOpen(options, out Stream))
				{
					return false;
				}

				Stream.ReadTimeout = (int)Timeout_ms;
				Stream.WriteTimeout = (int)Timeout_ms;
			}
			return true;
		}

		public void Write(int i2c_addr, byte[] v)
		{
			var req = RequestBuilder.BuildWriteRequest(i2c_addr, v);
			Stream.Write(req);

			byte[] resp;
			try
			{
				resp = Stream.Read();
			}
			catch (TimeoutException ex)
			{
				throw new Timeout(ex);
			}

			var error_code = resp[1];
			if (error_code == (byte)ResultCode.Ok)
			{
				return;
			} else if ((error_code & (byte)ResultCode.HwError) == (byte)ResultCode.HwError)
			{
				throw new TransactionException(error_code);
			} else
			{
				throw new UnknownError(error_code);
			}
		}

		#endregion Methods

		#region Constructors

		public HidI2C(HidDevice device)
		{
			SCTBHidI2CDevice = device;
			RequestBuilder = new RequestBuilder();
		}

		public HidI2C(int vid, int pid)
		{
			if (!DeviceList.Local.TryGetHidDevice(out SCTBHidI2CDevice, vid, pid))
			{
				throw new DeviceNotFoundException(vid, pid);
			}
			RequestBuilder = new RequestBuilder();
		}

		#endregion Constructors
	}
}