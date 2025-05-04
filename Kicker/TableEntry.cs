using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace Kicker
{
    public class TableEntry
    {
        public string Name { set; get; }
        public string Manufacturer { set; get; }
        public string Year { set; get; }
        public string TableVersion { set; get; }
        public string TableRevision { set; get; }
        public string TableDescription { set; get; }
        public string TableRules { set; get; }
        public string TableSaveDate { set; get; }
        public string AuthorName { set; get; }
        public string AuthorEmail { set; get; }
        public string AuthorWebsite { set; get; }
        public string Path { set; get; }
        public ImageSource? IconPath { set; get; }

        public TableEntry()
        {
            Name = string.Empty;
            Manufacturer = string.Empty;
            Year = string.Empty;
            TableVersion = string.Empty;
            TableRevision = string.Empty;
            TableDescription = string.Empty;
            TableRules = string.Empty;
            TableSaveDate = string.Empty;
            AuthorName = string.Empty;
            AuthorEmail = string.Empty;
            AuthorWebsite = string.Empty;
            Path = string.Empty;
            IconPath = null;
        }
    }
}
