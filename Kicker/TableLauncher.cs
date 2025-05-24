using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;

namespace Kicker
{
    internal class TableLauncher
    {
        private const string PINBALL_PATH = "PinballPath";
        private const string LAUNCH_JOY2KEY = "LaunchJoy2Key";
        private const string JOY2KEY_PATH = "Joy2KeyPath";

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
                localSettings.Values[PINBALL_PATH] = value;
            }
            get { return _exePath; }
        }
        private string workingDir = "";
        
        private bool _shouldLaunchJoy2Key;
        public bool ShouldLaunchJoy2Key
        {
            set
            {
                _shouldLaunchJoy2Key = value;
                ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values[LAUNCH_JOY2KEY] = value;
            }
            get { return _shouldLaunchJoy2Key; }
        }

        private String _joy2KeyPath;
        public String joy2KeyPath
        {
            set
            {
                _joy2KeyPath = value;
                ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values[JOY2KEY_PATH] = value;
            }
            get { return _joy2KeyPath; }
        }

        TableLauncher()
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            _exePath = localSettings.Values[PINBALL_PATH] as string ?? "";
            workingDir = Path.GetDirectoryName(_exePath) ?? "";

            _shouldLaunchJoy2Key = localSettings.Values[LAUNCH_JOY2KEY] as bool? ?? false;
            _joy2KeyPath = localSettings.Values[JOY2KEY_PATH] as string ?? "C:\\Joy2Key";
        }

        public void LaunchTable(String? tablePath = null)
        {
            if (ShouldLaunchJoy2Key && !string.IsNullOrEmpty(joy2KeyPath))
            {
                LaunchJoy2Key();
            }

            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new()
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = ExePath,
                WorkingDirectory = workingDir,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            if (!string.IsNullOrEmpty(tablePath))
            {
                startInfo.Arguments = $"-play \"{tablePath}\" -Minimized";
            }

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

        public void LaunchJoy2Key()
        {
            Process[] processes = Process.GetProcessesByName("JoyToKey");
            if (processes.Length == 0)
            {
                // Use ProcessStartInfo class
                ProcessStartInfo startInfo = new()
                {
                    UseShellExecute = true,
                    FileName = joy2KeyPath,
                    WorkingDirectory = Path.GetDirectoryName(joy2KeyPath) ?? ""
                };
                try
                {
                    Process.Start(startInfo);
                    _shouldLaunchJoy2Key = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
