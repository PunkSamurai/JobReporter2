using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using JobReporter2.ViewModel;

namespace JobReporter2.View
{
    public partial class FilterView : Window
    {
        private bool _isInitializing = true;
        private FilterViewModel _viewModel;

        // Store event handlers so we can remove them later
        private SelectionChangedEventHandler _connectionsSelectionHandler;
        private SelectionChangedEventHandler _endTypesSelectionHandler;
        private SelectionChangedEventHandler _shiftsSelectionHandler;

        public FilterView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += FilterView_Loaded;
        }

        private void FilterView_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitializing = false;
            UpdateListBoxSelections();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from old ViewModel if any
            if (e.OldValue is FilterViewModel oldVm)
            {
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;
            }

            // Unsubscribe old event handlers
            RemoveSelectionChangedHandlers();

            _viewModel = DataContext as FilterViewModel;
            if (_viewModel != null)
            {
                // Subscribe to property changes for the selected filter
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                // Set up the selection changed events
                SetupSelectionChangedEvents();

                // Initial update
                UpdateListBoxSelections();
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterViewModel.SelectedFilter))
            {
                // Use background priority to let the data binding update first
                Dispatcher.BeginInvoke(new Action(() => {
                    UpdateListBoxSelectionsWithRetry();
                }), DispatcherPriority.Background);
            }
        }

        private void RemoveSelectionChangedHandlers()
        {
            // Remove existing handlers if they exist
            if (_connectionsSelectionHandler != null)
                ConnectionsListBox.SelectionChanged -= _connectionsSelectionHandler;

            if (_endTypesSelectionHandler != null)
                EndTypesListBox.SelectionChanged -= _endTypesSelectionHandler;

            if (_shiftsSelectionHandler != null)
                ShiftsListBox.SelectionChanged -= _shiftsSelectionHandler;
        }

        private void SetupSelectionChangedEvents()
        {
            // Remove old handlers
            RemoveSelectionChangedHandlers();

            // Create and add new handlers
            _connectionsSelectionHandler = (s, ev) => {
                if (!_isInitializing && _viewModel != null)
                {
                    _viewModel.SelectedConnections = new ObservableCollection<string>(
                        ConnectionsListBox.SelectedItems.Cast<string>());
                }
            };

            _endTypesSelectionHandler = (s, ev) => {
                if (!_isInitializing && _viewModel != null)
                {
                    _viewModel.SelectedEndTypes = new ObservableCollection<string>(
                        EndTypesListBox.SelectedItems.Cast<string>());
                }
            };

            _shiftsSelectionHandler = (s, ev) => {
                if (!_isInitializing && _viewModel != null)
                {
                    _viewModel.SelectedShifts = new ObservableCollection<string>(
                        ShiftsListBox.SelectedItems.Cast<string>());
                }
            };

            // Add the handlers
            ConnectionsListBox.SelectionChanged += _connectionsSelectionHandler;
            EndTypesListBox.SelectionChanged += _endTypesSelectionHandler;
            ShiftsListBox.SelectionChanged += _shiftsSelectionHandler;
        }

        private void UpdateListBoxSelectionsWithRetry(int retryCount = 3)
        {
            if (retryCount <= 0)
                return;

            try
            {
                _isInitializing = true;
                UpdateListBoxSelections();
                _isInitializing = false;
            }
            catch (Exception ex)
            {
                // If failed, retry after a short delay
                System.Diagnostics.Debug.WriteLine($"Error updating ListBox selections: {ex.Message}. Retrying...");
                Dispatcher.BeginInvoke(new Action(() => {
                    UpdateListBoxSelectionsWithRetry(retryCount - 1);
                }), DispatcherPriority.Background);
            }
        }

        private void UpdateListBoxSelections()
        {
            if (_viewModel == null)
                return;

            // Clear and update Connections ListBox
            ConnectionsListBox.SelectedItems.Clear();
            if (_viewModel.SelectedConnections != null)
            {
                foreach (var connection in _viewModel.SelectedConnections)
                {
                    if (ConnectionsListBox.Items.Contains(connection))
                    {
                        ConnectionsListBox.SelectedItems.Add(connection);
                    }
                }
            }

            // Clear and update EndTypes ListBox
            EndTypesListBox.SelectedItems.Clear();
            if (_viewModel.SelectedEndTypes != null)
            {
                foreach (var endType in _viewModel.SelectedEndTypes)
                {
                    if (EndTypesListBox.Items.Contains(endType))
                    {
                        EndTypesListBox.SelectedItems.Add(endType);
                    }
                }
            }

            // Clear and update Shifts ListBox
            ShiftsListBox.SelectedItems.Clear();
            if (_viewModel.SelectedShifts != null)
            {
                foreach (var shift in _viewModel.SelectedShifts)
                {
                    if (ShiftsListBox.Items.Contains(shift))
                    {
                        ShiftsListBox.SelectedItems.Add(shift);
                    }
                }
            }
        }
    }
}