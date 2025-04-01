using System.Windows;
using OxyPlot;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JobReporter2.ViewModel
{
    public class ReportViewModel : INotifyPropertyChanged
    {
        private object _reportData;
        private Visibility _plotVisibility = Visibility.Visible;
        private Visibility _textReportVisibility = Visibility.Collapsed;
        private PlotModel _plotModel;
        private string _textReport;

        public object ReportData
        {
            get => _reportData;
            set
            {
                _reportData = value;
                UpdateVisibility();
                OnPropertyChanged();
            }
        }

        public PlotModel PlotModel
        {
            get => _plotModel;
            private set
            {
                _plotModel = value;
                OnPropertyChanged();
            }
        }

        public string TextReport
        {
            get => _textReport;
            private set
            {
                _textReport = value;
                OnPropertyChanged();
            }
        }

        public Visibility PlotVisibility
        {
            get => _plotVisibility;
            private set
            {
                _plotVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility TextReportVisibility
        {
            get => _textReportVisibility;
            private set
            {
                _textReportVisibility = value;
                OnPropertyChanged();
            }
        }

        private void UpdateVisibility()
        {
            if (ReportData is PlotModel model)
            {
                PlotModel = model;
                TextReport = null;
                PlotVisibility = Visibility.Visible;
                TextReportVisibility = Visibility.Collapsed;
            }
            else if (ReportData is string text)
            {
                PlotModel = null;
                TextReport = text;
                PlotVisibility = Visibility.Collapsed;
                TextReportVisibility = Visibility.Visible;
            }
            else
            {
                PlotModel = null;
                TextReport = null;
                PlotVisibility = Visibility.Collapsed;
                TextReportVisibility = Visibility.Collapsed;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}