using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using JobReporter2.Model;
using JobReporter2.Helpers;

namespace JobReporter2.ViewModel
{
    public class ThresholdManagerViewModel : ObservableObject
    {
        private ObservableCollection<ThresholdModel> _thresholds;
        public ObservableCollection<ThresholdModel> Thresholds
        {
            get => _thresholds;
            set => SetProperty(ref _thresholds, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        public ThresholdManagerViewModel()
        {
            Thresholds = new ObservableCollection<ThresholdModel>(
                SettingsHelper.LoadThresholds().Select(t => new ThresholdModel
                {
                    Name = t.Name,
                    IsEnabled = t.IsEnabled,
                    Value1 = t.Value1,
                    Value2 = t.Value2
                })
            );

            SaveCommand = new RelayCommand(SaveThresholds);
            CloseCommand = new RelayCommand(Close);
        }

        public void SaveThresholds()
        {
            SettingsHelper.SaveThresholds(Thresholds);
            ApplyThresholdStyling();
        }

        private void Close()
        {
            if (System.Windows.Application.Current.Windows
                    .OfType<System.Windows.Window>()
                    .FirstOrDefault(w => w.DataContext == this) is System.Windows.Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void ApplyThresholdStyling()
        {
            // Notify all relevant components about updated thresholds
            foreach (var threshold in Thresholds)
            {
                Console.WriteLine($"Threshold Updated: {threshold.Name}, Enabled: {threshold.IsEnabled}, Values: {threshold.Value1}, {threshold.Value2}");
            }
        }
    }
}
