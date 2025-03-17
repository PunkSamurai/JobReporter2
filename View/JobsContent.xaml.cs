using JobReporter2.Helpers;
using JobReporter2.Model;
using JobReporter2.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JobReporter2.View
{
    /// <summary>
    /// Interaction logic for JobsContent.xaml
    /// </summary>
    public partial class JobsContent
    {
        public ObservableCollection<JobModel> Jobs { get; set; }

        public JobsContent()
        {
            this.Resources["EndTypeToBrushConverter"] = new EndTypeToBrushConverter();
            this.Resources["FlagToBrushConverter"] = new FlagToBrushConverter();
            this.Resources["CutTimeToBrushConverter"] = new CutTimeToBrushConverter();
            this.Resources["PauseTimeToBrushConverter"] = new PauseTimeToBrushConverter();
            this.Resources["PrepTimeToBrushConverter"] = new PrepTimeToBrushConverter();
            this.Resources["ShiftToBrushConverter"] = new ShiftToBrushConverter();
            InitializeComponent();
            Loaded += JobsContent_Loaded;
            // DataContext = new MainViewModel();

        }

        private class EndTypeToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string endType)
                {
                    if (endType == "Job completed.")
                        return Brushes.LightGreen;
                    else
                        return Brushes.LightCoral;
                }
                return Brushes.Transparent;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private class CutTimeToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is JobModel data)
                {
                    if (data.TotalTime.TotalSeconds > 0)
                    {
                        double ratio = data.CutTime?.TotalSeconds / data.TotalTime.TotalSeconds ?? 0;

                        if (ratio > 0.75)
                            return Brushes.LightGreen;
                        else if (ratio > 0.5)
                            return Brushes.Yellow;
                        return Brushes.LightCoral;
                    }
                }

                return Brushes.Transparent; // Default
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }


        private class PauseTimeToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is JobModel data)
                {
                    if (data.TotalTime.TotalSeconds > 0)
                    {
                        double ratio = data.PauseTime?.TotalSeconds / data.TotalTime.TotalSeconds ?? 0;

                        if (ratio < 0.25)
                            return Brushes.LightGreen;
                        else if (ratio < 0.5)
                            return Brushes.Yellow;
                        return Brushes.LightCoral;
                    }
                }

                return Brushes.Transparent; // Default
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private class PrepTimeToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is JobModel data)
                {
                    if (data.TotalTime.TotalSeconds > 0)
                    {
                        double ratio = data.PrepTime?.TotalSeconds / data.TotalTime.TotalSeconds ?? 0;
                        Console.WriteLine(ratio);
                        if (ratio < 0.25)
                            return Brushes.LightGreen;
                        else if (ratio < 0.5)
                            return Brushes.Yellow;
                        return Brushes.LightCoral;
                    }
                }

                return Brushes.Transparent; // Default
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private class TotalTimeToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is JobModel data)
                {
                    if (data.TotalTime.TotalSeconds > 0 && data.TimeEstimate.HasValue)
                    {
                        double ratio = data.TimeEstimate?.TotalSeconds / data.TotalTime.TotalSeconds ?? 0;
                        Console.WriteLine(ratio);
                        if (ratio > 0.75)
                            return Brushes.LightGreen;
                        else if (ratio > 0.5)
                            return Brushes.Yellow;
                        return Brushes.LightCoral;
                    }
                }

                return Brushes.Transparent; // Default
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private class FlagToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool isFlagged)
                {
                    return isFlagged ? Brushes.Red : Brushes.White;
                }
                return Brushes.White; // Default
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private class ShiftToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                ObservableCollection<ShiftModel> Shifts;
                Shifts = SettingsHelper.LoadShifts();
                ObservableCollection<String> ShiftNames = new ObservableCollection<String>();
                foreach (ShiftModel shift in Shifts)
                {
                    ShiftNames.Add(shift.Name);
                }
                Console.Write(ShiftNames);
                if(value is string shiftName)
                {
                    if (shiftName == ShiftNames[0])
                        return Application.Current.Resources["BackgroundShift1"];
                    else if (shiftName == ShiftNames[1])
                        return Application.Current.Resources["BackgroundShift2"];
                    else if (shiftName == ShiftNames[2])
                        return Application.Current.Resources["BackgroundShift3"];
                    else if (shiftName == ShiftNames[3])
                        return Application.Current.Resources["BackgroundShift4"];
                    else if(shiftName == ShiftNames[4])
                        return Application.Current.Resources["BackgroundShift5"];
                    return Brushes.Transparent;
                }
                // Additional logic to convert ShiftNames to Brush can be added here
                return Brushes.Transparent; // Default return value
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }


        private void JobsContent_Loaded(object sender, RoutedEventArgs e)
        {
            // Call UpdateVisibleColumns when the component is initialized
            UpdateVisibleColumns();
        }

        private void UpdateVisibleColumns()
        {
            if (JobDataGrid == null || Jobs == null || !Jobs.Any())
                return;

            foreach (var column in JobDataGrid.Columns)
            {
                if (column is DataGridBoundColumn boundColumn && boundColumn.Binding is Binding binding)
                {
                    var bindingPath = binding.Path.Path;
                    if (string.IsNullOrEmpty(bindingPath))
                        continue;

                    // Get distinct values for the column
                    var distinctValues = Jobs
                        .Select(job => typeof(JobModel).GetProperty(bindingPath)?.GetValue(job))
                        .Distinct()
                        .ToList();

                    // Logic for column visibility
                    if (bindingPath == "Connection" || bindingPath == "OEMString")
                    {
                        column.Visibility = distinctValues.Count <= 1
                            ? Visibility.Collapsed
                            : Visibility.Visible;
                    }
                    else
                    {
                        // Hide if all values are null
                        column.Visibility = distinctValues.All(value => value == null)
                            ? Visibility.Collapsed
                            : Visibility.Visible;
                    }
                }
            }
        }
    }
}
