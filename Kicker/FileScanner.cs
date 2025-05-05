using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using OpenMcdf;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kicker
{
    public partial class FileScanner
    {
        private static FileScanner? instance;
        public static FileScanner Instance
        {
            get
            {
                instance ??= new FileScanner();
                return instance;
            }
        }
        
        private string _scanPath = "";
        public string ScanPath {
            set {
                if (_scanPath != value)
                {
                    _scanPath = value;
                    SaveSettings();
                    Scan();
                }
            }
            get { return _scanPath; }
        }
        public string FileExtension = "vpx";

        private FileScanner()
        {
            LoadSettings();
            Scan();
        }

        private void LoadSettings()
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            ScanPath = localSettings.Values["TablesPath"] as string ?? "";
        }

        public void SaveSettings()
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["TablesPath"] = ScanPath;
        }

        public List<TableEntry> Scan()
        {
            List<TableEntry> tablesList = [];

            if (ScanPath == string.Empty || !Directory.Exists(ScanPath))
            {
                Debug.WriteLine("No path set for scanning.");
                return tablesList;
            }

            var files = Directory.GetFiles(ScanPath, $"*.{FileExtension}");

            foreach (string file in files)
            {
                tablesList.Add(ParseVPX(file));
            }

            return tablesList;
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

                // Special case - If we get the default demo table for the name, fall back to the filename.
                if (table.Name == "Visual Pinball Demo Table")
                {
                    table.Name = string.Empty;
                    throw new Exception("Sample table name detected, fall back to filename");
                }

            }
            catch (Exception ex)
            {
                // No TableInfo, fall back to getting the info from the filename.
                Debug.WriteLine($"Error getting table information for {filePath}: {ex.Message}");
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
            try
            {
                var tableInfoStorage = cf.RootStorage.GetStorage("TableInfo");
                CFStream tableNameStream = tableInfoStorage.GetStream("TableName");
                byte[] buffer = tableNameStream.GetData();
                var tableName = Encoding.Unicode.GetString(buffer);
                ParseTableName(tableName, ref table);
                ParseFilePath(filePath, ref table);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting table info for {filePath}: {ex.Message}");
                throw;
            }
        }

        private static void GetExtendedTableInfo(ref TableEntry table, CompoundFile cf)
        {
            CFStorage? cfStorage;
            CFStream cfStream;
            byte[] buffer;

            try
            {
                cfStorage = cf.RootStorage.GetStorage("TableInfo");
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error getting table info for {table.Name}: {ex.Message}");
                return;
            }

            try
            {
                cfStream = cfStorage.GetStream("TableVersion");
                buffer = cfStream.GetData();
                table.TableVersion = Encoding.Unicode.GetString(buffer);
            } 
            catch(Exception ex)
            {
                Debug.WriteLine($"Error getting table version for {table.Name}: {ex.Message}");
            }

            try
            {
                cfStream = cfStorage.GetStream("TableSaveRev");
                buffer = cfStream.GetData();
                table.TableRevision = Encoding.Unicode.GetString(buffer);
            } 
            catch(Exception ex)
            {
                Debug.WriteLine($"Error getting table revision for {table.Name}: {ex.Message}");
            }

            try
            {
                cfStream = cfStorage.GetStream("TableRules");
                buffer = cfStream.GetData();
                table.TableRules = Encoding.Unicode.GetString(buffer);
            } catch(Exception ex)
            {
                Debug.WriteLine($"Error getting table rules for {table.Name}: {ex.Message}");
            }

            try
            {
                cfStream = cfStorage.GetStream("TableDescription");
                buffer = cfStream.GetData();
                table.TableDescription = Encoding.Unicode.GetString(buffer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting table description for {table.Name}: {ex.Message}");
            }
        }

        private static void GetAuthorInfo(ref TableEntry table, CompoundFile cf)
        {
            CFStorage? cfStorage;
            CFStream cfStream;
            byte[] buffer;

            try
            {
                cfStorage = cf.RootStorage.GetStorage("TableInfo");
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error getting author info for {table.Name}: {ex.Message}");
                return;
            }

            try
            {
                cfStream= cfStorage.GetStream("AuthorName");
                buffer = cfStream.GetData();
                table.AuthorName = Encoding.Unicode.GetString(buffer);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error getting author name for {table.Name}: {ex.Message}");
            }

            try
            {
                cfStream = cfStorage.GetStream("AuthorEmail");
                buffer = cfStream.GetData();
                table.AuthorEmail = Encoding.Unicode.GetString(buffer);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error getting author e-mail for {table.Name}: {ex.Message}");
            }

            try 
            { 
                cfStream = cfStorage.GetStream("AuthorWebsite");
                buffer = cfStream.GetData();
                table.AuthorWebsite = Encoding.Unicode.GetString(buffer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting author website for {table.Name}: {ex.Message}");
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

        private static SvgImageSource AddManufacturerLogo(string manufacturer)
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
