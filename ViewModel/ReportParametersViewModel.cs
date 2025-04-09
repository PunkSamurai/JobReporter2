using JobReporter2.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace JobReporter2.ViewModel
{
    public class ReportParametersViewModel : ObservableObject
    {
        private string _reportTitle;
        private string _selectedReportType;
        private string _selectedTimeFrame;
        private List<string> _reportTypes;
        private List<string> _timeFrames;

        public string ReportTitle
        {
            get => _reportTitle;
            set => SetProperty(ref _reportTitle, value);
        }

        public string SelectedReportType
        {
            get => _selectedReportType;
            set => SetProperty(ref _selectedReportType, value);
        }

        public string SelectedTimeFrame
        {
            get => _selectedTimeFrame;
            set => SetProperty(ref _selectedTimeFrame, value);
        }

        public List<string> ReportTypes
        {
            get => _reportTypes;
            set => SetProperty(ref _reportTypes, value);
        }

        public List<string> TimeFrames
        {
            get => _timeFrames;
            set => SetProperty(ref _timeFrames, value);
        }

        public ICommand GenerateCommand { get; }
        public ICommand CancelCommand { get; }

        public ReportParametersViewModel(List<string> reportTypes, List<string> timeFrames, int index)
        {
            // Initialize with defaults
            ReportTitle = $"Report {index} ({DateTime.Now.ToString("yyyy-MM-dd")})";
            ReportTypes = reportTypes;
            TimeFrames = timeFrames;

            // Select first items if available
            if (reportTypes.Count > 0)
                SelectedReportType = reportTypes[0];

            if (timeFrames.Count > 0)
                SelectedTimeFrame = timeFrames[0];

            // Commands
            GenerateCommand = new RelayCommand(Generate, CanGenerate);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanGenerate()
        {
            // Validate that we have all necessary parameters
            return !string.IsNullOrWhiteSpace(ReportTitle) &&
                   !string.IsNullOrWhiteSpace(SelectedReportType) &&
                   !string.IsNullOrWhiteSpace(SelectedTimeFrame);
        }

        private void Generate()
        {
            // Close the window with successful result
            if (Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this) is Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void Cancel()
        {
            // Close the window with unsuccessful result
            if (Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this) is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}
