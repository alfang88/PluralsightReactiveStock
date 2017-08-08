using System.Collections.Generic;
using Akka.Actor;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveStock.ActorModel.Messages;

namespace ReactiveStock.ActorModel.Actors.UI
{
    class LineChartingActor : ReceiveActor
    {
        private readonly PlotModel _chartModel;
        private readonly Dictionary<string, LineSeries> _series;

        public LineChartingActor(PlotModel chartModel)
        {
            _chartModel = chartModel;

            _series = new Dictionary<string, LineSeries>();

            Receive<AddChartSeriesMessage>(message => AddSeriesToChart(message));
            Receive<RemoveChartSeriesMessage>(message => RemoveSeriesFromChart(message));
            Receive<StockPriceMessage>(message => HandleNewStockPrice(message));
        }

        private void AddSeriesToChart(AddChartSeriesMessage message)
        {
            if (_series.ContainsKey(message.StockSymbol)) return;
            var newLineSeries = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.None,
                CanTrackerInterpolatePoints = false,
                Title = message.StockSymbol,
                Smooth = false
            };
            _series.Add(message.StockSymbol, newLineSeries);
            _chartModel.Series.Add(newLineSeries);

            RefreshChart();
        }

        private void RemoveSeriesFromChart(RemoveChartSeriesMessage message)
        {
            if (!_series.ContainsKey(message.StockSymbol)) return;
            var seriesToRemove = _series[message.StockSymbol];
            _chartModel.Series.Remove(seriesToRemove);
            _series.Remove(message.StockSymbol);

            RefreshChart();
        }

        private void HandleNewStockPrice(StockPriceMessage message)
        {
            if (!_series.ContainsKey(message.StockSymbol)) return;
            var series = _series[message.StockSymbol];

            var newDataPoint = new DataPoint(DateTimeAxis.ToDouble(message.Date),
                LinearAxis.ToDouble(message.StockPrice));

            // Keep the last 10 data points on graph
            if (series.Points.Count > 10)
                series.Points.RemoveAt(0);

            series.Points.Add(newDataPoint);

            RefreshChart();
        }

        private void RefreshChart()
        {
            _chartModel.InvalidatePlot(true);
        }
    }
}
