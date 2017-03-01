using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Storage;
using Windows.Devices.Enumeration;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BT_Printer_Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        DispatcherTimer timer = new DispatcherTimer();
        MenuFlyout popupMenu = new MenuFlyout();
        string currentFile = null;
        public MainPage()
        {
            this.InitializeComponent();

//            myListView.ItemsSource = _files;
            
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(200);
            timer.Start();
        }

        async void timer_Tick(object sender, object e)
        {
            timer.Stop();
            await Load();
            
        }
        private void myListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("clicked " + e.ClickedItem.ToString());
            currentFile = e.ClickedItem.ToString();
            popupMenu.ShowAt((FrameworkElement)sender);
        }
        async Task Load()
        {
          
            BTdevices btDevices = new BT_Printer_Demo.BTdevices();
            BTdeviceList = await BTdevices.GetDevicesAsync();
            BTdevices.GetDevicesAsync().Wait();
            foreach (DeviceInformation di in BTdeviceList) { 
                System.Diagnostics.Debug.WriteLine(di.Name);
                //_btdevices.Add(di);

                MenuFlyoutItem subItem = new MenuFlyoutItem();
                subItem.Name = di.Name;
                subItem.Text = di.Name;
                subItem.Tag = di;
                subItem.Click += SubItem_Click;
                popupMenu.Items.Add(subItem);
            }

            DemoFiles dFiles = new DemoFiles();
            demofiles = await dFiles.getFiles();
            myListView.Items.Clear();
            foreach (StorageFile sf in demofiles)
            {
                System.Diagnostics.Debug.WriteLine(sf.Name);
                myListView.Items.Add(sf.Name);
            }
        }

        private async void SubItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem current = (MenuFlyoutItem)e.OriginalSource;
            DeviceInformation di = (DeviceInformation)current.Tag;
            System.Diagnostics.Debug.WriteLine("popup menu: selected=" + current.Name);
            btprint _btPrint = new btprint(di, currentFile);
            await _btPrint.doPrint();
        }

        //ObservableCollection<StorageFile> _files = new ObservableCollection<StorageFile>();
        //ObservableCollection<DeviceInformation> _btdevices = new ObservableCollection<DeviceInformation>();
        List<DeviceInformation> BTdeviceList = new List<DeviceInformation>();
        List<StorageFile> demofiles = new List<StorageFile>();

    }
}
