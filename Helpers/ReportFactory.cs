using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
            return GenerateShiftJobReport(timeFrame);
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
        Key = "TimeFrames",
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
            YAxisKey = "TimeFrames"
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
            var colors = new[]
            {
            OxyColors.SkyBlue,
            OxyColors.LightGreen,
            OxyColors.LightPink,
            OxyColors.Orange,
            OxyColors.Purple
        };

            return colors[index % colors.Length];
        }
    


    private static PlotModel GenerateShiftJobReport(string timeFrame)
    {
        var plotModel = new PlotModel { Title = $"Shift Productivity (Jobs) - {timeFrame}" };

        // Example data - replace with actual data aggregation logic
        var shifts = new[] { "Shift 1", "Shift 2", "Shift 3" };
        var values = new[] { 30, 25, 35 }; // Replace with actual job counts per shift

        var categoryAxis = new CategoryAxis { Position = AxisPosition.Bottom, Key = "Shifts" };
        categoryAxis.Labels.AddRange(shifts);

        var valueAxis = new LinearAxis { Position = AxisPosition.Left, Title = "Number of Jobs" };

        var series = new BarSeries { Title = "Jobs Completed" };
        foreach (var value in values)
        {
            series.Items.Add(new BarItem(value));
        }

        plotModel.Axes.Add(categoryAxis);
        plotModel.Axes.Add(valueAxis);
        plotModel.Series.Add(series);

        //return new PlotView { Model = plotModel };
        return plotModel;
    }
}

