using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;

namespace BT_Printer_Demo
{
    class BTdevices
    {
        static ManualResetEvent DevicesLoaded = new ManualResetEvent(false);
        static List<DeviceInformation> devices = new List<DeviceInformation>();
        public BTdevices()
        {
            devices.Clear();
            start();
        }

        private async void start()
        {
            await loadDevices();
        }
        public static Task<List<DeviceInformation>> GetDevicesAsync()
        {
            return Task.Run(() =>
            {
                DevicesLoaded.WaitOne();
                return devices;
            });
        }

        /// <summary>
        /// return a list of paired BT devices with SPP support
        /// </summary>
        /// <returns></returns>
        public async Task loadDevices()
        {
            DeviceInformationCollection dataServiceDeviceCollection = null;
            
        
            // Find all paired instances of the Rfcomm service and display them in a list
            dataServiceDeviceCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

            if (dataServiceDeviceCollection.Count > 0)
            {
                devices.Clear();
                foreach (var dataServiceDevice in dataServiceDeviceCollection)
                {
                    devices.Add(dataServiceDevice);
                }
                
            }
            else
            {
                //NotifyUser(
                //    "No SPP services were found. Please pair with a device that is advertising the SPP service.",
                //    NotifyType.ErrorMessage);
            }
            DevicesLoaded.Set();
        }
    }
}
