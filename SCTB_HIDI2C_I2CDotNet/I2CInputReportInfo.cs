using System.Runtime.InteropServices;

namespace SCTB_HIDI2C_I2CDotNet
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct I2CInputReportInfo
	{
		#region Fields

		/// <summary>
		/// The length of valid data of payload.
		/// </summary>
		public byte length;

		#endregion Fields
	}
}