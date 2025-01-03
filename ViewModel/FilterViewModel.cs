using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace JobReporter2.ViewModel
{
    public class FilterViewModel : ObservableObject
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SelectedShift { get; set; }
        public string SelectedEndType { get; set; }
        public string SelectedConnection { get; set; }

        public ObservableCollection<string> Shifts { get; set; }
        public ObservableCollection<string> EndTypes { get; set; }
        public ObservableCollection<string> Connections { get; set; }

        public Action ApplyFilters { get; set; }
        public Action Cancel { get; set; }

        public FilterViewModel()
        {
            Shifts = new ObservableCollection<string> { "Day", "Night", "Swing" };
            EndTypes = new ObservableCollection<string> { "Type1", "Type2", "Type3" };
            Connections = new ObservableCollection<string> { "Conn1", "Conn2", "Conn3" };
        }

        public void Apply()
        {
            ApplyFilters?.Invoke();
        }

        public void Close()
        {
            Cancel?.Invoke();
        }
    }
}
