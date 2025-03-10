using JobReporter2.ViewModel;
using System;
using System.Collections.Generic;
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
        public JobsContent()
        {
            this.Resources["EndTypeToBrushConverter"] = new EndTypeToBrushConverter();
            InitializeComponent();
            // DataContext = new MainViewModel();
            
        }

        private class EndTypeToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string endType)
                {
                    switch (endType)
                    {
                        case "Job failed.":
                            return Brushes.LightCoral;
                        case "Job halted.":
                            return Brushes.LightYellow;
                        case "Job completed.":
                            return Brushes.LightGreen;
                        default:
                            return Brushes.Transparent;
                    }
                }
                return Brushes.Transparent;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}
