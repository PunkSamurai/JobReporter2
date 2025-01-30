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
        //private static readonly string SettingsFilePath = "C:\\Users\\dveli\\source\\repos\\PunkSamurai\\JobReporter2\\Settings.json";
        private static readonly string SettingsFilePath = "C:\\Users\\LENOVO\\source\\repos\\PunkSamurai\\JobReporter2\\Settings.json";

        // Load shifts from the settings file
        public static ObservableCollection<ShiftModel> LoadShifts()
        {
            if (!File.Exists(SettingsFilePath))
            {
                // Return an empty collection if the file doesn't exist
                return new ObservableCollection<ShiftModel>();
            }

            try
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonConvert.DeserializeObject<SettingsModel>(json);

                // Use null check and default to an empty ObservableCollection
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
                var settings = new SettingsModel { Shifts = shifts };
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving shifts: {ex.Message}");
            }
        }
    }

    // Root object for the settings file
    public class SettingsModel
    {
        public ObservableCollection<ShiftModel> Shifts { get; set; }
    }
}
