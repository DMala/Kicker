using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UntitledPinballFrontend
{
    internal class TableLauncher
    {
        private static string exePath = "E:\\Visual Pinball\\VPinballX64.exe";
        private static string workingDir = "E:\\Visual Pinball";

        public void LaunchTable(String tablePath)
        {
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = exePath;
            startInfo.WorkingDirectory = workingDir;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = $"-play \"{tablePath}\" -Minimized";

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process? exeProcess = Process.Start(startInfo))
                {
                    exeProcess?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
