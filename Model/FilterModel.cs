using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobReporter2.Model
{
    public class FilterModel : ObservableObject
    {
        public string Name {get; set;}
        public string TimeFrame { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ObservableCollection<string> Connections { get; set; }
        public ObservableCollection<string> EndTypes { get; set; }
        public ObservableCollection<string> Shifts { get; set; }  
    }
}