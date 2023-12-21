using System;

namespace SCTB_HIDI2C_I2CDotNet
{
	public class DeviceNotFoundException : HidI2CException
	{
		#region Fields

		public readonly int pid;
		public readonly int vid;

		#endregion Fields

		#region Constructors

		public DeviceNotFoundException() : base()
		{
		}

		public DeviceNotFoundException(int vid, int pid) : base()
		{
			this.vid = vid;
			this.pid = pid;
		}

		#endregion Constructors

		#region Methods

		public override string ToString()
		{
			return $"Can't find Sctb HidI2C device with VID={vid}, PID={pid}";
		}

		#endregion Methods
	}

	public class HidI2CException : Exception
	{
		#region Constructors

		public HidI2CException() : base()
		{
		}

		public HidI2CException(string message) : base(message)
		{
		}

		#endregion Constructors
	}

	public class SpeedComtrolException : HidI2CException
	{

		#region Constructors

		internal SpeedComtrolException(uint speed) : base($"Failed to setup bus speed {speed} KHz")
		{
		}

		#endregion Constructors
	}

	public class UnknownError : HidI2CException
	{

		#region Constructors

		internal UnknownError(byte e) : base($"Unknown I2C error 0x{e:X}")
		{
		}

		#endregion Constructors
	}

	public class Timeout : HidI2CException
	{
		#region Constructors

		internal Timeout(TimeoutException ex) : base(ex.Message)
		{
		}

		#endregion Constructors
	}

	public class TransactionException : HidI2CException
	{
		#region Fields

		public readonly HwError error;

		#endregion Fields

		#region Constructors

		internal TransactionException(byte status) : base()
		{
			error = (HwError)(status &~(byte)ResultCode.HwError);
		}

		#endregion Constructors
	}
}