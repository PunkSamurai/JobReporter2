using JobReporter2.ViewModel;
using System;
using System.Collections.Generic;
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
                // Optionally, you could show an error message here
            }
        }
    }
}
