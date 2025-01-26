using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobReporter2.Model
{
    public class ShiftModel
    {
        public String Name { get; set; }

        public bool IsEnabled { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool IsInShift(DateTime jobStartTime)
        {
            var time = jobStartTime.TimeOfDay;
            return time >= StartTime && time <= EndTime;
        }

        public override string ToString() => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }
}
