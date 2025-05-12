using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kicker
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        private bool _startButtonEnabled = false;
        public bool StartButtonEnabled
        {
            set
            {
                _startButtonEnabled = value;
                OnPropertyChanged();
            }
            get
            {
                return _startButtonEnabled;
            }
        }

        private List<TableEntry> _tables = [];
        public List<TableEntry> Tables
        {
            get
            {
                return _tables;
            }
            set
            {
                _tables = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public MainViewModel()
        {
            FileScanner.Instance.ScanCompleted += (sender, tables) =>
            {
                Tables = tables;
            };
            FileScanner.Instance.ScanAsync();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Sort(SortType by)
        {
            var cpy = Tables;
            switch (by)
            {
                case SortType.Name:
                    cpy.Sort((x, y) =>
                    {
                        return string.Compare(x.Name, y.Name);
                    });
                    break;
                case SortType.Year:
                    cpy.Sort((x, y) =>
                    {
                        return string.Compare(x.Year, y.Year);
                    });
                    break;
                case SortType.Manufacturer:
                    cpy.Sort((x, y) =>
                    {
                        return string.Compare(x.Manufacturer, y.Manufacturer);
                    });
                    break;
            }

            Tables = cpy;
        }
    }

    public enum SortType
    {
        Name,
        Year,
        Manufacturer
    }
}
