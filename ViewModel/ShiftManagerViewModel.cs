using JobReporter2.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace JobReporter2.ViewModel
{
    public class ShiftManagerViewModel : ObservableObject
    {
        private ObservableCollection<ShiftModel> _shifts;
        private readonly MainViewModel _mainViewModel;

        public ObservableCollection<ShiftModel> Shifts
        {
            get => _shifts;
            set => SetProperty(ref _shifts, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddShiftCommand { get; }

        public ShiftManagerViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            // Initialize Shifts with current shifts from MainViewModel
            Shifts = new ObservableCollection<ShiftModel>(_mainViewModel.Shifts);

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            AddShiftCommand = new RelayCommand(AddShift);
        }

        private void AddShift()
        {
            // Add a blank shift to the Shifts collection
            Shifts.Add(new ShiftModel
            {
                Name = "New Shift",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.Zero,
                IsEnabled = false
            });
        }

        private void Save()
        {
            // Update MainViewModel shifts
            _mainViewModel.Shifts = new ObservableCollection<ShiftModel>(Shifts);

            // Update jobs with the new shifts
            _mainViewModel.UpdateJobShifts();

            // Close the ShiftManagerView
            System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)?.Close();
        }

        private void Cancel()
        {
            // Close the ShiftManagerView without saving
            System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)?.Close();
        }
    }
}

