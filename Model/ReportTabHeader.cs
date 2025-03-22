using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JobReporter2.Model
{
    public class ReportTabHeader : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public ICommand CloseCommand { get; }
        public ICommand ExportCommand { get; } // New command for PDF export

        public ReportTabHeader(string title, Action<ReportTabHeader> closeAction, Action<ReportTabHeader> exportAction)
        {
            Title = title;
            CloseCommand = new RelayCommand(() => closeAction(this));
            ExportCommand = new RelayCommand(() => exportAction(this)); // Initialize the export command
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}