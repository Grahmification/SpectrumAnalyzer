using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Series;
using SpectrumAnalyzer.Models;
using System.Linq;

namespace SpectrumAnalyzer.ViewModels
{
    public class ReconstructionVM : ObservableObject
    {
        public ObservableCollection<Datapoint> Dataset { get; set; } = new ObservableCollection<Datapoint>();

        public ObservableCollection<SignalComponent> SignalComponents { get; set; } = new ObservableCollection<SignalComponent>();



        public ObservableCollection<SelectableData<SignalComponent>> SignalComponents2 { get; set; } = new ObservableCollection<SelectableData<SignalComponent>>();
        public SelectedItemCollection<SignalComponent> SelectedComponents { get; set; } = new SelectedItemCollection<SignalComponent>();

        //public ObservableCollection<SignalComponent> SelectedComponents { get; set; } = new ObservableCollection<SignalComponent>();

        public ObservableCollection<SignalReconstruction> Reconstructions { get; set; } = new ObservableCollection<SignalReconstruction>();
        public string NewReconstructionName { get; set; } = "Reconstruction 1";

        public PlotModelManaged ReconstructionPlot { get; set; } = new PlotModelManaged();

        



        public ICommand SelectionChangedCommand { get; private set; }

        public ICommand AddReconstructionCommand { get; private set; }

        public ReconstructionVM()
        {
            AddReconstructionCommand = new RelayCommand<object>(AddReconstruction, AreSignalComponentsSelected);
            SelectionChangedCommand = new RelayCommand<object>(SelectionChanged);
            SelectedComponents.CollectionChanged += OnSelectComponentsChanged;
            SetupPlots();
        }

        public void SetupPlots()
        {
            ReconstructionPlot = new PlotModelManaged()
            {
                Title = "Signal Reconstruction"
            };

            ReconstructionPlot.Axes.Add(PlotModelManaged.AxisXPrimaryData());
            ReconstructionPlot.Axes.Add(PlotModelManaged.AxisYPrimaryData());
            ReconstructionPlot.Legends.Add(PlotModelManaged.DataLengend());

            var DataSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                Title = "Input Data",
                CanTrackerInterpolatePoints = false
            };

            var fitLineSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                Title = "Reconstruction",
                CanTrackerInterpolatePoints = true
            };


            ReconstructionPlot.AddSeries(DataSeries, PlotSeriesTag.RawData);
            ReconstructionPlot.AddSeries(fitLineSeries, PlotSeriesTag.FitLine);

        }

        public void PopulateDataSet(IEnumerable<Datapoint> dataset)
        {
            Dataset.Clear();

            foreach(Datapoint point in dataset)
            {
                Dataset.Add(point);
            }

            ReconstructionPlot.UpdateLineSeriesData(PlotSeriesTag.RawData, new List<Datapoint>(dataset));

            ReconstructionPlot.ResetAllAxes();
            ReconstructionPlot.InvalidatePlot(true);
        }

        public void PopulateComponents(IEnumerable<SignalComponent> components)
        {
            SignalComponents.Clear();
            SignalComponents2.Clear();
            var line = (LineSeries)ReconstructionPlot.PlotSeries[PlotSeriesTag.FitLine];
            line.Points.Clear();


            foreach (SignalComponent component in components)
            {
                SignalComponents.Add(component);

                var data = new SelectableData<SignalComponent>(component);
                data.SelectionChanged += OnSignalComponentSelected;
                SignalComponents2.Add(data);
            }        
        }

        public void OnSignalComponentSelected(object sender, bool selected)
        {
            var data = (SelectableData<SignalComponent>)sender;

            if (data.Selected)
            {
                SelectedComponents.Add(data.Data);
            }
            else
            {
                SelectedComponents.Remove(data.Data);
            }

            LineSeries fitline = (LineSeries)ReconstructionPlot.PlotSeries[PlotSeriesTag.FitLine];

            if (fitline.Points.Count == 0)
            {
                foreach (Datapoint point in Dataset)
                {
                    fitline.Points.Add(new DataPoint(point.X, 0));
                }
            }

            int sign = 1;

            if (data.Selected == false)
                sign = -1;

            for (int i = 0; i < fitline.Points.Count; i++)
            {
                var point = fitline.Points[i];
                fitline.Points[i] = new DataPoint(point.X, point.Y + data.Data.GetYValue(point.X) * sign);
            }

            if (SelectedComponents.Count > 0)
            {
                ReconstructionPlot.SetSeriesVisibility(PlotSeriesTag.FitLine, true);
            }
            else
            {
                ReconstructionPlot.SetSeriesVisibility(PlotSeriesTag.FitLine, false);
            }

            ReconstructionPlot.InvalidatePlot(true);
        }


        public void OnSelectComponentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ComputeReconstructionCurve();
        }

        public void ComputeReconstructionCurve()
        {
            if(SelectedComponents.Count > 0)
            {
                LineSeries fitline = (LineSeries)ReconstructionPlot.PlotSeries[PlotSeriesTag.FitLine];

                if (fitline.Points.Count == 0)
                {
                    foreach (Datapoint point in Dataset)
                    {
                        fitline.Points.Add(new DataPoint(point.X, 0));
                    }
                }

                //fitline.ItemsSource = Dataset;
                //fitline.DataFieldX = "X";
                //fitline.DataFieldY = "Y";

                for (int i = 0; i < fitline.Points.Count; i++)
                {                   
                    var point = fitline.Points[i];
                    double yVal = SignalComponent.ComputeYValueSum(SelectedComponents, point.X);
                    fitline.Points[i] = new DataPoint(point.X, yVal);
                }


                ReconstructionPlot.SetSeriesVisibility(PlotSeriesTag.FitLine, true);
            }
            else
            {
                ReconstructionPlot.SetSeriesVisibility(PlotSeriesTag.FitLine, false);
            }

            ReconstructionPlot.InvalidatePlot(true);
        }


        public void SelectionChanged(object parameter)
        {
            var items = (ListViewItem[])parameter;
        }


        public void AddReconstruction(object parameter)
        {
            var recon = new SignalReconstruction(new List<SignalComponent>(SelectedComponents), NewReconstructionName);
            recon.PopulatePoints((List<double>)Datapoint.XValues(Dataset));

            Reconstructions.Add(recon);
            NewReconstructionName = string.Format("Reconstruction {0}", Reconstructions.Count + 1);

            //plot the reconstruction
            var fitLineSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = true,
                ItemsSource = recon.Points,
                Title = recon.Name
            };

            ReconstructionPlot.AddSeries(fitLineSeries, (PlotSeriesTag)(Reconstructions.Count+200));
            ReconstructionPlot.SetSeriesVisibility((PlotSeriesTag)(Reconstructions.Count + 200), true);
            ReconstructionPlot.InvalidatePlot(true);
        }

        public bool AreSignalComponentsSelected()
        {       
            return SelectedComponents.Count > 0;
        }



    }
}
