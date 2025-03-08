using JobReporter2.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace JobReporter2.ViewModel
{
    public class JobViewModel : ObservableObject
    {
        private ObservableCollection<JobModel> _jobs;
        private DataGrid _dataGrid;

        public ObservableCollection<JobModel> Jobs
        {
            get => _jobs;
            set => SetProperty(ref _jobs, value);
        }

        public DataGrid DataGrid
        {
            get => _dataGrid;
            set => SetProperty(ref _dataGrid, value);
        }

        public JobViewModel()
        {
            Jobs = new ObservableCollection<JobModel>();
        }

        public void UpdateVisibleColumns()
        {
            if (DataGrid == null || Jobs == null || !Jobs.Any())
                return;

            if (Jobs.Count == 1)
                return;

            foreach (var column in DataGrid.Columns)
            {
                if (column is DataGridBoundColumn boundColumn && boundColumn.Binding is Binding binding)
                {
                    var bindingPath = binding.Path.Path;
                    if (string.IsNullOrEmpty(bindingPath))
                        continue;

                    var distinctValues = Jobs
                        .Select(job => typeof(JobModel).GetProperty(bindingPath)?.GetValue(job))
                        .Distinct()
                        .ToList();

                    column.Visibility = distinctValues.Count <= 1
                        ? System.Windows.Visibility.Collapsed
                        : System.Windows.Visibility.Visible;
                }
            }
        }

    }
}
