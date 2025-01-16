using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using JobReporter2.ViewModel;

namespace JobReporter2.View
{
    public partial class FilterView : Window
    {
        public FilterView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is FilterViewModel viewModel)
            {
                ConnectionsListBox.SelectionChanged += (s, ev) =>
                {
                    viewModel.SelectedConnections = new ObservableCollection<string>(
                        ConnectionsListBox.SelectedItems.Cast<string>());
                };

                EndTypesListBox.SelectionChanged += (s, ev) =>
                {
                    viewModel.SelectedEndTypes = new ObservableCollection<string>(
                        EndTypesListBox.SelectedItems.Cast<string>());
                };
            }
        }
    }
}

