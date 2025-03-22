using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobReporter2.Model
{
    public class SettingsModel
    {
        public SettingsModel()
        {
            Shifts = new ObservableCollection<ShiftModel>();
            Filters = new List<FilterModel>();
            Thresholds = new List<ThresholdModel>();
            XjhDirectory = string.Empty;
            ReportDirectory = string.Empty;
        }

        public ObservableCollection<ShiftModel> Shifts { get; set; }
        public List<FilterModel> Filters { get; set; }
        public List<ThresholdModel> Thresholds { get; set; }
        public string XjhDirectory { get; set; }
        public string ReportDirectory { get; set; }
    }
}