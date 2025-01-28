using System;

namespace JobReporter2.Model
{
    public class JobModel
    {
        public string Connection { get; set; }
        public string Name { get; set; }
        public string JobFile { get; set; }
        public string OEMString { get; set; }
        public string Shift { get; set; }

        public string StartType { get; set; }
        public string EndType { get; set; }

        public TimeSpan? PrepTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan? CutTime { get; set; }

        public float? FeedrateOveride { get; set; }

        public TimeSpan? SlewTime { get; set; }
        public TimeSpan? PauseTime { get; set; }
        public float? SheetCount { get; set; }
        public TimeSpan? SheetChangeTime { get; set; }
        public string Tools { get; set; }
        public TimeSpan? ToolChangeTime { get; set; }
        public TimeSpan? ToolAvgTimes { get; set; }

        public bool Flagged { get; set; }

    }
}
