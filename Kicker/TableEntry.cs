using System;
using System.Text.Json.Serialization;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

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
        public string IconPath { set; get; }
        
        [JsonIgnore]
        public SvgImageSource Icon
        {
            get
            {
                if (string.IsNullOrEmpty(IconPath))
                {
                    return new SvgImageSource(); ;
                }
                var uri = new Uri(IconPath);
                var icon = new SvgImageSource(uri);
                return icon;
            }
        }

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
            IconPath = string.Empty;
        }
    }
}
