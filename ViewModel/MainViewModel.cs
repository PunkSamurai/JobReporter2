using JobReporter2.Model;
using JobReporter2.View;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using JobReporter2.Helpers;
using System.Windows.Controls;
using System.Windows;
using System.Threading.Tasks;
using OxyPlot.Series;
using System.Windows.Threading;

namespace JobReporter2.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        // Backing fields
        private ObservableCollection<JobModel> _allJobs;
        private ObservableCollection<JobModel> _filteredJobs;
        private JobModel _selectedJob;
        private string _selectedFilter;
        private string _selectedReportType;
        private string _selectedTimeFrame;
        private int _recordCount;
        private int _filteredRecordCount;

        private ObservableCollection<ShiftModel> _shifts;
        private ShiftModel _selectedShift;

        // Properties
        public ObservableCollection<TabItem> Tabs { get; } = new ObservableCollection<TabItem>();
        public ObservableCollection<string> ReportTypes { get; } = new ObservableCollection<string>
        {
            "Shift Productivity (Time)",
            "Shift Productivity (Number of Jobs)"
        };

        public ObservableCollection<string> TimeFrames { get; } = new ObservableCollection<string>
        {
            "Daily",
            "Weekly",
            "Monthly",
            "Yearly"
        };

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

        public int RecordCount
        {
            get => _recordCount;
            set => SetProperty(ref _recordCount, value);
        }

        public int FilteredRecordCount
        {
            get => _filteredRecordCount;
            set => SetProperty(ref _filteredRecordCount, value);
        }

        public ObservableCollection<ShiftModel> Shifts
        {
            get => _shifts;
            set => SetProperty(ref _shifts, value);
        }

        public ShiftModel SelectedShift
        {
            get => _selectedShift;
            set => SetProperty(ref _selectedShift, value);
        }

        // Commands
        public RelayCommand OpenFilterCommand { get; }
        public RelayCommand ClearFilterCommand { get; }
        public RelayCommand GenerateReportCommand { get; }
        public RelayCommand OpenShiftManagerCommand { get; }

        public HashSet<string> UniqueNames { get; private set; }
        public HashSet<string> UniqueConnections { get; private set; }
        public HashSet<string> UniqueEndTypes { get; private set; }

        // Constructor
        public JobViewModel JobViewModel { get; }
        public MainViewModel()
        {
            
            AllJobs = new ObservableCollection<JobModel>();
            FilteredJobs = new ObservableCollection<JobModel>();
            Shifts = new ObservableCollection<ShiftModel>();
            Shifts = SettingsHelper.LoadShifts();
            Console.WriteLine(Shifts);
            SelectedFilter = "No filters applied";

            OpenFilterCommand = new RelayCommand(OpenFilter);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            GenerateReportCommand = new RelayCommand(GenerateReport);
            OpenShiftManagerCommand = new RelayCommand(OpenShiftManager);
            

            // Load jobs
            LoadJobs();
            var jobsContent = new JobsContent();
            JobViewModel = new JobViewModel { DataGrid = jobsContent.JobDataGrid, Jobs = FilteredJobs};
            //jobsContent.DataContext = JobViewModel;

            var jobsTab = new TabItem
            {
                Header = "Jobs",
                Content = jobsContent
            };
            Tabs.Add(jobsTab);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => jobsTab.IsSelected = true), DispatcherPriority.Loaded);
            //jobsTab.IsSelected = true;
        }

        private void AssignShiftsToJobs()
        {
            foreach (var job in AllJobs)
            {
                // Find the first enabled shift that contains the job's start time
                var shift = Shifts.FirstOrDefault(s => s.IsEnabled && s.IsInShift(job.StartTime)); // ?? Shifts.FirstOrDefault(s => s.IsEnabled);

                if (shift != null)
                {
                    job.Shift = shift.Name; // Assign the shift name to the existing Shift field
                    Console.WriteLine($"Job {job.Name} assigned to {shift.Name}");
                }
                else
                {
                    job.Shift = "Unassigned"; // If no shift matches or all are disabled
                    Console.WriteLine($"Job {job.Name} has no valid shift.");
                }
            }

            CalculatePrepTimes();

            // Update the filtered jobs collection and notify UI
            FilteredJobs = new ObservableCollection<JobModel>(FilteredJobs);
            OnPropertyChanged(nameof(FilteredJobs));
        }

        private void CalculatePrepTimes()
        {
            // Group jobs by machine and shift
            var groupedJobs = AllJobs
                .Where(job => !string.IsNullOrEmpty(job.Shift)) // Ensure the job has a shift assigned
                .GroupBy(job => new { job.Connection, job.Shift })
                .ToList();

            foreach (var group in groupedJobs)
            {
                var jobsInGroup = group.OrderBy(job => job.StartTime).ToList();

                for (int i = 0; i < jobsInGroup.Count; i++)
                {
                    var currentJob = jobsInGroup[i];

                    if (i == 0)
                    {
                        // First job in the group; calculate prep time relative to shift start
                        var shift = Shifts.FirstOrDefault(s => s.Name == currentJob.Shift);
                        if (shift != null)
                        {
                            currentJob.PrepTime = currentJob.StartTime.TimeOfDay - shift.StartTime;
                            currentJob.PrepTime = currentJob.PrepTime > TimeSpan.Zero ? currentJob.PrepTime : TimeSpan.Zero;
                        }
                        else
                        {
                            currentJob.PrepTime = TimeSpan.Zero; // No valid shift
                        }
                    }
                    else
                    {
                        // Calculate prep time relative to the previous job's end time
                        var previousJob = jobsInGroup[i - 1];

                        if (currentJob.StartTime.Date == previousJob.EndTime.Date)
                        {
                            // Same day, calculate prep time normally
                            currentJob.PrepTime = currentJob.StartTime - previousJob.EndTime;
                        }
                        else
                        {
                            // Different day, calculate prep time relative to shift start
                            var shift = Shifts.FirstOrDefault(s => s.Name == currentJob.Shift);
                            if (shift != null)
                            {
                                currentJob.PrepTime = currentJob.StartTime.TimeOfDay - shift.StartTime;
                            }
                            else
                            {
                                currentJob.PrepTime = TimeSpan.Zero; // No valid shift
                            }
                        }

                        // Ensure prep time is not negative
                        currentJob.PrepTime = currentJob.PrepTime > TimeSpan.Zero ? currentJob.PrepTime : TimeSpan.Zero;
                    }
                }
            }
        }


        private void OpenShiftManager()
        {
            // Create deep copies of the shifts for editing
            var shiftCopies = new ObservableCollection<ShiftModel>(
                Shifts.Select(s => new ShiftModel
                {
                    Name = s.Name,
                    IsEnabled = s.IsEnabled,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                })
            );

            var shiftManagerViewModel = new ShiftManagerViewModel
            {
                Shifts = shiftCopies
            };

            var shiftManagerView = new ShiftManagerView
            {
                DataContext = shiftManagerViewModel
            };

            if (shiftManagerView.ShowDialog() == true)
            {
                Console.WriteLine("update");
                // Only update the main collection if OK was clicked
                Shifts = new ObservableCollection<ShiftModel>(
                    shiftManagerViewModel.Shifts.Select(s => new ShiftModel
                    {
                        Name = s.Name,
                        IsEnabled = s.IsEnabled,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime
                    })
                );
                AssignShiftsToJobs();
            }
        }

        // Load jobs from XML into ObservableCollection
        private void LoadJobs()
        {
            try
            {
                DataSet dataSet = new DataSet();
                // dataSet.ReadXml("C:\\Users\\LENOVO\\source\\repos\\PunkSamurai\\JobReporter2\\JobHistory.xjh");
                dataSet.ReadXml("C:\\Users\\LENOVO\\source\\repos\\PunkSamurai\\JobReporter2\\JobHistory2.xjh");
                // dataSet.ReadXml("C:\\Users\\dveli\\Source\\Repos\\PunkSamurai\\JobReporter2\\JobHistory.xjh");
                // dataSet.ReadXml("C:\\Users\\dveli\\Source\\Repos\\PunkSamurai\\JobReporter2\\JobHistory2.xjh");

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
                            : Path.GetFileName(jobFile);

                        string connection = row.Table.Columns.Contains("Connection") ? row["Connection"].ToString() : null;
                        string endType = row.Table.Columns.Contains("EndType") ? row["EndType"].ToString() : null;

                        DateTime startTime = DateTime.Parse(row["StartTime"].ToString()); // Guaranteed
                        DateTime endTime = DateTime.Parse(row["EndTime"].ToString()); // Guaranteed
                        TimeSpan totalTime = endTime - startTime; // Use TimeSpan for durations

                        // Calculate PrepTime
                        /* TimeSpan prepTime = TimeSpan.Zero;
                        if (!string.IsNullOrEmpty(connection) && machineLastEndTimes.ContainsKey(connection))
                        {
                            prepTime = startTime - machineLastEndTimes[connection];
                            prepTime = prepTime > TimeSpan.Zero ? prepTime : TimeSpan.Zero; // Ensure no negative prep times
                        }
                        machineLastEndTimes[connection] = endTime; // Update last end time for the machine
                        */

                        // Add to unique lists for dropdowns
                        if (!string.IsNullOrEmpty(name)) UniqueNames.Add(name);
                        if (!string.IsNullOrEmpty(connection)) UniqueConnections.Add(connection);
                        if (!string.IsNullOrEmpty(endType)) UniqueEndTypes.Add(endType);

                        var job = new JobModel
                        {
                            Connection = connection,
                            Name = name,
                            JobFile = jobFile,
                            OEMString = row.Table.Columns.Contains("OEMName") ? row["OEMName"].ToString() : null,
                            StartType = row.Table.Columns.Contains("StartType") ? row["StartType"].ToString() : null,
                            EndType = endType,
                            // PrepTime = prepTime,
                            StartTime = startTime,
                            EndTime = endTime,
                            TotalTime = totalTime,
                            MachineTime = row.Table.Columns.Contains("MachineTime") && TimeSpan.TryParse(row["MachineTime"].ToString(), out TimeSpan machineTime) ? machineTime : (TimeSpan?)null,
                            FeedrateOverride = row.Table.Columns.Contains("FeedrateOveride") && float.TryParse(row["FeedrateOveride"].ToString(), out float feedrate) ? feedrate : 0,
                            SlewTime = row.Table.Columns.Contains("SlewTime") && TimeSpan.TryParse(row["SlewTime"].ToString(), out TimeSpan slewTime) ? slewTime : (TimeSpan?)null,
                            PauseTime = row.Table.Columns.Contains("PauseTime") && TimeSpan.TryParse(row["PauseTime"].ToString(), out TimeSpan pauseTime) ? pauseTime : (TimeSpan?)null,
                            SheetCount = row.Table.Columns.Contains("SheetCount") && float.TryParse(row["SheetCount"].ToString(), out float sheetCount) ? sheetCount : 0,
                            SheetChangeTime = row.Table.Columns.Contains("SheetChangeTime") && TimeSpan.TryParse(row["SheetChangeTime"].ToString(), out TimeSpan sheetChangeTime) ? sheetChangeTime : (TimeSpan?)null,
                            Tools = row.Table.Columns.Contains("Tools") ? row["Tools"].ToString() : null,
                            ToolChangeTime = row.Table.Columns.Contains("ToolChangeTime") && TimeSpan.TryParse(row["ToolChangeTime"].ToString(), out TimeSpan toolChangeTime) ? toolChangeTime : (TimeSpan?)null,
                            ToolAvgTimes = row.Table.Columns.Contains("ToolAvgTimes") && TimeSpan.TryParse(row["ToolAvgTimes"].ToString(), out TimeSpan toolAvgTimes) ? toolAvgTimes : (TimeSpan?)null,
                            Size = row.Table.Columns.Contains("Size") ? row["Size"].ToString() : null,
                            Flagged = row.Table.Columns.Contains("Flagged") && bool.TryParse(row["Flagged"].ToString(), out bool flagged) ? flagged : false
                        };

                        job.WastedTime = job.TotalTime - job.MachineTime;
                        job.GeneratePieChart();

                        return job;
                    })
                );

                AssignShiftsToJobs();

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

        private void ClearFilter()
        {
            FilteredJobs = AllJobs;
            FilteredRecordCount = AllJobs.Count;
            JobViewModel.Jobs = FilteredJobs;
            JobViewModel.UpdateVisibleColumns();

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
            JobViewModel.Jobs = FilteredJobs;
            JobViewModel.UpdateVisibleColumns();
            Console.WriteLine(FilteredRecordCount);
            var filters = new List<string>();
            if (startDate.HasValue) filters.Add($"Start Date: {startDate.Value.ToShortDateString()}");
            if (endDate.HasValue) filters.Add($"End Date: {endDate.Value.ToShortDateString()}");
            if (selectedConnections.Any()) filters.Add($"Connections: {string.Join(", ", selectedConnections)}");
            if (selectedEndTypes.Any()) filters.Add($"End Types: {string.Join(", ", selectedEndTypes)}");

            SelectedFilter = filters.Any() ? string.Join("\n", filters) : "No filters applied";
        }

        private bool CanGenerateReport()
        {
            return !string.IsNullOrEmpty(SelectedReportType) && !string.IsNullOrEmpty(SelectedTimeFrame);
        }
        private void GenerateReport()
        {
            try
            {
                // Create a new tab
                var reportTab = new TabItem
                {
                    Header = $"Report {Tabs.Count}"
                };

                // Create and configure the ReportContent
                var reportContent = new ReportContent
                {
                    ReportModel = ReportFactory.GenerateReport(FilteredJobs, SelectedReportType, SelectedTimeFrame) // Generate the PlotModel for the report
                };

                // Set the ReportContent as the content of the tab
                reportTab.Content = reportContent;

                // Add the tab to the TabControl
                Tabs.Add(reportTab);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}

