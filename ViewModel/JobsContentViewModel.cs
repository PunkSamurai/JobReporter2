using JobReporter2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobReporter2.ViewModel
{
    public class JobsContentViewModel : ObservableObject
    {
        public ObservableCollection<JobModel> Jobs { get; set; }
        private JobModel _selectedJob;

        public JobModel SelectedJob
        {
            get => _selectedJob;
            set => SetProperty(ref _selectedJob, value);
        }

        public JobsContentViewModel()
        {
            // Example data loading
            Jobs = new ObservableCollection<JobModel>
        {
            new JobModel
            {
                Name = "23CAT PTF V2018_3.ucj",
                StartTime = DateTime.Parse("2023-09-27 08:52:32"),
                EndTime = DateTime.Parse("2023-09-27 09:00:11"),
                TotalTime = TimeSpan.Parse("00:07:39"),
                MachineTime = TimeSpan.Parse("00:07:20"),
                CutTime = TimeSpan.Parse("00:06:15"),
                SlewTime = TimeSpan.Parse("00:00:45"),
                PauseTime = TimeSpan.Parse("00:00:20"),
                SheetChangeTime = TimeSpan.Parse("00:00:19"),
                ToolChangeTime = TimeSpan.Parse("00:00:15"),
                Connection = "ADVA (21163)",
                Material = "_na_",
                PD_Count = 10,
                Size = "41.53 x 142.72",
                FeedrateOverride = 1.0,
                SheetCount = 1,
                Tools = "T1:Drill,T2:EndMill,T3:Router"
            }
        };

            SelectedJob = Jobs.FirstOrDefault();
        }
    }

}
