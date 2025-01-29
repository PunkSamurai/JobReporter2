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

        public ObservableCollection<ShiftModel> Shifts
        {
            get => _shifts;
            set => SetProperty(ref _shifts, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddShiftCommand { get; }

        public ShiftManagerViewModel()
        {
            // Initialize Shifts with default values
            Shifts = new ObservableCollection<ShiftModel>
            {
                // new ShiftModel {  Name = "Shift 1", StartTime = TimeSpan.Zero, EndTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)) }
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            AddShiftCommand = new RelayCommand(AddShift);
        }

        private void AddShift()
        {
            // Add a blank shift to the Shifts collection
            Shifts.Add(new ShiftModel
            {
                Name = $"Shift {Shifts.Count + 1}",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.Zero,
                IsEnabled = false
            });
        }

        private void Save()
        {
            foreach (var shift in Shifts)
            {
                Console.WriteLine(shift.Name);
            }
            // Logic to save the shifts and update jobs
            if (System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this) is Window window)
            {
                window.DialogResult = true; // This ensures the parent detects a successful apply
                window.Close();
            }
        }

        private void Cancel()
        {
            // Logic to cancel and close the window
            System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)?.Close();
        }
    }
}


