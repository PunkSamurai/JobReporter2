using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobReporter2.Model
{
    public class ThresholdModel
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public double Value1 { get; set; }
        public double Value2 { get; set; }
    }
}
