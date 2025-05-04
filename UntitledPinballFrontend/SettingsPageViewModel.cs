using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UntitledPinballFrontend
{
    public partial class SettingsPageViewModel: INotifyPropertyChanged
{
        public string ScanPath
        {
            set
            {
                FileScanner.Instance.ScanPath = value;
                OnPropertyChanged();
            }
            get 
            { 
                return FileScanner.Instance.ScanPath; 
            }
        }

        public string ExecutablePath
        {
            set
            {
                TableLauncher.Instance.ExePath = value;
                OnPropertyChanged();
            }
            get { return TableLauncher.Instance.ExePath; }
        }

        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
