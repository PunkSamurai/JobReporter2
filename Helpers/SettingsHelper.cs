using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using JobReporter2.Model;
using System.Collections.ObjectModel;

namespace JobReporter2.Helpers
{
    public static class SettingsHelper
    {
        private static readonly string SettingsFilePath = "C:\\Users\\LENOVO\\source\\repos\\PunkSamurai\\JobReporter2\\Settings.json";

        // Load shifts from the settings file
        public static ObservableCollection<ShiftModel> LoadShifts()
        {
            if (!File.Exists(SettingsFilePath))
            {
                return new ObservableCollection<ShiftModel>();
            }
            try
            {
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
    }

    // Root object for the settings file
    public class SettingsModel
    {
        public SettingsModel()
        {
            Shifts = new ObservableCollection<ShiftModel>();
            Filters = new List<FilterModel>();
        }

        public ObservableCollection<ShiftModel> Shifts { get; set; }
        public List<FilterModel> Filters { get; set; }
    }
}
