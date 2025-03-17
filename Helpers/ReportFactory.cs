using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JobReporter2.Model;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;

public static class ReportFactory
{
    public static PlotModel GenerateReport(ObservableCollection<JobModel> filteredJobs, string reportType, string timeFrame)
    {
        if (reportType == "Shift Productivity (Time)")
        {
            return GenerateShiftTimeReport(filteredJobs, timeFrame);
        }
        else if (reportType == "Shift Productivity (Number of Jobs)")
        {
            return GenerateShiftJobReport(filteredJobs, timeFrame);
        }
        else if (reportType == "Advanced Shift Productivity (Time)")
        {
            return GenerateAdvancedShiftTimeReport(filteredJobs, timeFrame);
        }
        else
        {
            return null;
        }
    }


    
    
        public static PlotModel GenerateShiftTimeReport(ObservableCollection<JobModel> filteredJobs, string timeFrame)
{
    // Validate input
    if (filteredJobs == null || filteredJobs.Count == 0)
        throw new ArgumentException("No job data provided");

    // Create the plot model
    var plotModel = new PlotModel
    {
        Title = $"Shift Productivity (Time) - {timeFrame}"
    };

    // Get unique shifts
    var uniqueShifts = filteredJobs
        .Select(j => j.Shift)
        .Distinct()
        .OrderBy(s => s)
        .ToList();

    // Group data based on timeframe
    var groupedData = GroupDataByTimeFrame(filteredJobs, timeFrame);

    // Create category axis (y-axis)
    var categoryAxis = new CategoryAxis
    {
        Position = AxisPosition.Bottom,  // Changed to Left
        Title = timeFrame,
        Key = "yaxis",
        ItemsSource = groupedData.Keys
    };

    // Create value axis (x-axis)
    var valueAxis = new LinearAxis
    {
        Position = AxisPosition.Left,  // Changed to Bottom
        Title = "Hours Worked",
        MinimumPadding = 0.1,
        MaximumPadding = 0.1,
        Key = "xaxis"
    };

    // Add series for each shift
    int shiftIndex = 0;
    foreach (var shift in uniqueShifts)
    {
        var series = new BarSeries
        {
            Title = shift,
            StrokeColor = OxyColors.Black,
            StrokeThickness = 1,
            FillColor = GetColorForIndex(shiftIndex),
            IsStacked = false,  // Ensure bars aren't stacked
            BaseValue = 0,  // Start bars from 0
            XAxisKey = "xaxis",
            YAxisKey = "yaxis"
        };

        // Populate the series
        int categoryIndex = 0;
        foreach (var timeGroup in groupedData)
        {
            var hoursForShift = timeGroup.Value
                .Where(j => j.Shift == shift)
                .Sum(j => j.TotalTime.TotalHours);

            // For horizontal bars, we need to set the category index explicitly
            series.Items.Add(new BarItem { Value = hoursForShift, CategoryIndex = categoryIndex });
            categoryIndex++;
        }

        plotModel.Series.Add(series);
        shiftIndex++;
    }

    // Add axes to the plot - order matters for OxyPlot
    plotModel.Axes.Add(categoryAxis);
    plotModel.Axes.Add(valueAxis);

    return plotModel;
}

        private static Dictionary<string, List<JobModel>> GroupDataByTimeFrame(ObservableCollection<JobModel> jobs, string timeFrame)
        {
            var result = new Dictionary<string, List<JobModel>>();

            if (timeFrame.ToLower() == "daily")
            {
                result = jobs
                    .GroupBy(j => j.StartTime.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(
                        g => g.Key.ToString("MM/dd/yyyy"),
                        g => g.ToList()
                    );
            }
            else if (timeFrame.ToLower() == "weekly")
            {
                result = jobs
                    .GroupBy(j => GetWeekNumber(j.StartTime))
                    .OrderBy(g => g.Key)
                    .ToDictionary(
                        g => "Week " + g.Key.ToString(),
                        g => g.ToList()
                    );
            }
            else if (timeFrame.ToLower() == "monthly")
            {
                result = jobs
                    .GroupBy(j => new { j.StartTime.Year, j.StartTime.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .ToDictionary(
                        g => string.Format("{0}/{1}", g.Key.Year, g.Key.Month),
                        g => g.ToList()
                    );
            }
            else
            {
                throw new ArgumentException("Unsupported timeframe: " + timeFrame);
            }

            return result;
        }

        private static int GetWeekNumber(DateTime date)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                date,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }

    private static OxyColor GetColorForIndex(int index)
    {
        // Get the colors from the resource dictionary
        var colors = new[]
        {
        GetOxyColorFromResource("BackgroundShift1"),
        GetOxyColorFromResource("BackgroundShift2"),
        GetOxyColorFromResource("BackgroundShift3"),
        GetOxyColorFromResource("BackgroundShift4"),
        GetOxyColorFromResource("BackgroundShift5")
    };

        return colors[index % colors.Length];
    }

    private static OxyColor GetOxyColorFromResource(string resourceKey)
    {
        // Get the brush from the resource dictionary
        if (Application.Current.Resources[resourceKey] is SolidColorBrush brush)
        {
            // Convert the brush's color to OxyColor
            Color color = brush.Color;
            return OxyColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        // Return a default color if the resource is not found
        return OxyColors.Black;
    }



    public static PlotModel GenerateShiftJobReport(ObservableCollection<JobModel> filteredJobs, string timeFrame)
    {
        // Validate input
        if (filteredJobs == null || filteredJobs.Count == 0)
            throw new ArgumentException("No job data provided");

        // Create the plot model
        var plotModel = new PlotModel
        {
            Title = $"Shift Productivity (Job Count) - {timeFrame}"
        };

        // Get unique shifts
        var uniqueShifts = filteredJobs
            .Select(j => j.Shift)
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        // Group data based on timeframe
        var groupedData = GroupDataByTimeFrame(filteredJobs, timeFrame);

        // Create category axis (y-axis)
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            Title = timeFrame,
            Key = "yaxis",
            ItemsSource = groupedData.Keys
        };

        // Create value axis (x-axis)
        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Number of Jobs",
            MinimumPadding = 0.1,
            MaximumPadding = 0.1,
            Key = "xaxis",
            MajorStep = 1,  // Ensure whole number steps for job counts
            MinorStep = 1   // Ensure whole number steps for job counts
        };

        // Add series for each shift
        int shiftIndex = 0;
        foreach (var shift in uniqueShifts)
        {
            var series = new BarSeries
            {
                Title = shift,
                StrokeColor = OxyColors.Black,
                StrokeThickness = 1,
                FillColor = GetColorForIndex(shiftIndex),
                IsStacked = false,
                BaseValue = 0,
                XAxisKey = "xaxis",
                YAxisKey = "yaxis"
            };

            // Populate the series
            int categoryIndex = 0;
            foreach (var timeGroup in groupedData)
            {
                // Count jobs instead of summing time
                var jobCount = timeGroup.Value
                    .Count(j => j.Shift == shift);

                series.Items.Add(new BarItem { Value = jobCount, CategoryIndex = categoryIndex });
                categoryIndex++;
            }

            plotModel.Series.Add(series);
            shiftIndex++;
        }

        // Add axes to the plot
        plotModel.Axes.Add(categoryAxis);
        plotModel.Axes.Add(valueAxis);

        return plotModel;
    }

    public static PlotModel GenerateAdvancedShiftTimeReport(ObservableCollection<JobModel> filteredJobs, string timeFrame)
    {
        // Validate input
        if (filteredJobs == null || filteredJobs.Count == 0)
            throw new ArgumentException("No job data provided");

        // Create the plot model
        var plotModel = new PlotModel
        {
            Title = $"Advanced Shift Time Report - {timeFrame}"
        };

        // Get unique shifts
        var uniqueShifts = filteredJobs
            .Select(j => j.Shift)
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        // Group data based on timeframe
        var groupedData = GroupDataByTimeFrame(filteredJobs, timeFrame);

        // Create category axis
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            Title = timeFrame,
            Key = "yaxis",
            ItemsSource = groupedData.Keys
        };

        // Create value axis
        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Hours",
            MinimumPadding = 0.1,
            MaximumPadding = 0.1,
            Key = "xaxis"
        };

        // Add a legend
        plotModel.Legends.Add(new Legend
        {
            LegendPosition = LegendPosition.RightTop,
            LegendPlacement = LegendPlacement.Outside
        });

        // Calculate number of categories
        int categoryCount = groupedData.Count;

        // Loop through each time period (categories on the x-axis)
        int categoryIndex = 0;
        foreach (var timeGroup in groupedData)
        {
            // Loop through each shift (groups within each category)
            int shiftIndex = 0;
            foreach (var shift in uniqueShifts)
            {
                var jobsInShift = timeGroup.Value.Where(j => j.Shift == shift).ToList();

                var machineTime = jobsInShift
                    .Where(j => j.MachineTime.HasValue)
                    .Sum(j => j.MachineTime.Value.TotalHours);

                var prepTime = jobsInShift
                    .Where(j => j.PrepTime.HasValue)
                    .Sum(j => j.PrepTime.Value.TotalHours);

                var wastedTime = jobsInShift
                    .Where(j => j.WastedTime.HasValue)
                    .Sum(j => j.WastedTime.Value.TotalHours);

                // Create or retrieve series for each time type
                string machineTimeKey = $"{shift} - Machine Time";
                string prepTimeKey = $"{shift} - Prep Time";
                string wastedTimeKey = $"{shift} - Wasted Time";

                // Find existing series or create new ones
                BarSeries machineTimeSeries = plotModel.Series.OfType<BarSeries>().FirstOrDefault(s => s.Title == machineTimeKey);
                BarSeries prepTimeSeries = plotModel.Series.OfType<BarSeries>().FirstOrDefault(s => s.Title == prepTimeKey);
                BarSeries wastedTimeSeries = plotModel.Series.OfType<BarSeries>().FirstOrDefault(s => s.Title == wastedTimeKey);

                if (machineTimeSeries == null)
                {
                    machineTimeSeries = new BarSeries
                    {
                        Title = machineTimeKey,
                        StrokeColor = OxyColors.Black,
                        StrokeThickness = 1,
                        FillColor = GetColorForIndex(shiftIndex),
                        IsStacked = true,
                        StackGroup = $"{categoryIndex}-{shiftIndex}",
                        XAxisKey = "xaxis",
                        YAxisKey = "yaxis"
                    };
                    plotModel.Series.Add(machineTimeSeries);
                }

                if (prepTimeSeries == null)
                {
                    prepTimeSeries = new BarSeries
                    {
                        Title = prepTimeKey,
                        StrokeColor = OxyColors.Black,
                        StrokeThickness = 1,
                        FillColor = LightenColor(GetColorForIndex(shiftIndex), 0.3),
                        IsStacked = true,
                        StackGroup = $"{categoryIndex}-{shiftIndex}",
                        XAxisKey = "xaxis",
                        YAxisKey = "yaxis"
                    };
                    plotModel.Series.Add(prepTimeSeries);
                }

                if (wastedTimeSeries == null)
                {
                    wastedTimeSeries = new BarSeries
                    {
                        Title = wastedTimeKey,
                        StrokeColor = OxyColors.Black,
                        StrokeThickness = 1,
                        FillColor = LightenColor(GetColorForIndex(shiftIndex), 0.6),
                        IsStacked = true,
                        StackGroup = $"{categoryIndex}-{shiftIndex}",
                        XAxisKey = "xaxis",
                        YAxisKey = "yaxis"
                    };
                    plotModel.Series.Add(wastedTimeSeries);
                }

                // Calculate offset for this shift's group within the category
                double barGroupWidth = 0.8; // Width of the entire bar group as a fraction of category width
                double barWidth = barGroupWidth / uniqueShifts.Count;
                double offset = shiftIndex * barWidth - (barGroupWidth / 2) + (barWidth / 2);

                // Add data points with calculated offset
                machineTimeSeries.Items.Add(new BarItem { Value = machineTime, CategoryIndex = categoryIndex + (int) offset });
                prepTimeSeries.Items.Add(new BarItem { Value = prepTime, CategoryIndex = categoryIndex + (int)offset });
                wastedTimeSeries.Items.Add(new BarItem { Value = wastedTime, CategoryIndex = categoryIndex + (int)offset });

                shiftIndex++;
            }
            categoryIndex++;
        }

        // Add axes to the plot
        plotModel.Axes.Add(categoryAxis);
        plotModel.Axes.Add(valueAxis);

        return plotModel;
    }

    // Helper method to create lighter versions of colors for the stacked segments
    private static OxyColor LightenColor(OxyColor color, double factor)
    {
        // Create a lighter version of the provided color
        byte r = (byte)Math.Min(255, color.R + (255 - color.R) * factor);
        byte g = (byte)Math.Min(255, color.G + (255 - color.G) * factor);
        byte b = (byte)Math.Min(255, color.B + (255 - color.B) * factor);

        return OxyColor.FromRgb(r, g, b);
    }

}

