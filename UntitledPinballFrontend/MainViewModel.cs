using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UntitledPinballFrontend
{
    public class MainViewModel: INotifyPropertyChanged
    {
        private bool _startButtonEnabled = false;
        public bool StartButtonEnabled
        {
            get
            {
                return _startButtonEnabled;
            }

            set
            {
                _startButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
