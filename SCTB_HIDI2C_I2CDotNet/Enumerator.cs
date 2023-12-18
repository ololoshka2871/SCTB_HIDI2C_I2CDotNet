using HidSharp;
using System.Collections.Generic;

namespace SCTB_HIDI2C_I2CDotNet
{
	public static class Enumerator
	{
		#region Methods

		public static IEnumerable<HidDevice> Enumerate(int vid, int pid)
		{
			foreach (var device in DeviceList.Local.GetHidDevices()) { 
				if (device.VendorID == vid && device.ProductID == pid)
				{
					yield return device;
				}
			}
		}

		#endregion Methods
	}
}