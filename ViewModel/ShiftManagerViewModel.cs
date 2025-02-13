using JobReporter2.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JobReporter2.Helpers;

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
            Shifts = new ObservableCollection<ShiftModel>();
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            AddShiftCommand = new RelayCommand(AddShift);
        }

        private void AddShift()
        {
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
                Console.WriteLine(shift.Name + " " + shift.StartTime + " " + shift.EndTime + " " + shift.IsEnabled);
            }
            SettingsHelper.SaveShifts(Shifts);
            if (System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this) is Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void Cancel()
        {
            foreach (var shift in Shifts)
            {
                Console.WriteLine(shift.Name + " " + shift.StartTime + " " + shift.EndTime + " " + shift.IsEnabled);
            }
            System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)?.Close();
            if (System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this) is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}


