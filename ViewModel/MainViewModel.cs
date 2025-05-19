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
using Microsoft.Win32;
using OxyPlot;
using iTextSharp;

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
        private string _filePath;
        private int _recordCount;
        private int _filteredRecordCount;
        private int _selectedTabIndex;

        private ObservableCollection<ShiftModel> _shifts;
        private ObservableCollection<ThresholdModel> _thresholds;
        private ShiftModel _selectedShift;

        // Properties
        public ObservableCollection<TabItem> Tabs { get; } = new ObservableCollection<TabItem>();
        public ObservableCollection<string> ReportTypes { get; } = new ObservableCollection<string>
        {
            "Advanced Shift Productivity (Time)",
            "Shift Productivity (Time)",
            "Shift Productivity (Number of Jobs)",
            "Text Report"
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

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged(nameof(SelectedTabIndex));
                }
            }
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
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }
        public ObservableCollection<ShiftModel> Shifts
        {
            get => _shifts;
            set => SetProperty(ref _shifts, value);
        }

        public ObservableCollection<ThresholdModel> Thresholds
        {
            get => _thresholds;
            set => SetProperty(ref _thresholds, value);
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

        public RelayCommand ExportToCsvCommand { get; }

        public HashSet<string> UniqueNames { get; private set; }
        public HashSet<string> UniqueConnections { get; private set; }
        public HashSet<string> UniqueEndTypes { get; private set; }
        public HashSet<string> UniqueShifts { get; private set; }

        // Constructor
        public JobViewModel JobViewModel { get; }
        public MainViewModel()
        {

            AllJobs = new ObservableCollection<JobModel>();
            FilteredJobs = new ObservableCollection<JobModel>();
            Shifts = new ObservableCollection<ShiftModel>();
            Shifts = SettingsHelper.LoadShifts();
            Thresholds = SettingsHelper.LoadThresholds();
            foreach (var threshold in Thresholds)
            {
                Console.WriteLine(threshold.Name + " " + threshold.Unit);
            }
            Console.WriteLine("SHIFTS LOADED");
            UniqueShifts = new HashSet<string>();
            SelectedReportType = ReportTypes.FirstOrDefault();
            SelectedTimeFrame = TimeFrames.FirstOrDefault();
            foreach (var shift in Shifts)
            {
                UniqueShifts.Add(shift.Name);
                Console.WriteLine("SHIFT:" + shift);
            }

            SelectedFilter = "No filters applied";

            OpenFilterCommand = new RelayCommand(OpenFilter);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            GenerateReportCommand = new RelayCommand(GenerateReport);
            OpenShiftManagerCommand = new RelayCommand(OpenShiftManager);
            ExportToCsvCommand = new RelayCommand(ExportToCsv);
            LoadJobs();
            var jobsContent = new JobsContent { Jobs = FilteredJobs };
            JobViewModel = new JobViewModel { DataGrid = jobsContent.JobDataGrid, Jobs = FilteredJobs };
            var jobsTab = new TabItem
            {
                Header = "Jobs",
                Content = jobsContent
            };
            Tabs.Add(jobsTab);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => jobsTab.IsSelected = true), DispatcherPriority.Loaded);
            //jobsTab.IsSelected = true;
        }

        private void ExportToCsv()
        {
            Console.WriteLine("Exporting to CSV...");
            try
            {
                string previousCsvPath = SettingsHelper.LoadCsvDirectory();

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    Title = "Export Jobs to CSV",
                    DefaultExt = "csv",
                    FileName = "JobExport_" + DateTime.Now.ToString("yyyyMMdd")
                };

                if (!string.IsNullOrEmpty(previousCsvPath))
                {
                    saveFileDialog.InitialDirectory = previousCsvPath;
                }

                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string csvPath = saveFileDialog.FileName;
                    SettingsHelper.SaveCsvDirectory(Path.GetDirectoryName(csvPath));

                    using (StreamWriter writer = new StreamWriter(csvPath))
                    {
                        // Write header
                        // Write header
                        writer.WriteLine(
                            "Connection,Name,JobFile,StartTime,EndTime,TotalTime,MachineTime,PauseTime,WastedTime," +
                            "Shift,StartType,EndType,PrepTime,Flagged,OEMString,FileSize,CutTime,Length,FeedrateOverride," +
                            "SlewTime,SheetCount,TimeEstimate,SheetChangeTime,Tools,ToolChangeTime,ToolAvgTimes,Size,ShortenedStartType");

                        foreach (var job in FilteredJobs)
                        {
                            writer.WriteLine(
                                $"{EscapeCsvField(job.Connection)}," +
                                $"{EscapeCsvField(job.Name)}," +
                                $"{EscapeCsvField(job.JobFile)}," +
                                $"{job.StartTime:yyyy-MM-dd HH:mm:ss}," +
                                $"{job.EndTime:yyyy-MM-dd HH:mm:ss}," +
                                $"{FormatTimeSpan(job.TotalTime)}," +
                                $"{FormatTimeSpan(job.MachineTime)}," +
                                $"{FormatTimeSpan(job.PauseTime)}," +
                                $"{FormatTimeSpan(job.WastedTime)}," +
                                $"{EscapeCsvField(job.Shift)}," +
                                $"{EscapeCsvField(job.StartType)}," +
                                $"{EscapeCsvField(job.EndType)}," +
                                $"{FormatTimeSpan(job.PrepTime)}," +
                                $"{job.Flagged}," +
                                $"{EscapeCsvField(job.OEMString)}," +
                                $"{job.FileSize}," +
                                $"{FormatTimeSpan(job.CutTime)}," +
                                $"{job.Length}," +
                                $"{job.FeedrateOverride}," +
                                $"{FormatTimeSpan(job.SlewTime)}," +
                                $"{job.SheetCount}," +
                                $"{FormatTimeSpan(job.TimeEstimate)}," +
                                $"{FormatTimeSpan(job.SheetChangeTime)}," +
                                $"{EscapeCsvField(job.Tools)}," +
                                $"{FormatTimeSpan(job.ToolChangeTime)}," +
                                $"{FormatTimeSpan(job.ToolAvgTimes)}," +
                                $"{EscapeCsvField(job.Size)}," +
                                $"{EscapeCsvField(job.ShortenedStartType)}");
                        }
                    }

                    MessageBox.Show($"Successfully exported {FilteredJobs.Count} jobs to:\n{csvPath}",
                                   "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Export Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper methods for CSV export
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // If the field contains comma, quotes, or newline, wrap it in quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                // Double up any quotes
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }
            return field;
        }

        private string FormatTimeSpan(TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue)
                return "";

            // Format as hours:minutes:seconds
            return $"{(int)timeSpan.Value.TotalHours}:{timeSpan.Value.Minutes:D2}:{timeSpan.Value.Seconds:D2}";
        }

        private void AssignShiftsToJobs()
        {
            foreach (var job in AllJobs)
            {
                var shift = Shifts.FirstOrDefault(s => s.IsEnabled && s.IsInShift(job.StartTime));
                if (shift != null)
                {
                    job.Shift = shift.Name;
                    Console.WriteLine($"Job {job.Name} assigned to {shift.Name}");
                }
                else
                {
                    job.Shift = "Unassigned";
                    Console.WriteLine($"Job {job.Name} has no valid shift.");
                }
            }

            CalculatePrepTimes();
            FilteredJobs = new ObservableCollection<JobModel>(FilteredJobs);
            OnPropertyChanged(nameof(FilteredJobs));
        }

        
        private void CalculatePrepTimes()
        {
            var groupedJobs = AllJobs
                .Where(job => job != null)
                .GroupBy(job => job.Connection)
                .ToList();

            foreach (var group in groupedJobs)
            {
                var jobsInGroup = group.OrderBy(job => job.StartTime).ToList();

                for (int i = 0; i < jobsInGroup.Count; i++)
                {
                    var currentJob = jobsInGroup[i];
                    TimeSpan prepFromShiftStart = TimeSpan.MaxValue;
                    TimeSpan prepFromPreviousJob = TimeSpan.MaxValue;
                    if (!string.IsNullOrEmpty(currentJob.Shift))
                    {
                        var shift = Shifts.FirstOrDefault(s => s.Name == currentJob.Shift);
                        if (shift != null)
                        {
                            prepFromShiftStart = currentJob.StartTime.TimeOfDay - shift.StartTime;
                        }
                    }
                    if (i > 0)
                    {
                        var previousJob = jobsInGroup[i - 1];
                        prepFromPreviousJob = currentJob.StartTime - previousJob.EndTime;
                    }

                    currentJob.PrepTime = new[] { prepFromShiftStart, prepFromPreviousJob }
                        .Where(ts => ts != TimeSpan.MaxValue)
                        .DefaultIfEmpty(TimeSpan.Zero)
                        .Min();

                    currentJob.PrepTime = currentJob.PrepTime > TimeSpan.Zero ? currentJob.PrepTime : TimeSpan.Zero;
                    currentJob.Flagged = currentJob.CalculateFlagged(Thresholds[0].Value1, Thresholds[1].Value2);
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

            var thresholdCopies = new ObservableCollection<ThresholdModel>(
                Thresholds.Select(t => new ThresholdModel
                {
                    Name = t.Name,
                    IsEnabled = t.IsEnabled,
                    Value1 = t.Value1,
                    Value2 = t.Value2,
                    Unit = t.Unit
                })
            );

            var shiftManagerViewModel = new ShiftManagerViewModel
            {
                Shifts = shiftCopies,
                Thresholds = thresholdCopies
            };

            var shiftManagerView = new ShiftManagerView
            {
                DataContext = shiftManagerViewModel,
                Owner = Application.Current.MainWindow
            };

            if (shiftManagerView.ShowDialog() == true)
            {
                Console.WriteLine("update");
                Shifts = new ObservableCollection<ShiftModel>(
                    shiftManagerViewModel.Shifts.Select(s => new ShiftModel
                    {
                        Name = s.Name,
                        IsEnabled = s.IsEnabled,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime
                    })
                );
                UniqueShifts.Clear();

                Thresholds = new ObservableCollection<ThresholdModel>(
                    shiftManagerViewModel.Thresholds.Select(t => new ThresholdModel
                    {
                        Name = t.Name,
                        IsEnabled = t.IsEnabled,
                        Value1 = t.Value1,
                        Value2 = t.Value2,
                        Unit = t.Unit
                    })
                );

                foreach (var shift in Shifts)
                {
                    UniqueShifts.Add(shift.Name);
                    Console.WriteLine(shift.Name);
                }

                foreach (var threshold in Thresholds)
                {
                    Console.WriteLine(threshold.Name + " " + threshold.Value1 + " " + threshold.Value2);
                }

                AssignShiftsToJobs();
            }
        }

        private void LoadJobs()
        {
            try
            {
                // First try loading from command line arguments
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1 && File.Exists(args[1]) && args[1].EndsWith(".xjh", StringComparison.OrdinalIgnoreCase))
                {
                    FilePath = args[1];
                    SettingsHelper.SaveXjhDirectory(FilePath); 

                    // DataSet dataSet = new DataSet();
                    // dataSet.ReadXml(FilePath);
                    DataSet dataSet = XjhParser.ReadXjh(FilePath);
                    ProcessDataSet(dataSet);
                    return; 
                }
                string previousFilePath = SettingsHelper.LoadXjhDirectory();

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "XML Job History files (*.xjh)|*.xjh|All files (*.*)|*.*",
                    Title = "Select Job History File"
                };

                if (!string.IsNullOrEmpty(previousFilePath) && File.Exists(previousFilePath))
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(previousFilePath);
                    openFileDialog.FileName = Path.GetFileName(previousFilePath);
                }
                else
                {
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }

                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    FilePath = openFileDialog.FileName;
                    SettingsHelper.SaveXjhDirectory(FilePath);

                    // DataSet dataSet = new DataSet();
                    // dataSet.ReadXml(FilePath);
                    DataSet dataSet = XjhParser.ReadXjh(FilePath);
                    ProcessDataSet(dataSet);
                }
                openFileDialog = null;
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
                FilteredRecordCount = 0;

                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        private void ProcessDataSet(DataSet dataSet)
        {
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
                    TimeSpan machineTime = row.Table.Columns.Contains("MachineTime") && TimeSpan.TryParse(row["MachineTime"].ToString(), out TimeSpan machineTime2) ? machineTime2 : TimeSpan.Zero;

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
                        MachineTime = machineTime,
                        FileSize = row.Table.Columns.Contains("FileSize") && int.TryParse(row["FileSize"].ToString(), out int fileSize) ? fileSize : (int?)null,
                        CutTime = row.Table.Columns.Contains("CutTime") && TimeSpan.TryParse(row["CutTime"].ToString(), out TimeSpan cutTime) ? cutTime : machineTime,
                        Length = row.Table.Columns.Contains("Length") && float.TryParse(row["Length"].ToString(), out float length) ? length : (float?)null,
                        FeedrateOverride = row.Table.Columns.Contains("FeedrateOveride") && float.TryParse(row["FeedrateOveride"].ToString(), out float feedrate) ? feedrate : (float?)null,
                        SlewTime = row.Table.Columns.Contains("SlewTime") && TimeSpan.TryParse(row["SlewTime"].ToString(), out TimeSpan slewTime) ? slewTime : (TimeSpan?)null,
                        PauseTime = row.Table.Columns.Contains("PauseTime") && TimeSpan.TryParse(row["PauseTime"].ToString(), out TimeSpan pauseTime) ? pauseTime : totalTime - machineTime,
                        SheetCount = row.Table.Columns.Contains("SheetCount") && float.TryParse(row["SheetCount"].ToString(), out float sheetCount) ? sheetCount : (float?)null,
                        TimeEstimate = row.Table.Columns.Contains("TimeEstimate") && TimeSpan.TryParse(row["TimeEstimate"].ToString(), out TimeSpan timeEstimate) ? timeEstimate : (TimeSpan?)null,
                        SheetChangeTime = row.Table.Columns.Contains("SheetChangeTime") && TimeSpan.TryParse(row["SheetChangeTime"].ToString(), out TimeSpan sheetChangeTime) ? sheetChangeTime : (TimeSpan?)null,
                        Tools = row.Table.Columns.Contains("Tools") ? row["Tools"].ToString() : null,
                        ToolChangeTime = row.Table.Columns.Contains("ToolChangeTime") && TimeSpan.TryParse(row["ToolChangeTime"].ToString(), out TimeSpan toolChangeTime) ? toolChangeTime : (TimeSpan?)null,
                        ToolAvgTimes = row.Table.Columns.Contains("ToolAvgTimes") && TimeSpan.TryParse(row["ToolAvgTimes"].ToString(), out TimeSpan toolAvgTimes) ? toolAvgTimes : (TimeSpan?)null,
                        Size = row.Table.Columns.Contains("Size") ? row["Size"].ToString() : null
                    };
                    // job.Flagged = job.CalculateFlagged();
                    job.WastedTime = job.TotalTime - job.MachineTime;
                    job.GeneratePieChart();
                    job.PreviewImagePath = job.GetPreviewImagePath();
                    job.ShortenedStartType = job.FindShortenedStartType();
                    Console.WriteLine($"PREVIEW IMAGE PATH: {job.PreviewImagePath}");
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



        // Command logic
        private void OpenFilter()
        {
            // Create and configure the FilterView
            var filterViewModel = new FilterViewModel
            {
                StartDate = null,
                EndDate = null,
                TimeFrame = "Custom",
                AvailableConnections = UniqueConnections.ToList(),
                SelectedConnections = new ObservableCollection<string>(),
                AvailableEndTypes = UniqueEndTypes.ToList(),
                SelectedEndTypes = new ObservableCollection<string>(),
                AvailableShifts = UniqueShifts.ToList(),
                SelectedShifts = new ObservableCollection<string>(),
                FlaggedStatus = "All"
            };

            var filterWindow = new FilterView
            {
                DataContext = filterViewModel,
                Owner = Application.Current.MainWindow
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
            //JobViewModel.UpdateVisibleColumns();
            SelectedFilter = "No filters applied";

        }

        private void ApplyFilters(FilterViewModel filterViewModel)
        {
            var startDate = filterViewModel.StartDate;
            var endDate = filterViewModel.EndDate;
            var selectedConnections = filterViewModel.SelectedConnections;
            var selectedEndTypes = filterViewModel.SelectedEndTypes;
            var selectedShifts = filterViewModel.SelectedShifts;
            var flaggedStatus = filterViewModel.FlaggedStatus;

            FilteredJobs = new ObservableCollection<JobModel>(
                AllJobs.Where(job =>
                    (startDate == null || job.StartTime >= startDate) &&
                    (endDate == null || job.EndTime <= endDate) &&
                    (!selectedConnections.Any() || selectedConnections.Contains(job.Connection)) &&
                    (!selectedEndTypes.Any() || selectedEndTypes.Contains(job.EndType)) &&
                    (!selectedShifts.Any() || selectedShifts.Contains(job.Shift)) &&
                    (flaggedStatus == "All" ||
                     (flaggedStatus == "Flagged" && job.Flagged) ||
                     (flaggedStatus == "Unflagged" && !job.Flagged))
                )
            );

            FilteredRecordCount = FilteredJobs.Count;
            JobViewModel.Jobs = FilteredJobs;

            var filters = new List<string>();

            // First line: combine date range and flag status
            var firstLineItems = new List<string>();
            if (startDate.HasValue) firstLineItems.Add($"Start: {startDate.Value.ToShortDateString()}");
            if (endDate.HasValue) firstLineItems.Add($"End: {endDate.Value.ToShortDateString()}");
            if (flaggedStatus != "All") firstLineItems.Add($"Status: {flaggedStatus}");

            if (firstLineItems.Any())
                filters.Add(string.Join("   |   ", firstLineItems));

            // Add remaining filters, each on their own line
            if (selectedShifts.Any())
                filters.Add($"Shifts: {string.Join(", ", selectedShifts)}");

            if (selectedConnections.Any())
                filters.Add($"Connections: {string.Join(", ", selectedConnections)}");

            if (selectedEndTypes.Any())
                filters.Add($"End Types: {string.Join(", ", selectedEndTypes)}");

            SelectedFilter = filters.Any() ? string.Join("\n", filters) : "No filters applied";
        }

        private void GenerateReport()
        {
            try
            {
                var parametersViewModel = new ReportParametersViewModel(
                    ReportTypes.ToList(),  // Pass the available report types
                    TimeFrames.ToList(),    // Pass the available time frames
                    Tabs.Count
                );

                var parametersView = new ReportParametersView
                {
                    DataContext = parametersViewModel,
                    Owner = Application.Current.MainWindow
                };

                bool? result = parametersView.ShowDialog();
                if (result != true)
                    return;

                string reportTitle = parametersViewModel.ReportTitle;
                string selectedReportType = parametersViewModel.SelectedReportType;
                string selectedTimeFrame = parametersViewModel.SelectedTimeFrame;
                int reportNumber = Tabs.Count;
                var reportContent = new ReportContent();

                var reportModel = ReportFactory.GenerateReport(FilteredJobs, selectedReportType, selectedTimeFrame);
                reportContent.ReportModel = reportModel;

                var reportTab = new TabItem
                {
                    Content = reportContent,
                    HeaderTemplate = Application.Current.Resources["CloseableTabHeaderTemplate"] as DataTemplate
                };

                var header = new ReportTabHeader(
                    reportTitle,  // Use the custom title from the dialog
                    h => Tabs.Remove(reportTab), // This is the close action
                    h => ExportReportToPdf(reportContent, reportTitle)  // Pass title for PDF
                );

                reportTab.Header = header;
                Tabs.Add(reportTab);
                SelectedTabIndex = Tabs.Count - 1;
            }
            catch (Exception ex)
            {
                // Show error message
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportReportToPdf(ReportContent reportContent, string reportTitle)
        {
            string PreviousReportPath = SettingsHelper.LoadReportDirectory();
            try
            {
                // Create SaveFileDialog
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    Title = "Save Report as PDF",
                    FileName = reportTitle
                };

                if (PreviousReportPath != "")
                {
                    saveFileDialog.InitialDirectory = PreviousReportPath;
                }

                // Show the dialog and process the result
                if (saveFileDialog.ShowDialog() == true)
                {
                    string fileName = saveFileDialog.FileName;
                    using (var stream = File.Create(fileName))
                    {
                        if (reportContent.ReportModel is string textContent)
                        {
                            // Export text report
                            ExportTextReportToPdf(textContent, reportTitle, stream);
                        }
                        else if (reportContent.ReportModel is PlotModel plotModel)
                        {
                            // Export plot model report
                            var pdfExporter = new OxyPlot.SkiaSharp.PdfExporter { Width = 1920, Height = 1080 };
                            pdfExporter.Export(plotModel, stream);
                        }
                    }

                    SettingsHelper.SaveReportDirectory(Path.GetDirectoryName(fileName));
                    // Optionally show success message
                    MessageBox.Show($"Report successfully exported to:\n{fileName}", "Export Successful",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportTextReportToPdf(string textContent, string title, Stream stream)
        {
            // Add a PDF library like iTextSharp via NuGet if you don't have one already
            using (var document = new iTextSharp.text.Document())
            {
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);
                document.Open();

                // Add title
                var titleFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.COURIER_BOLD, 14);
                var titleParagraph = new iTextSharp.text.Paragraph(title, titleFont);
                document.Add(titleParagraph);

                // Add current date
                var dateFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.COURIER, 10);
                var dateParagraph = new iTextSharp.text.Paragraph(DateTime.Now.ToShortDateString(), dateFont);
                document.Add(dateParagraph);

                // Add content with monospaced font to preserve formatting
                var contentFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.COURIER, 10);
                var contentParagraph = new iTextSharp.text.Paragraph(textContent, contentFont);
                document.Add(contentParagraph);

                document.Close();
            }
        }
    }
}

