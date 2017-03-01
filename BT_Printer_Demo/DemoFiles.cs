using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;

namespace BT_Printer_Demo
{
    class DemoFiles
    {
        public DemoFiles()
        {

        }
        public async Task<List<StorageFile>> getFiles()
        {
            List<StorageFile> demofiles = new List<StorageFile>();
            try
            {
                demofiles.Clear();
                //fill the list with the files
                var folder = await StorageFolder.GetFolderFromPathAsync(Windows.ApplicationModel.Package.Current.InstalledLocation.Path + @"\files");
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                foreach (StorageFile f in files)
                    demofiles.Add(f);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ListFiles: " + ex.Message);
            }
            return demofiles;
        }
    }
}
