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

        private void OpenShiftManager()
        {
            var shiftManagerView = new ShiftManagerView
            {
                DataContext = new ShiftManagerViewModel(this) // Pass MainViewModel to ShiftManagerViewModel
            };

            shiftManagerView.ShowDialog();
        }
    }
}
