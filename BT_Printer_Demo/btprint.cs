using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BT_Printer_Demo
{
    class btprint
    {
        string _file = null;
        DeviceInformation _device = null;
        private StreamSocket dataSocket = null;
        private RfcommDeviceService dataService = null;
        private DataWriter dataWriter = null;
        public btprint(DeviceInformation theDevice, string theFile)
        {
            _file = theFile;
            _device = theDevice;
        }

        public async Task doPrint()
        {
            System.Diagnostics.Debug.WriteLine("doPrint...");
            bool bConnect = await connect();
            Task tSend = SendFile();
            tSend.Start();
            while (tSend.Status == TaskStatus.Running)
            {
                await Task.Delay(200);
            }
            System.Diagnostics.Debug.WriteLine("doPrint() DONE");
        }
        private async Task<bool> connect()
        {
            System.Diagnostics.Debug.WriteLine("Connect()...");
            var dataServiceDevice = _device;
            dataService = await RfcommDeviceService.FromIdAsync(_device.Id);
            bool bRet = false;

            if (dataService == null)
            {
                NotifyUser("Access to the device is denied because the application was not granted access", NotifyType.StatusMessage);
                return bRet;
            }
            

            lock (this)
            {
                dataSocket = new StreamSocket();
            }
            try
            {
                await dataSocket.ConnectAsync(dataService.ConnectionHostName, dataService.ConnectionServiceName);

                dataWriter = new DataWriter(dataSocket.OutputStream);

                DataReader dataReader = new DataReader(dataSocket.InputStream);
                ReceiveStringLoop(dataReader);
                bRet = true;
            }
            catch (Exception ex)
            {
                switch ((uint)ex.HResult)
                {
                    case (0x80070490): // ERROR_ELEMENT_NOT_FOUND
                        NotifyUser("Please verify that the device is using SPP.", NotifyType.ErrorMessage);
                        break;
                    case (0x80070103): //not connected, possibly switched off
                        NotifyUser("Please verify that the device is switched ON.", NotifyType.ErrorMessage);
                        break;
                    default:
                        throw;
                }
            }
            System.Diagnostics.Debug.WriteLine("Connect is done with " + bRet.ToString());
            return bRet;
        }
        private async Task SendFile()
        {
            System.Diagnostics.Debug.WriteLine("SendFile...");
            try
            {
                string filename = _file;
                // fp3macklabel.prn
                // Open file in application package
                // needs to be marked as Content and Copy Allways

                var fileToRead = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///files/" + filename, UriKind.Absolute));
                byte[] buffer = new byte[1024];
                int readcount = 0;
                using (BinaryReader fileReader = new BinaryReader(await fileToRead.OpenStreamForReadAsync()))
                {
                    int read = fileReader.Read(buffer, 0, buffer.Length);
                    while (read > 0)
                    {
                        readcount += read;
                        Stream streamWrite = dataSocket.OutputStream.AsStreamForWrite();
                        streamWrite.Write(buffer, 0, read);
                        streamWrite.Flush();
                        //the following does corrupt the byte stream!!!!!
                        //byte[] buf = new byte[read];
                        //Array.Copy(buffer, buf, read);
                        //chatWriter.WriteBytes(buf);
                        //await chatWriter.FlushAsync();
                        //fileWriter.Write(buffer, 0, read);
                        read = fileReader.Read(buffer, 0, buffer.Length);
                    }
                }
                NotifyUser("sendFile " + readcount.ToString(), NotifyType.StatusMessage);
                await dataWriter.StoreAsync();
            }
            //catch hresult = 0x8000000e
            catch (NullReferenceException ex)
            {
                NotifyUser("Error: " + ex.HResult.ToString() + " - " + ex.Message,
                    NotifyType.StatusMessage);
            }
            catch (IOException ex)
            {
                NotifyUser("Error: " + ex.HResult.ToString() + " - " + ex.Message,
                    NotifyType.StatusMessage);
            }
            catch (Exception ex)
            {
                // TODO: Catch disconnect -  HResult = 0x80072745 - catch this (remote device disconnect) ex = {"An established connection was aborted by the software in your host machine. (Exception from HRESULT: 0x80072745)"}
                NotifyUser("Error: " + ex.HResult.ToString() + " - " + ex.Message,
                    NotifyType.StatusMessage);
            }
            Disconnect("done");
            System.Diagnostics.Debug.WriteLine("SendFile done");
        }

        private async void ReceiveStringLoop(DataReader dataReader)
        {
            try
            {
                uint bufLen = dataReader.UnconsumedBufferLength;
                if (bufLen > 0)
                {
                    byte[] buffer = new byte[bufLen];
                    uint size = await dataReader.LoadAsync(bufLen);
                    dataReader.ReadBytes(buffer);
                    string s = toHex(buffer);
                    NotifyUser(s,NotifyType.StatusMessage);
                    ReceiveStringLoop(dataReader);
                }
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    if (dataSocket == null)
                    {
                        // Do not print anything here -  the user closed the socket.
                        // HResult = 0x80072745 - catch this (remote device disconnect) ex = {"An established connection was aborted by the software in your host machine. (Exception from HRESULT: 0x80072745)"}
                    }
                    else
                    {
                        Disconnect("Read stream failed with error: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Cleans up the socket and DataWriter and reset the UI
        /// </summary>
        /// <param name="disconnectReason"></param>
        private void Disconnect(string disconnectReason)
        {
            if (dataWriter != null)
            {
                dataWriter.DetachStream();
                dataWriter = null;
            }


            if (dataService != null)
            {
                dataService.Dispose();
                dataService = null;
            }
            lock (this)
            {
                if (dataSocket != null)
                {
                    dataSocket.Dispose();
                    dataSocket = null;
                }
            }

            NotifyUser(disconnectReason, NotifyType.StatusMessage);
        }

        private string toHex(byte[] buffer)
        {
            string s = "";
            foreach (byte b in buffer)
                if (b < 32)
                    s += "<" + b.ToString("x") + ">";
                else
                    s += System.Text.Encoding.UTF8.GetString(new byte[] { b });
            return s;
        }

        /// <summary>
        /// Used to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            System.Diagnostics.Debug.WriteLine(type.ToString() + ":" + strMessage);
        }
        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };
    }
}
