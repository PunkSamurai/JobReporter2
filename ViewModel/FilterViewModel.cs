using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;

namespace JobReporter2.ViewModel
{
    public class FilterViewModel : ObservableObject
    {
        private DateTime? _startDate;
        private DateTime? _endDate;

        public DateTime? StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public List<string> AvailableConnections { get; set; }
        public ObservableCollection<string> SelectedConnections { get; set; }

        public List<string> AvailableEndTypes { get; set; }
        public ObservableCollection<string> SelectedEndTypes { get; set; }

        public RelayCommand ApplyCommand { get; }
        public RelayCommand ResetCommand { get; }
        public RelayCommand CancelCommand { get; }

        public FilterViewModel()
        {
            SelectedConnections = new ObservableCollection<string>();
            SelectedEndTypes = new ObservableCollection<string>();

            ApplyCommand = new RelayCommand(ApplyFilter);
            ResetCommand = new RelayCommand(ResetFilter);
            CancelCommand = new RelayCommand(CancelFilter);
        }

        private void ApplyFilter()
        {
            // Get the current window and set the DialogResult to true
            if (System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this) is Window window)
            {
                window.DialogResult = true; // This ensures the parent detects a successful apply
                window.Close();
            }
        }

        private void ResetFilter()
        {
            StartDate = null;
            EndDate = null;
            SelectedConnections.Clear();
            SelectedEndTypes.Clear();
        }

        private void CancelFilter()
        {
            // Close window with cancel result
            System.Windows.Application.Current.Windows[System.Windows.Application.Current.Windows.Count - 1]?.Close();
        }
    }
}
