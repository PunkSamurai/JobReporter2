using JobReporter2.Model;
using JobReporter2.View;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System;
using System.Linq;

namespace JobReporter2.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        // Backing fields
        private ObservableCollection<JobModel> _allJobs;
        private ObservableCollection<JobModel> _filteredJobs;
        private JobModel _selectedJob;
        private string _selectedFilter;

        // Properties
        public ObservableCollection<JobModel> AllJobs
        {
            get => _allJobs;
            set => SetProperty(ref _allJobs, value);
        }

        public ObservableCollection<JobModel> FilteredJobs
        {
            get => _filteredJobs;
            set => SetProperty(ref _filteredJobs, value);
        }

        public JobModel SelectedJob
        {
            get => _selectedJob;
            set => SetProperty(ref _selectedJob, value);
        }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set => SetProperty(ref _selectedFilter, value);
        }

        // Commands
        public RelayCommand OpenFilterCommand { get; }
        public RelayCommand GenerateReportCommand { get; }

        public HashSet<string> UniqueNames { get; private set; }
        public HashSet<string> UniqueConnections { get; private set; }
        public HashSet<string> UniqueEndTypes { get; private set; }
        public int RecordCount { get; private set; }
        public int FilteredRecordCount { get; private set; }

        // Constructor
        public MainViewModel()
        {
            AllJobs = new ObservableCollection<JobModel>();
            FilteredJobs = new ObservableCollection<JobModel>();


            OpenFilterCommand = new RelayCommand(OpenFilter);
            GenerateReportCommand = new RelayCommand(GenerateReport);

            // Load jobs
            LoadJobs();
        }

        // Load jobs from XML into ObservableCollection
        private void LoadJobs()
        {
            try
            {
                DataSet dataSet = new DataSet();
                // dataSet.ReadXml("C:\\Users\\LENOVO\\source\\repos\\JobReporter2\\JobHistory.xjh");
                dataSet.ReadXml("C:\\Users\\dveli\\Source\\Repos\\PunkSamurai\\JobReporter2\\JobHistory.xjh");

                DataTable jobTable = dataSet.Tables["Job"];
                Dictionary<string, DateTime> machineLastEndTimes = new Dictionary<string, DateTime>();

                // Initialize lists for dropdowns
                UniqueNames = new HashSet<string>();
                UniqueConnections = new HashSet<string>();
                UniqueEndTypes = new HashSet<string>();

                AllJobs = new ObservableCollection<JobModel>(
                    jobTable.AsEnumerable().Select(row =>
                    {
                        string jobFile = row.Table.Columns.Contains("Name") ? row["Name"].ToString() : null;
                        string name = string.IsNullOrEmpty(jobFile)
                            ? null
                            : jobFile.Substring(jobFile.LastIndexOf("\\", StringComparison.Ordinal) + 1);

                        string connection = row.Table.Columns.Contains("Connection") ? row["Connection"].ToString() : null;
                        string endType = row.Table.Columns.Contains("EndType") ? row["EndType"].ToString() : null;

                        DateTime startTime = DateTime.Parse(row["StartTime"].ToString()); // Guaranteed
                        DateTime endTime = DateTime.Parse(row["EndTime"].ToString()); // Guaranteed
                        TimeSpan totalTime = endTime - startTime; // Use TimeSpan for durations

                        // Calculate PrepTime
                        TimeSpan prepTime = TimeSpan.Zero;
                        if (!string.IsNullOrEmpty(connection) && machineLastEndTimes.ContainsKey(connection))
                        {
                            prepTime = startTime - machineLastEndTimes[connection];
                            prepTime = prepTime > TimeSpan.Zero ? prepTime : TimeSpan.Zero; // Ensure no negative prep times
                        }
                        machineLastEndTimes[connection] = endTime; // Update last end time for the machine

                        // Add to unique lists for dropdowns
                        if (!string.IsNullOrEmpty(name)) UniqueNames.Add(name);
                        if (!string.IsNullOrEmpty(connection)) UniqueConnections.Add(connection);
                        if (!string.IsNullOrEmpty(endType)) UniqueEndTypes.Add(endType);

                        return new JobModel
                        {
                            Connection = connection,
                            Name = name,
                            JobFile = jobFile,
                            OEMString = row.Table.Columns.Contains("OEMName") ? row["OEMName"].ToString() : null,
                            Shift = row.Table.Columns.Contains("Shift") ? row["Shift"].ToString() : null,
                            StartType = row.Table.Columns.Contains("StartType") ? row["StartType"].ToString() : null,
                            EndType = endType,
                            PrepTime = prepTime,
                            StartTime = startTime,
                            EndTime = endTime,
                            TotalTime = totalTime,
                            CutTime = row.Table.Columns.Contains("CutTime") && TimeSpan.TryParse(row["CutTime"].ToString(), out TimeSpan cutTime) ? cutTime : (TimeSpan?)null,
                            FeedrateOveride = row.Table.Columns.Contains("FeedrateOveride") && float.TryParse(row["FeedrateOveride"].ToString(), out float feedrate) ? feedrate : 0,
                            SlewTime = row.Table.Columns.Contains("SlewTime") && TimeSpan.TryParse(row["SlewTime"].ToString(), out TimeSpan slewTime) ? slewTime : (TimeSpan?)null,
                            PauseTime = row.Table.Columns.Contains("PauseTime") && TimeSpan.TryParse(row["PauseTime"].ToString(), out TimeSpan pauseTime) ? pauseTime : (TimeSpan?)null,
                            SheetCount = row.Table.Columns.Contains("SheetCount") && float.TryParse(row["SheetCount"].ToString(), out float sheetCount) ? sheetCount : 0,
                            SheetChangeTime = row.Table.Columns.Contains("SheetChangeTime") && TimeSpan.TryParse(row["SheetChangeTime"].ToString(), out TimeSpan sheetChangeTime) ? sheetChangeTime : (TimeSpan?)null,
                            Tools = row.Table.Columns.Contains("Tools") ? row["Tools"].ToString() : null,
                            ToolChangeTime = row.Table.Columns.Contains("ToolChangeTime") && TimeSpan.TryParse(row["ToolChangeTime"].ToString(), out TimeSpan toolChangeTime) ? toolChangeTime : (TimeSpan?)null,
                            ToolAvgTimes = row.Table.Columns.Contains("ToolAvgTimes") && TimeSpan.TryParse(row["ToolAvgTimes"].ToString(), out TimeSpan toolAvgTimes) ? toolAvgTimes : (TimeSpan?)null,
                            Flagged = row.Table.Columns.Contains("Flagged") && bool.TryParse(row["Flagged"].ToString(), out bool flagged) ? flagged : false
                        };
                    })
                );

                // Initialize FilteredJobs with all records
                FilteredJobs = new ObservableCollection<JobModel>(AllJobs);

                // Update the record count for the status bar
                RecordCount = AllJobs.Count;
                FilteredRecordCount = FilteredJobs.Count;
            }
            catch (Exception ex)
            {
                // Handle exceptions, such as missing file or invalid XML
                AllJobs = new ObservableCollection<JobModel>();
                FilteredJobs = new ObservableCollection<JobModel>();
                UniqueNames = new HashSet<string>();
                UniqueConnections = new HashSet<string>();
                UniqueEndTypes = new HashSet<string>();
                RecordCount = 0;
            }
        }



        // Command logic
        private void OpenFilter()
        {
            // Create and configure the FilterView
            var filterViewModel = new FilterViewModel
            {
                StartDate = null,
                EndDate = null,
                AvailableConnections = UniqueConnections.ToList(),
                SelectedConnections = new ObservableCollection<string>(),
                AvailableEndTypes = UniqueEndTypes.ToList(),
                SelectedEndTypes = new ObservableCollection<string>()
            };

            var filterWindow = new FilterView
            {
                DataContext = filterViewModel
            };

            // Show filter window modally
            if (filterWindow.ShowDialog() == true)
            {
                // Apply filters based on the selected criteria
                ApplyFilters(filterViewModel);
            }
        }

        private void ApplyFilters(FilterViewModel filterViewModel)
        {
            var startDate = filterViewModel.StartDate;
            var endDate = filterViewModel.EndDate;
            var selectedConnections = filterViewModel.SelectedConnections;
            var selectedEndTypes = filterViewModel.SelectedEndTypes;

            FilteredJobs = new ObservableCollection<JobModel>(
                AllJobs.Where(job =>
                    (startDate == null || job.StartTime >= startDate) &&
                    (endDate == null || job.EndTime <= endDate) &&
                    (!selectedConnections.Any() || selectedConnections.Contains(job.Connection)) &&
                    (!selectedEndTypes.Any() || selectedEndTypes.Contains(job.EndType))
                )
            );
            FilteredRecordCount = FilteredJobs.Count;
        }

        private void GenerateReport()
        {
            // Logic to generate a report
        }
    }
}

