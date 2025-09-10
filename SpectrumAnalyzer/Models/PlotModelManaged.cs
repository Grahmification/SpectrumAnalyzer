using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace SpectrumAnalyzer.Models
{
    public class PlotModelManaged : PlotModel
    {
        public const string XAxisPrimaryKey = "Primary X";
        public const string YAxisPrimaryKey = "Primary Y";
        public const string YAxixSecondaryKey = "Secondary Y";


        public Dictionary<PlotSeriesTag, Series> PlotSeries = new Dictionary<PlotSeriesTag, Series>();

        public void AddSeries(Series series, PlotSeriesTag tag)
        {
            series.Tag = tag;
            PlotSeries.Add(tag, series);
        }
        public void RemoveSeries(PlotSeriesTag tag)
        {
            SetSeriesVisibility(tag, false);
            PlotSeries.Remove(tag);
        }
        public void RemoveSeries(Series series)
        {
            var tag = (PlotSeriesTag)series.Tag;
            RemoveSeries(tag);
        }

        public void UpdateLineSeriesData(PlotSeriesTag series, List<Datapoint> data)
        {
            LineSeries dataline = (LineSeries)PlotSeries[series];
            dataline.Points.Clear();

            foreach (Datapoint datapoint in data)
            {
                dataline.Points.Add(new DataPoint(datapoint.X, datapoint.Y));
            }

            if (Series.Contains(dataline) == false)
                Series.Add(dataline);
        }
        public void SetSeriesVisibility(PlotSeriesTag series, bool visible)
        {
            if (visible)
            {
                if (Series.Contains(PlotSeries[series]) == false)
                    Series.Add(PlotSeries[series]);
            }
            else
            {
                Series.Remove(PlotSeries[series]);
            }
        }

        public void ResetZoom()
        {
            this.ResetAllAxes();
            this.InvalidatePlot(false);
        }
        public void SaveImage(string filePath)
        {
            var pngExporter = new PngExporter { Width = 2000, Height = 1200 };
            pngExporter.ExportToFile(this, filePath);
        }

        public static LinearAxis AxisXPrimaryData(string title = "X Data")
        {
            return new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = title,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Key = XAxisPrimaryKey
            };
        }
        public static LinearAxis AxisYPrimaryData(string title = "Y Data")
        {
            return new LinearAxis()
            {
                Position = AxisPosition.Left,
                Title = title,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Key = YAxisPrimaryKey
            };
        }
        public static LinearAxis AxisYSecondaryData(string title = "Y Data 2")
        {
            return new LinearAxis()
            {
                Position = AxisPosition.Right,
                Title = title,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Key = YAxixSecondaryKey
            };
        }
        public static Legend DataLengend()
        {
            return new Legend()
            {
                IsLegendVisible = true,
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopRight,
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White)
            };
        }
    }
}
