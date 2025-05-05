using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Kicker
{
    internal class TableLauncher
    {
        private static TableLauncher? instance;
        public static TableLauncher Instance
        {
            get
            {
                instance ??= new TableLauncher();
                return instance;
            }
        }

        private string _exePath = "";
        public string ExePath
        {
            set
            {
                _exePath = value;
                workingDir = Path.GetDirectoryName(_exePath) ?? "";
                ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["PinballPath"] = value;
            }
            get { return _exePath; }
        }
        private string workingDir = "";

        TableLauncher()
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            _exePath = localSettings.Values["PinballPath"] as string ?? "";
            workingDir = Path.GetDirectoryName(_exePath) ?? "";
        }

        public void LaunchTable(String tablePath)
        {
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = ExePath;
            startInfo.WorkingDirectory = workingDir;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = $"-play \"{tablePath}\" -Minimized";

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using Process? exeProcess = Process.Start(startInfo);
                exeProcess?.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
