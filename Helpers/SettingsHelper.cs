using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using JobReporter2.Model;
using Newtonsoft.Json;

namespace JobReporter2.Helpers
{
    public static class SettingsHelper
    {
        private static readonly string SettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "JobReporter",
            "Settings");

        private static readonly string SettingsFilePath = Path.Combine(SettingsFolder, "Settings.json");
        private static readonly string DefaultSettingsPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "DefaultSettings.json");

        static SettingsHelper()
        {
            // Ensure the settings directory exists
            EnsureSettingsDirectoryExists();

            // Make sure settings file exists (or create from default)
            EnsureSettingsFileExists();
        }

        private static void EnsureSettingsDirectoryExists()
        {
            if (!Directory.Exists(SettingsFolder))
            {
                try
                {
                    Directory.CreateDirectory(SettingsFolder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating settings directory: {ex.Message}");
                }
            }
        }

        private static void EnsureSettingsFileExists()
        {
            if (!File.Exists(SettingsFilePath))
            {
                try
                {
                    // If default settings file exists, copy it
                    if (File.Exists(DefaultSettingsPath))
                    {
                        File.Copy(DefaultSettingsPath, SettingsFilePath);
                    }
                    else
                    {
                        // Otherwise create a new settings file with default content
                        var defaultSettings = new SettingsModel
                        {
                            Shifts = new ObservableCollection<ShiftModel>
                            {
                                new ShiftModel { Name = "Shift 1", IsEnabled = true, StartTime = TimeSpan.Parse("09:00:00"), EndTime = TimeSpan.Parse("17:00:00") },
                                new ShiftModel { Name = "Shift 2", IsEnabled = false, StartTime = TimeSpan.Parse("00:00:00"), EndTime = TimeSpan.Parse("23:59:00") },
                                new ShiftModel { Name = "Shift 3", IsEnabled = false, StartTime = TimeSpan.Parse("00:00:00"), EndTime = TimeSpan.Parse("23:59:00") },
                                new ShiftModel { Name = "Shift 4", IsEnabled = false, StartTime = TimeSpan.Parse("00:00:00"), EndTime = TimeSpan.Parse("23:59:00") },
                                new ShiftModel { Name = "Shift 5", IsEnabled = false, StartTime = TimeSpan.Parse("00:01:00"), EndTime = TimeSpan.Parse("23:59:00") }
                            },
                            Filters = new List<FilterModel>(),
                            Thresholds = new List<ThresholdModel>
                            {
                                new ThresholdModel { Name = "PrepTime", IsEnabled = true, Value1 = 15, Value2 = 30, Unit = "Minutes" },
                                new ThresholdModel { Name = "PauseTime", IsEnabled = true, Value1 = 75, Value2 = 50, Unit = "Percent" },
                                new ThresholdModel { Name = "CutTime", IsEnabled = false, Value1 = 50, Value2 = 75, Unit = "Percent" },
                                new ThresholdModel { Name = "TotalTime", IsEnabled = false, Value1 = 50, Value2 = 75, Unit = "Percent" }
                            },
                            XjhDirectory = "",
                            ReportDirectory = ""
                        };

                        string json = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
                        File.WriteAllText(SettingsFilePath, json);
                        Console.WriteLine("Created new settings file at " + SettingsFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating settings file: {ex.Message}");
                }
            }
        }

        // Load shifts from the settings file
        public static ObservableCollection<ShiftModel> LoadShifts()
        {
            if (!File.Exists(SettingsFilePath))
            {
                Console.WriteLine("Settings.json not found");
                return new ObservableCollection<ShiftModel>();
            }
            try
            {
                Console.WriteLine("Settings.json :" + SettingsFilePath);
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonConvert.DeserializeObject<SettingsModel>(json);
                return settings?.Shifts != null
                    ? new ObservableCollection<ShiftModel>(settings.Shifts)
                    : new ObservableCollection<ShiftModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shifts: {ex.Message}");
                return new ObservableCollection<ShiftModel>();
            }
        }

        // Save shifts to the settings file
        public static void SaveShifts(ObservableCollection<ShiftModel> shifts)
        {
            try
            {
                // First, load existing settings
                SettingsModel settings;
                if (File.Exists(SettingsFilePath))
                {
                    string existingJson = File.ReadAllText(SettingsFilePath);
                    settings = JsonConvert.DeserializeObject<SettingsModel>(existingJson) ?? new SettingsModel();
                }
                else
                {
                    settings = new SettingsModel();
                }

                // Update only the shifts
                settings.Shifts = shifts;

                // Save back to file
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving shifts: {ex.Message}");
            }
        }

        public static ObservableCollection<FilterModel> LoadFilters()
        {
            if (!File.Exists(SettingsFilePath))
            {
                return new ObservableCollection<FilterModel>();
            }
            try
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonConvert.DeserializeObject<SettingsModel>(json);
                return new ObservableCollection<FilterModel>(settings?.Filters ?? new List<FilterModel>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading filters: {ex.Message}");
                return new ObservableCollection<FilterModel>();
            }
        }

        public static void SaveFilters(ObservableCollection<FilterModel> filters)
        {
            try
            {
                // First, load existing settings
                SettingsModel settings;
                if (File.Exists(SettingsFilePath))
                {
                    string existingJson = File.ReadAllText(SettingsFilePath);
                    settings = JsonConvert.DeserializeObject<SettingsModel>(existingJson) ?? new SettingsModel();
                }
                else
                {
                    settings = new SettingsModel();
                }

                // Update only the filters
                settings.Filters = filters.ToList();

                // Save back to file
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving filters: {ex.Message}");
            }
        }

        // Load thresholds from the settings file
        public static ObservableCollection<ThresholdModel> LoadThresholds()
        {
            if (!File.Exists(SettingsFilePath))
            {
                return new ObservableCollection<ThresholdModel>();
            }
            try
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonConvert.DeserializeObject<SettingsModel>(json);
                return settings?.Thresholds != null
                    ? new ObservableCollection<ThresholdModel>(settings.Thresholds)
                    : new ObservableCollection<ThresholdModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading thresholds: {ex.Message}");
                return new ObservableCollection<ThresholdModel>();
            }
        }

        // Save thresholds to the settings file
        public static void SaveThresholds(ObservableCollection<ThresholdModel> thresholds)
        {
            try
            {
                // First, load existing settings
                SettingsModel settings;
                if (File.Exists(SettingsFilePath))
                {
                    string existingJson = File.ReadAllText(SettingsFilePath);
                    settings = JsonConvert.DeserializeObject<SettingsModel>(existingJson) ?? new SettingsModel();
                }
                else
                {
                    settings = new SettingsModel();
                }

                // Update only the thresholds
                settings.Thresholds = thresholds.ToList();

                // Save back to file
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving thresholds: {ex.Message}");
            }
        }

        // Load XjhDirectory from the settings file
        public static string LoadXjhDirectory()
        {
            if (!File.Exists(SettingsFilePath))
            {
                return string.Empty;
            }
            try
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonConvert.DeserializeObject<SettingsModel>(json);
                return settings?.XjhDirectory ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading XjhDirectory: {ex.Message}");
                return string.Empty;
            }
        }

        // Save XjhDirectory to the settings file
        public static void SaveXjhDirectory(string xjhDirectory)
        {
            try
            {
                // First, load existing settings
                SettingsModel settings;
                if (File.Exists(SettingsFilePath))
                {
                    string existingJson = File.ReadAllText(SettingsFilePath);
                    settings = JsonConvert.DeserializeObject<SettingsModel>(existingJson) ?? new SettingsModel();
                }
                else
                {
                    settings = new SettingsModel();
                }

                // Update only the XjhDirectory
                settings.XjhDirectory = xjhDirectory;

                // Save back to file
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving XjhDirectory: {ex.Message}");
            }
        }

        // Load ReportDirectory from the settings file
        public static string LoadReportDirectory()
        {
            if (!File.Exists(SettingsFilePath))
            {
                return string.Empty;
            }
            try
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonConvert.DeserializeObject<SettingsModel>(json);
                return settings?.ReportDirectory ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ReportDirectory: {ex.Message}");
                return string.Empty;
            }
        }

        // Save ReportDirectory to the settings file
        public static void SaveReportDirectory(string reportDirectory)
        {
            try
            {
                // First, load existing settings
                SettingsModel settings;
                if (File.Exists(SettingsFilePath))
                {
                    string existingJson = File.ReadAllText(SettingsFilePath);
                    settings = JsonConvert.DeserializeObject<SettingsModel>(existingJson) ?? new SettingsModel();
                }
                else
                {
                    settings = new SettingsModel();
                }

                // Update only the ReportDirectory
                settings.ReportDirectory = reportDirectory;

                // Save back to file
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving ReportDirectory: {ex.Message}");
            }
        }
    }
}