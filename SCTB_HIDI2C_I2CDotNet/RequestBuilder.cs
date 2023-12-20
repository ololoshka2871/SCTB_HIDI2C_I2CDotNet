using System;

namespace SCTB_HIDI2C_I2CDotNet
{
	internal class RequestBuilder
	{
		#region Constants

		public const byte FirstByte = 0xFF;

		public const byte I2CReadRequest = 0x0B;
		public const byte I2CWriteRequest = 0x0A;

		public const byte SetSpeedRequest = 0x10;

		#endregion Constants

		#region Methods

		public byte[] BuildReadRequest(int i2c_addr, int data_len)
			=> new byte[] { FirstByte, I2CReadRequest, (byte)data_len, (byte)i2c_addr };

		public byte[] BuildWriteRequest(int i2c_addr, byte[] v)
		{
			var data_size = v.Length;
			var result = new byte[4 + data_size];
			result[0] = FirstByte;
			result[1] = I2CWriteRequest;
			result[2] = (byte)(v.Length + 1);
			result[3] = (byte)i2c_addr;
			Buffer.BlockCopy(v, 0, result, 4, data_size);
			return result;
		}

		public byte[] BuildScanRequest(int i2c_addr)
			=> new byte[] { FirstByte, I2CReadRequest, 1, (byte)i2c_addr };

		public byte[] BuildSetSpeedRequest(uint speed_khz) => new byte[4] { FirstByte, SetSpeedRequest, (byte)(speed_khz & 0xff), (byte)(speed_khz >> 8) };

		#endregion Methods
	}
}