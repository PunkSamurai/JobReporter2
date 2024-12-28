namespace JobReporter2.Model
{
    public class JobModel
    {
        public string Connection { get; set; }
        public string JobFile { get; set; }
        public string EndType { get; set; }
        //public string PrepTime { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TotalTime { get; set; }
        //public string CutTime { get; set; }
        //public string Details => $"{JobFile} - {Connection}";
    }
}
