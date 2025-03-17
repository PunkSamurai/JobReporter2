using OxyPlot.Series;
using OxyPlot;
using System;
using System.ComponentModel;
using OxyPlot.Legends;
using System.IO;

namespace JobReporter2.Model
{
    public class JobModel : ObservableObject
    {
        public string Connection { get; set; }
        public string Name { get; set; }
        public string JobFile { get; set; }
        public string OEMString { get; set; }
        public string Shift { get; set; }
        public int? FileSize { get; set; }
        public float? CutLength { get; set; }
        public TimeSpan? TimeEstimate { get; set; }

        public string StartType { get; set; }
        public string EndType { get; set; }

        public TimeSpan? PrepTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan? WastedTime { get; set; }

        public TimeSpan? MachineTime { get; set; }
        public TimeSpan? CutTime { get; set; }
        public float? FeedrateOverride { get; set; }

        public TimeSpan? SlewTime { get; set; }
        public TimeSpan? PauseTime { get; set; }
        public float? SheetCount { get; set; }

        public int? PD_Count { get; set; }
        public string Material { get; set; }
        public TimeSpan? SheetChangeTime { get; set; }
        public string Tools { get; set; }
        public TimeSpan? ToolChangeTime { get; set; }
        public TimeSpan? ToolAvgTimes { get; set; }

        public string Size { get; set; }
        public bool Flagged { get; set; }

        public string PreviewImagePath { get; set; }
        public PlotModel PieChartModel { get; private set; }

        public bool CalculateFlagged()
        {
            if (TotalTime.TotalSeconds == 0)
                return false;

            if (EndType != "Job completed.")
                return true;

            double cutRatio = CutTime?.TotalSeconds / TotalTime.TotalSeconds ?? 0;
            double pauseRatio = PauseTime?.TotalSeconds / TotalTime.TotalSeconds ?? 0;

            return (cutRatio <= 0.75) || (pauseRatio >= 0.25);
        }

        public JobModel()
        {
            GeneratePieChart();
        }

        private double TimeSpanToValue(TimeSpan? timeSpan) => timeSpan?.TotalSeconds ?? 0;

        private string FormatSliceLabel(string name, TimeSpan? time)
        {
            if (time == null || time.Value.TotalSeconds == 0)
                return $"{name}: 0s";

            var ts = time.Value;
            return ts.TotalMinutes >= 1
                ? $"{name}: {ts.Minutes}m {ts.Seconds}s"
                : $"{name}: {ts.Seconds}s";
        }

        public void GeneratePieChart()
        {
            var plotModel = new PlotModel { Title = "Time Distribution" };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 2.0,
                AngleSpan = 360,
                StartAngle = 0,
                InnerDiameter = 0.6,
                OutsideLabelFormat = null, // Disable outside labels
                InsideLabelFormat = null, // Disable inside labels
                FontSize = 12,
                
                // LegendFormat = "{0}: {1}%"
            };

            var timeData = new[]
            {
                (Name: "Cut", Time: CutTime, Color: OxyColors.LightGreen),
                (Name: "Slew", Time: SlewTime, Color: OxyColors.LightBlue),
                (Name: "Pause", Time: PauseTime, Color: OxyColors.LightCoral),
                (Name: "Sheet Change", Time: SheetChangeTime, Color: OxyColors.Yellow),
                (Name: "Tool Change", Time: ToolChangeTime, Color: OxyColors.Violet)
            };

            foreach (var data in timeData)
            {
                pieSeries.Slices.Add(new PieSlice(data.Name, TimeSpanToValue(data.Time))
                {
                    Fill = data.Color,
                    IsExploded = false // Optional: Disable exploded slices
                });
            }

            plotModel.Series.Add(pieSeries);
            PieChartModel = plotModel;

            Console.WriteLine("Pie chart generated successfully.");
        }

        public string GetPreviewImagePath()
        {
            if (string.IsNullOrEmpty(JobFile))
            {
                Console.WriteLine("Debug: JobFile is null or empty.");
                return string.Empty;
            }

            // Determine the base application data path for "MachineToolSuite510"
            string appDataPath = GetApplicationDataPath("MachineToolSuite510", useAllUsers: true);
            // Build the preview cache directory: "Temp\PreviewCache"
            string cacheDir = Path.Combine(appDataPath, "Temp", "PreviewCache");

            if (!Directory.Exists(cacheDir))
            {
                Console.WriteLine($"Debug: Cache directory not found. Creating directory at: {cacheDir}");
                Directory.CreateDirectory(cacheDir);
            }

            // Get the job file name without folder and extension
            string jobFileName = Path.GetFileNameWithoutExtension(JobFile);
            Console.WriteLine($"Debug: Job file name (no extension): {jobFileName}");

            // Compute the hash using a case-insensitive method
            uint hash = HashKeyNoCase(jobFileName);
            Console.WriteLine($"Debug: Computed hash: {hash}");

            // Construct the final preview file path with PNG extension
            string cacheFile = Path.Combine(cacheDir, $"{hash}.png");
            Console.WriteLine($"Debug: Generated preview image path: {cacheFile}");

            return cacheFile;
        }

        /// <summary>
        /// Retrieves the application data path for the specified application name.
        /// Creates the directory if it does not exist.
        /// </summary>
        private string GetApplicationDataPath(string appName, bool useAllUsers)
        {
            string basePath = useAllUsers
                ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string targetPath = Path.Combine(basePath, appName);

            if (!Directory.Exists(targetPath))
            {
                Console.WriteLine($"Debug: Creating application data path: {targetPath}");
                Directory.CreateDirectory(targetPath);
            }

            return targetPath;
        }

        /// <summary>
        /// Computes a simple hash for the given key.
        /// </summary>
        private uint HashKey(string key)
        {
            uint hash = 0;
            foreach (char c in key)
            {
                hash = (hash << 5) + hash + c;
            }
            return hash;
        }

        /// <summary>
        /// Computes a case-insensitive hash by converting the key to lowercase first.
        /// </summary>
        private uint HashKeyNoCase(string key)
        {
            return HashKey(key.ToLowerInvariant());
        }

    }
}