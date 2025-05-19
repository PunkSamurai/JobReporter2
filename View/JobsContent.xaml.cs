using JobReporter2.Helpers;
using JobReporter2.Model;
using JobReporter2.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
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
    public partial class JobsContent
    {
        public ObservableCollection<JobModel> Jobs { get; set; }

        public JobsContent()
        {
            this.Resources["EndTypeToBrushConverter"] = new EndTypeToBrushConverter();
            this.Resources["CutTimeToBrushConverter"] = new CutTimeToBrushConverter();
            this.Resources["PauseTimeToBrushConverter"] = new PauseTimeToBrushConverter();
            this.Resources["PrepTimeToBrushConverter"] = new PrepTimeToBrushConverter();
            this.Resources["ShiftToBrushConverter"] = new ShiftToBrushConverter();
            this.Resources["TotalTimeToBrushConverter"] = new TotalTimeToBrushConverter();
            this.Resources["BoolToColorConverter"] = new BoolToColorConverter();
            this.Resources["NullToVisibilityConverter"] = new NullToVisibilityConverter();
            this.Resources["NullToRowHeightConverter"] = new NullToRowHeightConverter();
            InitializeComponent();
            Loaded += JobsContent_Loaded;
            // DataContext = new MainViewModel();

        }

        public class EndTypeToBrushConverter : IValueConverter
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

        public class CutTimeToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is JobModel data)
                {
                    var thresholds = GetThresholds("CutTime");
                    if (!thresholds.IsEnabled) return Brushes.Transparent;

                    double ratio = -1;
                    if (data.TotalTime.TotalSeconds > 0)
                    {
                        ratio = data.CutTime?.TotalSeconds / data.TotalTime.TotalSeconds ?? 0;
                    }

                    if (ratio == -1) return Brushes.Transparent;

                    if (ratio > (thresholds.Value1 / 100)) return Brushes.LightGreen;
                    if (ratio > (thresholds.Value2 / 100)) return Brushes.Yellow;
                    return Brushes.LightCoral;
                }
                return Brushes.Transparent;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }


        public class PauseTimeToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is JobModel data)
                {
                    var thresholds = GetThresholds("PauseTime");
                    if (!thresholds.IsEnabled) return Brushes.Transparent;

                    double ratio = 0;
                    if (data.TotalTime.TotalSeconds > 0)
                    {
                        ratio = data.PauseTime?.TotalSeconds / data.TotalTime.TotalSeconds ?? 0;
                    }

                    if (ratio < (thresholds.Value1 / 100)) return Brushes.LightGreen;
                    if (ratio < (thresholds.Value2 / 100)) return Brushes.Yellow;
                    return Brushes.LightCoral;
                }
                return Brushes.Transparent; // Default
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        public class PrepTimeToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is JobModel data && parameter is string timeType)
                {
                    var thresholds = GetThresholds(timeType);
                    if (!thresholds.IsEnabled) return Brushes.Transparent;

                    if (data.PrepTime.HasValue)
                    {
                        double minutes = data.PrepTime?.TotalMinutes ?? 0;
                        if (minutes <= thresholds.Value1) return Brushes.LightGreen;
                        if (minutes <= thresholds.Value2) return Brushes.Yellow;
                        return Brushes.LightCoral;
                    }
                }
                return Brushes.Transparent; 
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        public class TotalTimeToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is JobModel data)
                {
                    var thresholds = GetThresholds("TotalTime");
                    if (!thresholds.IsEnabled) return Brushes.Transparent;
                    double ratio = -1;
                    if (data.TotalTime.TotalSeconds > 0 && data.TimeEstimate.HasValue)
                    {
                        ratio = data.TimeEstimate.Value.TotalSeconds / data.TotalTime.TotalSeconds;
                    }
                    else if (data.TotalTime.TotalSeconds == 0)
                    {
                        return Brushes.LightCoral;
                    }

                    if (ratio == -1) return Brushes.Transparent;

                    if (ratio > (thresholds.Value1 / 100)) return Brushes.LightGreen;
                    if (ratio > (thresholds.Value2 / 100)) return Brushes.Yellow;
                    return Brushes.LightCoral;
                }
                return Brushes.Transparent;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }


        public class ShiftToBrushConverter : IValueConverter
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
                if (value is string shiftName)
                {
                    if (shiftName == ShiftNames[0])
                        return Application.Current.Resources["BackgroundShift1"];
                    else if (shiftName == ShiftNames[1])
                        return Application.Current.Resources["BackgroundShift2"];
                    else if (shiftName == ShiftNames[2])
                        return Application.Current.Resources["BackgroundShift3"];
                    else if (shiftName == ShiftNames[3])
                        return Application.Current.Resources["BackgroundShift4"];
                    else if (shiftName == ShiftNames[4])
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

        public class BoolToColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool flagged && flagged)
                    return new SolidColorBrush(Colors.Red);
                else
                    return new SolidColorBrush(Colors.Transparent);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class NullToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value == null ? Visibility.Hidden : Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class NullToRowHeightConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value == null ? new GridLength(0) : new GridLength(1, GridUnitType.Auto);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
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


        private static ThresholdModel GetThresholds(string name)
        {
            // Retrieve threshold data dynamically, assuming it's preloaded
            return SettingsHelper.LoadThresholds().FirstOrDefault(t => t.Name == name) ?? new ThresholdModel();
        }


    }
}
