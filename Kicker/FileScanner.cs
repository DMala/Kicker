using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using OpenMcdf;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

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
                }
            }
            get { return _scanPath; }
        }
        public string FileExtension = "vpx";
        private readonly DispatcherQueue DispatcherQueue = DispatcherQueue.GetForCurrentThread();

        private FileScanner()
        {
            LoadSettings();
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

        public event EventHandler<List<TableEntry>>? ScanCompleted;
        public void ScanAsync(bool rescan = false)
        {
            Task.Run(() => Scan(rescan));
        }

        private async Task Scan(bool rescan = false)
        {
            List<TableEntry>? tablesList = null;
            if (!rescan)
            {
                tablesList = await LoadTablesList();
                if (tablesList != null && tablesList.Count > 0)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ScanCompleted?.Invoke(this, tablesList);
                    });
                    return;
                }
            }

            tablesList ??= [];

            if (ScanPath == string.Empty || !Directory.Exists(ScanPath))
            {
                Debug.WriteLine("No path set for scanning.");
                return;
            }

            var files = Directory.GetFiles(ScanPath, $"*.{FileExtension}");

            foreach (string file in files)
            {
                tablesList.Add(ParseVPX(file));
            }

            await PersistTablesList(tablesList);

            ScanCompleted?.Invoke(this, tablesList);
        }

        private static TableEntry ParseVPX(string filePath)
        {
            var table = new TableEntry();

            CompoundFile cf = new(filePath);
            try {
                var tableInfoStorage = cf.RootStorage.GetStorage("TableInfo");
            }
            catch (Exception ex)
            {
                // Cannot open root storage, fall back to getting the info from the filename.
                Debug.WriteLine($"Error getting table information for {filePath}: {ex.Message}");
                ParseFilePath(filePath, ref table);
            }

            if (!GetBaseTableInfo(cf, filePath, ref table))
            {
                // No TableInfo, fall back to getting the info from the filename.
                Debug.WriteLine($"Error getting table information for {filePath}: No TableInfo found");
                ParseFilePath(filePath, ref table);
            }

            GetExtendedTableInfo(ref table, cf);
            GetAuthorInfo(ref table, cf);

            // Special case - If we get the default demo table for the name, fall back to the filename.
            if (table.Name == "Visual Pinball Demo Table")
            {
                table.Name = string.Empty;
                ParseFilePath(filePath, ref table);
            }

            cf.Close();

            return table;
        }

        private static bool GetBaseTableInfo(CompoundFile cf, string filePath, ref TableEntry table)
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
                return false;
            }

            return true;
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

                if (String.IsNullOrEmpty(entry.Year))
                {
                    Regex reYear = ParseFourDigitYearRegex();
                    Match resultYear = reYear.Match(info);
                    string year = resultYear.Value;

                    if (year != string.Empty)
                    {
                        entry.Year = year;
                    }
                }

                if (String.IsNullOrEmpty(entry.Manufacturer))
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

                if (String.IsNullOrEmpty(entry.IconPath)) {
                    entry.IconPath = AddManufacturerLogoUri(entry.Manufacturer);
                } 
            }
        }

        private static string AddManufacturerLogoUri(string manufacturer)
        {
            return manufacturer switch
            {
                "Atari" => "ms-appx:///Assets/Images/AtariLogo.svg",
                "Bally" => "ms-appx:///Assets/Images/BallyLogo.svg",
                "Data East" => "ms-appx:///Assets/Images/DataEastLogo.svg",
                "Gottlieb" => "ms-appx:///Assets/Images/GottliebLogo.svg",
                "Sega" => "ms-appx:///Assets/Images/SegaLogo.svg",
                "Stern" => "ms-appx:///Assets/Images/SternLogo.svg",
                "Williams" => "ms-appx:///Assets/Images/WilliamsLogo.svg",
                "Zaccaria" => "ms-appx:///Assets/Images/ZaccariaLogo.svg",
                _ => "ms-appx:///Assets/Images/pinmachine.svg"
            };
        }

        private static async Task PersistTablesList(List<TableEntry> tables)
        {
            try
            {
                string jsonTables = JsonSerializer.Serialize(tables);

                // Get the local folder
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                // Create or replace the file
                StorageFile file = await localFolder.CreateFileAsync("TablesList.json", CreationCollisionOption.ReplaceExisting);

                // Write the JSON data to the file
                await FileIO.WriteTextAsync(file, jsonTables);
            } 
            catch(Exception ex)
            {
                Debug.WriteLine($"Error persisting tables list: {ex.Message}");
            }
        }

        private static async Task<List<TableEntry>?> LoadTablesList()
        {
            try
            {
                // Get the local folder
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                // Check if the file exists
                StorageFile file = await localFolder.GetFileAsync("TablesList.json");

                // Read the JSON data from the file
                string jsonTables = await FileIO.ReadTextAsync(file);

                // Deserialize the JSON data
                return JsonSerializer.Deserialize<List<TableEntry>>(jsonTables);
            }
            catch (FileNotFoundException)
            {
                // File not found, return null
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading tables list: {ex.Message}");
                return null;
            }
        }

        [GeneratedRegex(@"\b\d{4}\b")]
        private static partial Regex ParseFourDigitYearRegex();

        [GeneratedRegex(@"[a-zA-Z]+")]
        private static partial Regex ParseLettersRegex();
    }
}
