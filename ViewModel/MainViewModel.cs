using JobReporter2.Model;
using JobReporter2.View;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace JobReporter2.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<JobModel> Jobs { get; set; }
        public ObservableCollection<JobModel> AllJobs { get; set; }

        public RelayCommand OpenFilterCommand { get; }
        public RelayCommand ApplyFilterCommand { get; }

        private FilterViewModel filterViewModel;

        public MainViewModel()
        {
            LoadJobs();
            OpenFilterCommand = new RelayCommand(OpenFilters);
            ApplyFilterCommand = new RelayCommand(ApplyFilters);
        }

        private void LoadJobs()
        {
            try
            {
                // Load dataset from XML
                var dataSet = new DataSet();
                dataSet.ReadXml("C:\\Users\\dveli\\Source\\Repos\\PunkSamurai\\JobReporter2\\JobHistory.xjh");

                // Ensure dataset contains tables
                if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                {
                    AllJobs = new ObservableCollection<JobModel>();
                    Jobs = new ObservableCollection<JobModel>();
                    return;
                }

                // Populate AllJobs from dataset
                AllJobs = new ObservableCollection<JobModel>(
                    dataSet.Tables[0].AsEnumerable().Select(row => new JobModel
                    {
                        Connection = row.Field<string>("Connection"),
                        JobFile = row.Field<string>("JobFile"),
                        EndType = row.Field<string>("EndType"),
                        // PrepTime = row.Field<string>("PrepTime"),
                        StartTime = DateTime.TryParse(row.Field<string>("StartTime"), out var startTime) ? startTime : (DateTime?)null,
                        // EndDate = DateTime.TryParse(row.Field<string>("EndDate"), out var endDate) ? endDate : (DateTime?)null,
                        TotalTime = row.Field<string>("TotalTime"),
                        // Shift = row.Field<string>("Shift") // Assuming a "Shift" column exists
                    })
                );

                // Initialize Jobs with all records
                Jobs = new ObservableCollection<JobModel>(AllJobs);
            }
            catch (Exception ex)
            {
                // Handle exceptions, such as missing file or invalid XML
                MessageBox.Show($"Error loading jobs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AllJobs = new ObservableCollection<JobModel>();
                Jobs = new ObservableCollection<JobModel>();
            }
        }

        private void OpenFilters()
        {
            filterViewModel = new FilterViewModel
            {
                ApplyFilters = ApplyFilters,
                Cancel = () => { /* Close filter window */ }
            };

            var filterWindow = new Window
            {
                Content = new FilterView { DataContext = filterViewModel },
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow
            };

            filterWindow.ShowDialog();
        }

        private void ApplyFilters()
        {
            Jobs.Clear();
            var filteredJobs = AllJobs.Where(job =>
                // (!filterViewModel.StartDate.HasValue || job.StartDate >= filterViewModel.StartDate.Value) &&
                // !filterViewModel.EndDate.HasValue || job.EndDate <= filterViewModel.EndDate.Value) &&
                // (string.IsNullOrEmpty(filterViewModel.SelectedShift) || job.Shift == filterViewModel.SelectedShift) &&
                (string.IsNullOrEmpty(filterViewModel.SelectedEndType) || job.EndType == filterViewModel.SelectedEndType) &&
                (string.IsNullOrEmpty(filterViewModel.SelectedConnection) || job.Connection == filterViewModel.SelectedConnection));

            foreach (var job in filteredJobs)
                Jobs.Add(job);
        }

        private void GenerateReport()
        {
            // Logic to generate a report
        }
    }
}


