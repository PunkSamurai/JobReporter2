using OxyPlot;
using System.Windows.Controls;
using System.Windows;

namespace JobReporter2.View
{
    public partial class ReportContent : UserControl
    {
        public static readonly DependencyProperty ReportModelProperty =
            DependencyProperty.Register("ReportModel", typeof(object), typeof(ReportContent),
                new PropertyMetadata(null, OnReportModelChanged));

        public object ReportModel
        {
            get { return GetValue(ReportModelProperty); }
            set { SetValue(ReportModelProperty, value); }
        }

        public ReportContent()
        {
            InitializeComponent();
        }

        private static void OnReportModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ReportContent;
            control?.UpdateContent(e.NewValue);
        }

        private void UpdateContent(object model)
        {
            // Reset visibility
            ReportPlotView.Visibility = Visibility.Visible;
            TextReportScrollViewer.Visibility = Visibility.Collapsed;

            if (model is PlotModel plotModel)
            {
                ReportPlotView.Model = plotModel;
            }
            else if (model is string textContent)
            {
                // It's a text report
                TextReportBlock.Text = textContent;
                ReportPlotView.Visibility = Visibility.Collapsed;
                TextReportScrollViewer.Visibility = Visibility.Visible;
            }
        }
    }
}