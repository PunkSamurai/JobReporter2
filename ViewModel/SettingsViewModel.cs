using JobReporter2.Helpers;
using JobReporter2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JobReporter2.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        public ObservableCollection<ShiftModel> Shifts;
        public ObservableCollection<ThresholdModel> Thresholds;

        public ICommand SaveAllCommand { get; }
        public ICommand CloseCommand { get; }

        public SettingsViewModel(ObservableCollection<ShiftModel> shifts, ObservableCollection<ThresholdModel> thresholds)
        {
            Shifts = shifts;
            Thresholds = thresholds;

            SaveAllCommand = new RelayCommand(SaveAll);
            CloseCommand = new RelayCommand(Close);
        }

        private void SaveAll()
        {
            // Save shifts
            SettingsHelper.SaveShifts(Shifts);
            // Save thresholds
            SettingsHelper.SaveThresholds(Thresholds);

            // Close window after saving
            if (System.Windows.Application.Current.Windows
                    .OfType<System.Windows.Window>()
                    .FirstOrDefault(w => w.DataContext == this) is System.Windows.Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void Close()
        {
            if (System.Windows.Application.Current.Windows
                    .OfType<System.Windows.Window>()
                    .FirstOrDefault(w => w.DataContext == this) is System.Windows.Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}
