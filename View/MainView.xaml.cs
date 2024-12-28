using System.Windows.Controls;
using JobReporter2.ViewModel;

namespace JobReporter2.View
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel(); // Bind the ViewModel
        }
    }
}
