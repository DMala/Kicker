using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using OpenMcdf;
using Windows.Devices.Geolocation;

namespace UntitledPinballFrontend
{
    public partial class FileScanner
    {
        private static FileScanner? instance;
        public ObservableCollection<TableEntry> tablesList = [];

        public static FileScanner Instance
        {
            get
            {
                instance ??= new FileScanner();
                return instance;
            }
        }

        private FileScanner()
        {
            Scan();
        }

        void Scan()
        {
            var files = Directory.GetFiles("E:\\Visual Pinball\\Tables", "*.vpx");

            foreach (string file in files)
            {
                tablesList.Add(ParseVPX(file));
            }
        }

        private static TableEntry ParseVPX(string filePath)
        {
            var table = new TableEntry();

            CompoundFile cf = new(filePath);
            try {
                var tableInfoStorage = cf.RootStorage.GetStorage("TableInfo");
                GetBaseTableInfo(cf, filePath, ref table);
                GetExtendedTableInfo(ref table, cf);
                GetAuthorInfo(ref table, cf);
            }
            catch (Exception ex)
            {
                // No TableInfo, fall back to getting the info from the filename.
                Console.WriteLine($"Error getting table information: {ex.Message}");
                ParseFilePath(filePath, ref table);
            }
            finally
            {
                cf.Close();
            }

            return table;
        }

        private static void GetBaseTableInfo(CompoundFile cf, string filePath, ref TableEntry table)
        {
            var tableInfoStorage = cf.RootStorage.GetStorage("TableInfo");
            CFStream tableNameStream = tableInfoStorage.GetStream("TableName");
            byte[] buffer = tableNameStream.GetData();
            var tableName = Encoding.Unicode.GetString(buffer);
            ParseTableName(tableName, ref table);
            ParseFilePath(filePath, ref table);
        }

        private static void GetExtendedTableInfo(ref TableEntry table, CompoundFile cf)
        {
            try
            {
                var tableInfoStorage = cf.RootStorage.GetStorage("TableInfo");
                CFStream tableVersionStream = tableInfoStorage.GetStream("TableVersion");
                byte[] buffer = tableVersionStream.GetData();
                table.TableVersion = Encoding.Unicode.GetString(buffer);

                CFStream tableRevStream = tableInfoStorage.GetStream("TableSaveRev");
                buffer = tableVersionStream.GetData();
                table.TableRevision = Encoding.Unicode.GetString(buffer);

                CFStream tableRulesStream = tableInfoStorage.GetStream("TableRules");
                buffer = tableRulesStream.GetData();
                table.TableRules = Encoding.Unicode.GetString(buffer);

                CFStream tableDescriptionStream = tableInfoStorage.GetStream("TableDescription");
                buffer = tableDescriptionStream.GetData();
                table.TableDescription = Encoding.Unicode.GetString(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting extended table info: {ex.Message}");
            }
        }

        private static void GetAuthorInfo(ref TableEntry table, CompoundFile cf)
        {
            try
            {
                var tableInfoStorage = cf.RootStorage.GetStorage("TableInfo");
                CFStream authorNameStream = tableInfoStorage.GetStream("AuthorName");
                byte[] buffer = authorNameStream.GetData();
                table.AuthorName = Encoding.Unicode.GetString(buffer);
                CFStream authorEmailStream = tableInfoStorage.GetStream("AuthorEmail");
                buffer = authorEmailStream.GetData();
                table.AuthorEmail = Encoding.Unicode.GetString(buffer);
                CFStream authorWebsiteStream = tableInfoStorage.GetStream("AuthorWebsite");
                buffer = authorWebsiteStream.GetData();
                table.AuthorWebsite = Encoding.Unicode.GetString(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting author info: {ex.Message}");
            }
        }

        private static void ParseFilePath(string filePath, ref TableEntry entry)
        {
            entry.Path = filePath;
            var name = filePath[(filePath.LastIndexOf('\\') + 1)..];
            if (name != null)
            {
                name = name.Replace(".vpx", string.Empty);
            }

            if (name != null)
            {
                ParseTableName(name, ref entry);
            }
        }

        private static void ParseTableName(string tableName, ref TableEntry entry)
        {
            var openParens = tableName.IndexOf('(');
            if (openParens > -1)
            {
                if (entry.Name.Length == 0)
                {
                    entry.Name = tableName[..openParens].Trim();
                }
            }
            else
            {
                if (entry.Name.Length == 0) {
                    entry.Name = tableName;
                }
                return;
            }

            var closeParens = tableName.IndexOf(')');
            if (closeParens > -1) {
                var info = tableName.Substring(++openParens, closeParens - openParens);

                if (entry.Year == string.Empty)
                {
                    Regex reYear = ParseFourDigitYearRegex();
                    Match resultYear = reYear.Match(info);
                    string year = resultYear.Value;

                    if (year != string.Empty)
                    {
                        entry.Year = year;
                    }
                }

                if (entry.Manufacturer == string.Empty)
                {
                    Regex reManufacturer = ParseLettersRegex();
                    MatchCollection resultManufacturer = reManufacturer.Matches(info);

                    string manufacturer = string.Empty;
                    foreach (Match match in resultManufacturer)
                    {
                        manufacturer += match.Value + " ";
                    }

                    if (manufacturer != string.Empty)
                    {
                        entry.Manufacturer = manufacturer.Trim();
                    }
                }

                if (entry.IconPath == null) 
                {
                    entry.IconPath = AddManufacturerLogo(entry.Manufacturer);
                }
            }
        }

        private static ImageSource AddManufacturerLogo(string manufacturer)
        {
            return manufacturer switch
            {
                "Atari" => new SvgImageSource(new Uri("ms-appx:///Assets/Images/AtariLogo.svg")),
                "Bally" => new SvgImageSource(new Uri("ms-appx:///Assets/Images/BallyLogo.svg")),
                "Data East" => new SvgImageSource(new Uri("ms-appx:///Assets/Images/DataEastLogo.svg")),
                "Gottlieb" => new SvgImageSource(new Uri("ms-appx:///Assets/Images/GottliebLogo.svg")),
                "Sega" => new SvgImageSource(new Uri("ms-appx:///Assets/Images/SegaLogo.svg")),
                "Stern" => new SvgImageSource(new Uri("ms-appx:///Assets/Images/SternLogo.svg")),
                "Williams" => new SvgImageSource(new Uri("ms-appx:///Assets/Images/WilliamsLogo.svg")),
                "Zaccaria" => new SvgImageSource(new Uri("ms-appx:///Assets/Images/ZaccariaLogo.svg")),
                _ => new SvgImageSource(new Uri("ms-appx:///Assets/Images/pinmachine.svg"))
            };
        }

        [GeneratedRegex(@"\b\d{4}\b")]
        private static partial Regex ParseFourDigitYearRegex();

        [GeneratedRegex(@"[a-zA-Z]+")]
        private static partial Regex ParseLettersRegex();
    }
}
