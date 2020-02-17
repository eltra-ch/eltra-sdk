using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using EltraNavigo.Device.Vcs;
using EltraNavigo.Views.Devices.Thermo.Base;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using EltraCommon.Math;
    
namespace EltraNavigo.Views.Devices.Thermo.History
{
    public class ThermoHistoryViewModel : ThermoToolViewModel
    {
        private enum PlotTypes
        {
            Internal,
            External,
            All
        }

        #region Private fields

        private DateTime _startDate;
        private DateTime _endDate;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private List<TemperatureHistory> _temperatureList;

        private double _minIntTemp;
        private double _minExtTemp;
        private double _maxIntTemp;
        private double _maxExtTemp;

        private double _minIntHum;
        private double _minExtHum;
        private double _maxIntHum;
        private double _maxExtHum;

        private int _relayOnCount;
        private double _relayOnTime;

        #endregion

        #region Constructors

        public ThermoHistoryViewModel()
        {
            UpdateViewModels = false;

            Title = "History";
            Image = ImageSource.FromResource("EltraNavigo.Resources.presentation_32px.png");
            Uuid = "FBC7F717-5F88-449D-B13D-A6CB866863E9";

            TempPlotModel = CreatePlotSeries(PlotTypes.All, "Temperature", OxyColors.CadetBlue, OxyColors.DarkBlue, 1, 0.5);
            HumidityPlotModel = CreatePlotSeries(PlotTypes.Internal, "Humidity", OxyColors.Coral, OxyColors.Crimson, 2, 1);

            var now = DateTime.Now;
            EndDate = now.Date;
            StartDate = EndDate - TimeSpan.FromMinutes(60*12);
            StartTime = new TimeSpan(now.Hour, now.Minute, now.Second);
            EndTime = new TimeSpan(now.Hour, now.Minute, now.Second);
        }

        #endregion

        #region Commands 

        public ICommand RefreshPlotsCommand => new Command(OnRefreshPlotPressed);

        public ICommand SetEndDateCommand => new Command(OnResetEndDatePressed);

        #endregion

        #region Properties

        public PlotModel TempPlotModel { get; set; }

        public PlotModel HumidityPlotModel { get; set; }

        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                SetProperty(ref _startDate, value);
            }
        }

        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                SetProperty(ref _endDate, value);
            }
        }

        public TimeSpan StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                SetProperty(ref _startTime, value);
            }
        }

        public TimeSpan EndTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                SetProperty(ref _endTime, value);
            }
        }

        public IList<TemperatureHistory> TemperatureList
        {
            get
            {
                return _temperatureList ?? (_temperatureList = new List<TemperatureHistory>());
            }
            set
            {
                _temperatureList = new List<TemperatureHistory>(value);

                OnPropertyChanged("TemperatureList");
            }
        }

        public double MinIntTemp
        {
            get => _minIntTemp;
            set => SetProperty(ref _minIntTemp, value);
        }

        public double MinExtTemp
        {
            get => _minExtTemp;
            set => SetProperty(ref _minExtTemp, value);
        }

        public double MaxIntTemp
        {
            get => _maxIntTemp;
            set => SetProperty(ref _maxIntTemp, value);
        }

        public double MaxExtTemp
        {
            get => _maxExtTemp;
            set => SetProperty(ref _maxExtTemp, value);
        }

        public double MinIntHum
        {
            get => _minIntHum;
            set => SetProperty(ref _minIntHum, value);
        }

        public double MinExtHum
        {
            get => _minExtHum;
            set => SetProperty(ref _minExtHum, value);
        }

        public double MaxIntHum
        {
            get => _maxIntHum;
            set => SetProperty(ref _maxIntHum, value);
        }

        public double MaxExtHum
        {
            get => _maxExtHum;
            set => SetProperty(ref _maxExtHum, value);
        }

        public int RelayOnCount
        {
            get => _relayOnCount;
            set => SetProperty(ref _relayOnCount, value);
        }

        public double RelayOnTime
        {
            get => _relayOnTime;
            set => SetProperty(ref _relayOnTime, value);
        }

        #endregion

        #region Events handling

        private async void OnRefreshPlotPressed(object obj)
        {
            await GetSensorHistory();
        }

        private void OnResetEndDatePressed(object e)
        {
            var now = DateTime.Now;

            EndDate = now.Date;
            EndTime = new TimeSpan(now.Hour, now.Minute, now.Second);
        }

        #endregion

        #region Methods

        private void UpdatePlotSamples(PlotModel model, List<SensorHistoryItem> history, List<SensorHistoryItem> historyExt)
        {
            try
            {
                foreach (var series in model.Series)
                {
                    if (series is FunctionSeries fixDivLine && fixDivLine.Title == "Internal")
                    {
                        fixDivLine.Points.Clear();

                        foreach (var item in history)
                        {
                            fixDivLine.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Timestamp), item.Value));
                        }
                    }
                    if (series is FunctionSeries fixDivLineExt && fixDivLineExt.Title == "External")
                    {
                        fixDivLineExt.Points.Clear();

                        foreach (var item in historyExt)
                        {
                            fixDivLineExt.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Timestamp), item.Value));
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                model.ResetAllAxes();
                model.InvalidatePlot(true);
            });
        }

        private void UpdatePlotSamples(PlotModel model, List<SensorHistoryItem> history)
        {
            try
            {
                foreach (var series in model.Series)
                {
                    if (series is FunctionSeries fixDivLine && fixDivLine.Title == "Internal")
                    {
                        fixDivLine.Points.Clear();

                        foreach (var item in history)
                        {
                            fixDivLine.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Timestamp), item.Value));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                model.ResetAllAxes();
                model.InvalidatePlot(true);
            });
        }

        private async Task GetRelaySensorHistory(DateTime start, DateTime end)
        {
            IsBusy = true;

            var thistory = await (Vcs as ThermoVcs).GetRelayHistory(start, end);
            
            DateTime a1 = DateTime.MinValue;
            var diff = new TimeSpan();
            
            int relayHighCount = 0;

            foreach (var item in thistory)
            {
                ushort val = 0;
                
                if (item.GetValue(ref val))
                {
                    if (val == 1)
                    {
                        relayHighCount++;

                        a1 = item.Modified;
                    }
                    else if (val == 0 && a1 != DateTime.MinValue)
                    {
                        diff += item.Modified - a1;
                    }
                }
            }

            RelayOnCount = relayHighCount;
            RelayOnTime = Math.Round(diff.TotalMinutes / 60, 1);

            IsBusy = false;
        }

        private async Task GetSensorHistory()
        {
            IsBusy = true;

            var endDate = EndDate.Date + EndTime;
            var startDate = StartDate.Date + StartTime;

            var th1 = await (Vcs as ThermoVcs).GetTemperatureHistory(startDate, endDate);
            var th2 = await (Vcs as ThermoVcs).GetTemperatureExtHistory(startDate, endDate);

            UpdateSensorStat(th1, out var minInternalTValue, out var maxInternalTValue);

            MinIntTemp = minInternalTValue;
            MaxIntTemp = maxInternalTValue;

            UpdateSensorStat(th2, out var minExternalTValue, out var maxExternalTValue);

            MinExtTemp = minExternalTValue;
            MaxExtTemp = maxExternalTValue;

            var t1 = KalmanFilter(th1);
            var t2 = KalmanFilter(th2);

            UpdatePlotSamples(TempPlotModel, t1, t2);

            var hh1 = await (Vcs as ThermoVcs).GetHumidityHistory(startDate, endDate);
            
            UpdateSensorStat(hh1, out var minInternalHValue, out var maxInternalHValue);

            MinIntHum = minInternalHValue;
            MaxIntHum = maxInternalHValue;

            var h1 = KalmanFilter(hh1);
            
            UpdatePlotSamples(HumidityPlotModel, h1);

            await GetRelaySensorHistory(startDate, endDate);

            IsBusy = false;
        }

        private static List<SensorHistoryItem> KalmanFilter(List<SensorHistoryItem> thistory)
        {
            Kalman kalman = null;
            var filteredItems = new List<SensorHistoryItem>();

            foreach (var th in thistory)
            {
                if (kalman == null)
                {
                    kalman = new Kalman(0.125, 32, 1023, th.Value);
                }

                var kv = kalman.GetFilteredValue(th.Value);

                filteredItems.Add(new SensorHistoryItem(kv, th.Timestamp));
            }

            return filteredItems;
        }

        private void RemoveOutOfRange(ref List<KeyValuePair<double, DateTime>> items, double minValue, double maxValue)
        {
            foreach(var item in items)
            {
                if(item.Key < minValue || item.Key > maxValue)
                {
                    items.Remove(item);

                    RemoveOutOfRange(ref items, minValue, maxValue);
                    break;
                }
            }
        }

        private void UpdateSensorStat(List<SensorHistoryItem> historyList, out double minValue, out double maxValue)
        {
            maxValue = double.MinValue;
            minValue = double.MaxValue;

            foreach (var t in historyList)
            {
                if (t.Value > maxValue)
                {
                    maxValue = t.Value;
                }

                if (t.Value < minValue)
                {
                    minValue = t.Value;
                }
            }
        }
        
        public override async Task Show()
        {
            IsBusy = true;

            await GetSensorHistory();

            await base.Show();

            IsBusy = false;
        }

        private void EnablePanning(PlotModel model, bool enable)
        {
            foreach (var axis in model.Axes)
            {
                axis.IsPanEnabled = enable;
            }
        }

        private void EnableZooming(PlotModel model, bool enable)
        {
            foreach (var axis in model.Axes)
            {
                axis.IsZoomEnabled = enable;
            }
        }

        private PlotModel CreatePlotSeries(PlotTypes type, string label, OxyColor color, OxyColor colorExt, double majorStep, double minorStep)
        {
            var model = new PlotModel { Title = label, LegendSymbolLength = 24 };

            var xAxis = new DateTimeAxis()
            {
                Key = "XAxis",
                StringFormat = "dd/MM HH:mm",
                MinorIntervalType = DateTimeIntervalType.Hours,
                TickStyle = TickStyle.None,
                Position = AxisPosition.Bottom
            };

            var yAxis = new LinearAxis()
            {
                Key = "YAxis",
                Position = AxisPosition.Left,
                MajorStep = majorStep,
                MinorStep = minorStep
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            EnablePanning(model, false);
            EnableZooming(model, false);

            if (type == PlotTypes.Internal || type == PlotTypes.All)
            {
                var series = new FunctionSeries();

                series.Title = "Internal";
                series.Color = color;
                series.LineJoin = LineJoin.Bevel;
                series.LineStyle = LineStyle.Solid;
                series.StrokeThickness = 1;

                model.Series.Add(series);
            }

            if (type == PlotTypes.External || type == PlotTypes.All)
            {
                var seriesExt = new FunctionSeries();

                seriesExt.Title = "External";
                seriesExt.Color = colorExt;
                seriesExt.LineJoin = LineJoin.Bevel;
                seriesExt.LineStyle = LineStyle.Solid;
                seriesExt.StrokeThickness = 1;

                model.Series.Add(seriesExt);

                model.InvalidatePlot(true);
            }

            return model;
        }

        #endregion
    }
}
