using JobReporter2.Model;
using System.Collections.ObjectModel;
using System.Data;

namespace JobReporter2.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        // Backing fields
        private ObservableCollection<JobModel> _jobs;
        private JobModel _selectedJob;
        private string _selectedFilter;

        // Properties
        public ObservableCollection<JobModel> Jobs
        {
            get => _jobs;
            set => SetProperty(ref _jobs, value);
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

        // Constructor
        public MainViewModel()
        {
            Jobs = new ObservableCollection<JobModel>();
            OpenFilterCommand = new RelayCommand(OpenFilter);
            GenerateReportCommand = new RelayCommand(GenerateReport);

            // Load jobs
            LoadJobs();
        }

        // Load jobs from XML into ObservableCollection
        private void LoadJobs()
        {
            DataSet dataSet = new DataSet();
            dataSet.ReadXml("C:\\Users\\LENOVO\\source\\repos\\JobReporter2\\JobHistory.xjh");

            DataTable jobTable = dataSet.Tables["Job"];

            foreach (DataRow row in jobTable.Rows)
            {
                Jobs.Add(new JobModel
                {
                    Connection = row["Connection"].ToString(),
                    JobFile = row["Name"].ToString(),
                    EndType = row["EndType"].ToString(),
                    //PrepTime = row["PrepTime"].ToString(),
                    StartTime = row["StartTime"].ToString(),
                    EndTime = row["EndTime"].ToString(),
                    TotalTime = row["TotalTime"].ToString()
                });
            }
        }

        // Command logic
        private void OpenFilter()
        {
            // Logic to open the filter window
        }

        private void GenerateReport()
        {
            // Logic to generate a report
        }
    }
}


