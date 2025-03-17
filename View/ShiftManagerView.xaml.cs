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
    /// Interaction logic for ShiftManagerView.xaml
    /// </summary>
    public partial class ShiftManagerView : Window
    {
        public ShiftManagerView()
        {
            this.Resources["ShiftToBrushConverter"] = new ShiftToBrushConverter();
            InitializeComponent();
        }
        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var proposedText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // Allow only numbers and colon
            if (!char.IsDigit(e.Text[0]) && e.Text[0] != ':')
            {
                e.Handled = true;
                return;
            }

            // Handle colon input
            if (e.Text[0] == ':')
            {
                if (proposedText.Count(c => c == ':') > 1 || textBox.Text.Length == 0)
                {
                    e.Handled = true;
                    return;
                }
            }

            // Prevent invalid time format
            if (proposedText.Length > 5)
            {
                e.Handled = true;
                return;
            }
        }

        private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                textBox.Text = "00:00";
                return;
            }

            // Try to parse and format the time
            if (TimeSpan.TryParse(text, out TimeSpan time))
            {
                textBox.Text = time.ToString(@"hh\:mm");
            }
            else
            {
                // Handle invalid time format
                textBox.Text = "00:00";
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
    }
}
