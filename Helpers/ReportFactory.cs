using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;

public static class ReportFactory
{
    public static PlotModel GenerateReport(string reportType, string timeFrame)
    {
        if (reportType == "Shift Productivity (Time)")
        {
            return GenerateShiftTimeReport(timeFrame);
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


    public static PlotModel GenerateShiftTimeReport(string timeFrame)
    {
        // Create the plot model
        var plotModel = new PlotModel { Title = $"Shift Productivity (Time) - {timeFrame}" };

        // Example data - replace with actual data aggregation logic
        var shifts = new[] { "Shift 1", "Shift 2", "Shift 3" };
        var values = new[] { 120.0, 80.0, 100.0 }; // Using double values

        // Create and configure the category axis
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Left,
            Key = "Shifts",
            ItemsSource = shifts
        };

        // Create and configure the value axis
        var valueAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Hours",
            MinimumPadding = 0.1,
            MaximumPadding = 0.1
        };

        // Create and configure the bar series
        var series = new BarSeries
        {
            Title = "Time Worked",
            ItemsSource = values.Select((value, index) => new BarItem { Value = value }),
            LabelPlacement = LabelPlacement.Inside,
            LabelFormatString = "{0:0.#}"
        };

        // Add everything to the plot model
        plotModel.Axes.Add(categoryAxis);
        plotModel.Axes.Add(valueAxis);
        plotModel.Series.Add(series);

        

        return plotModel;
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

