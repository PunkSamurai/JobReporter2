using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using JobReporter2.Model;
using System.Windows.Input;
using JobReporter2.Helpers;

namespace JobReporter2.ViewModel
{
    public class FilterViewModel : ObservableObject
    {
        private DateTime? _startDate;
        private DateTime? _endDate;
        private FilterModel _selectedFilter;
        private string _timeFrame;

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

        public string TimeFrame
        {
            get => _timeFrame;
            set
            {
                if (SetProperty(ref _timeFrame, value))
                {
                    UpdateDatesByTimeFrame();
                }
            }
        }

        public List<string> TimeFrameOptions => new List<string>
        {
            "Today",
            "This Week",
            "This Month",
            "This Year",
            "Custom"
        };

        public List<string> AvailableConnections { get; set; }
        public ObservableCollection<string> SelectedConnections { get; set; }

        public List<string> AvailableEndTypes { get; set; }
        public ObservableCollection<string> SelectedEndTypes { get; set; }
        public ObservableCollection<FilterModel> Filters { get; set; } = new ObservableCollection<FilterModel>();

        public FilterModel SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                SetProperty(ref _selectedFilter, value);
                if (value != null)
                {
                    LoadFilter(value);
                }
            }
        }

        public RelayCommand NewFilterCommand { get; }
        public RelayCommand SaveFilterCommand { get; }
        public RelayCommand ApplyCommand { get; }
        public RelayCommand ResetCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand DeleteFilterCommand { get; }  // New command for delete

        public FilterViewModel()
        {
            SelectedConnections = new ObservableCollection<string>();
            SelectedEndTypes = new ObservableCollection<string>();

            NewFilterCommand = new RelayCommand(CreateNewFilter);
            SaveFilterCommand = new RelayCommand(SaveCurrentFilter);
            ApplyCommand = new RelayCommand(ApplyFilter);
            ResetCommand = new RelayCommand(ResetFilter);
            CancelCommand = new RelayCommand(CancelFilter);
            DeleteFilterCommand = new RelayCommand(DeleteFilter);  // New delete command

            TimeFrame = "Today";
            LoadFiltersFromSettings();
        }

        private void UpdateDatesByTimeFrame()
        {
            switch (TimeFrame)
            {
                case "Today":
                    StartDate = DateTime.Today;
                    EndDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;

                case "This Week":
                    StartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    EndDate = StartDate.Value.AddDays(7).AddSeconds(-1);
                    break;

                case "This Month":
                    StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    EndDate = StartDate.Value.AddMonths(1).AddSeconds(-1);
                    break;

                case "This Year":
                    StartDate = new DateTime(DateTime.Today.Year, 1, 1);
                    EndDate = StartDate.Value.AddYears(1).AddSeconds(-1);
                    break;

                case "Custom":
                    // Don't modify dates for custom timeframe
                    break;
            }
        }

        private void CreateNewFilter()
        {
            // Show input dialog for filter name
            string filterName = ShowFilterNameInputDialog();

            if (!string.IsNullOrWhiteSpace(filterName))
            {
                var newFilter = new FilterModel
                {
                    Name = filterName,
                    TimeFrame = "Today",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1).AddSeconds(-1),
                    Connections = new ObservableCollection<string>(),
                    EndTypes = new ObservableCollection<string>()
                };

                Filters.Add(newFilter);
                SelectedFilter = newFilter;
            }
        }

        private string ShowFilterNameInputDialog()
        {
            // Find the current window hosting this ViewModel
            var currentWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);

            // Create and configure the input dialog window
            var dialog = new Window
            {
                Title = "New Filter",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = currentWindow // Set to the current filter window
            };

            var stackPanel = new System.Windows.Controls.StackPanel
            {
                Margin = new Thickness(10)
            };

            var label = new System.Windows.Controls.Label
            {
                Content = "Enter filter name:"
            };

            var textBox = new System.Windows.Controls.TextBox
            {
                Margin = new Thickness(0, 5, 0, 10)
            };

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new System.Windows.Controls.Button
            {
                Content = "OK",
                Width = 75,
                Margin = new Thickness(0, 0, 5, 0),
                IsDefault = true
            };

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Width = 75,
                IsCancel = true
            };

            okButton.Click += (s, e) =>
            {
                dialog.DialogResult = true;
                dialog.Close();
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(buttonPanel);

            dialog.Content = stackPanel;

            // Show dialog and return result
            return dialog.ShowDialog() == true ? textBox.Text : null;
        }


        private void DeleteFilter()
        {
            if (SelectedFilter != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the filter '{SelectedFilter.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Filters.Remove(SelectedFilter);
                    SettingsHelper.SaveFilters(Filters);
                    ResetFilter();
                }
            }
        }

        private bool CanDeleteFilter()
        {
            return SelectedFilter != null;
        }

        // Rest of the existing methods remain the same
        private void SaveCurrentFilter()
        {
            if (SelectedFilter != null)
            {
                SelectedFilter.TimeFrame = TimeFrame;
                SelectedFilter.StartDate = StartDate;
                SelectedFilter.EndDate = EndDate;
                SelectedFilter.Connections = new ObservableCollection<string>(SelectedConnections);
                SelectedFilter.EndTypes = new ObservableCollection<string>(SelectedEndTypes);
                SettingsHelper.SaveFilters(Filters);
            }
        }

        private void LoadFilter(FilterModel filter)
        {
            TimeFrame = filter.TimeFrame;
            StartDate = filter.StartDate;
            EndDate = filter.EndDate;
            SelectedConnections = new ObservableCollection<string>(filter.Connections);
            SelectedEndTypes = new ObservableCollection<string>(filter.EndTypes);
        }

        private void LoadFiltersFromSettings()
        {
            Filters.Clear();
            foreach (var filter in SettingsHelper.LoadFilters())
            {
                Filters.Add(filter);
            }
        }

        private void ApplyFilter()
        {
            if (Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this) is Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void ResetFilter()
        {
            TimeFrame = "Today";
            StartDate = null;
            EndDate = null;
            SelectedConnections.Clear();
            SelectedEndTypes.Clear();
        }

        private void CancelFilter()
        {
            Application.Current.Windows[Application.Current.Windows.Count - 1]?.Close();
        }
    }
}