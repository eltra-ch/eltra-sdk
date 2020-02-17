using EltraConnector.UserAgent.Vcs;
using EltraNavigo.Controls;
using EltraNavigo.Device.Vcs;
using EltraNavigo.Views.DataRecorder.Series;
using EltraNavigo.Views.Obd.Inputs;
using EltraNavigo.Views.Obd.Inputs.Events;
using EltraNavigo.Views.Obd.Outputs;
using EltraNavigo.Views.Obd.Outputs.Events;
using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace EltraNavigo.Views.DataRecorder
{
    public class DataRecorderViewModel : ToolViewModel
    {
        #region Private fields

        private bool _isRunning;

        private ObdInputsViewModel _inputsViewModel;
        private ObdOutputsViewModel _outputsViewModel;
        
        #endregion

        #region Constructors

        public DataRecorderViewModel()
        {
            Title = "Data Recorder";
            Image = ImageSource.FromResource("EltraNavigo.Resources.presentation_32px.png");
            Uuid = "1440B525-0DE2-46C7-872F-CE4FAE1B02DC";

            Model = OneSeries();

            PropertyChanged += OnViewModelPropertyChanged;
        }

        #endregion

        #region Properties

        public PlotModel Model { get; set; }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public ObdInputsViewModel InputsViewModel => _inputsViewModel ?? (_inputsViewModel = CreateInputsViewModel());

        public ObdOutputsViewModel OutputsViewModel => _outputsViewModel ?? (_outputsViewModel = CreateOutputsViewModel());

        #endregion

        #region Events handling

        private async void OnSearchTextChanged(object sender, SearchTextChangedEventArgs e)
        {
            await OutputsViewModel.SetFilter(e.Text);
        }

        private async void OnSearchTextCanceled(object sender, EventArgs e)
        {
            await OutputsViewModel.ResetFilter();
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsRunning")
            {
                if(!IsRunning)
                {
                    Task.Run(() => { UpdateRecorderSamples(); });
                }
            }
        }

        private EposVcs EposVcs => (Vcs as EposVcs);

        private async void OnEntryAdded(object sender, ObdEventAddedEventArgs e)
        {
            const int maxChannelsCount = 4;
            
            if(Model.Series.Count < maxChannelsCount)
            {
                var entry = e.Entry;
                var parameterEntry = entry.ParameterEntry;
                var series = new FixDivLineSeries(entry, GetSeriesColor());

                await EposVcs.ActivateChannel((byte)(Model.Series.Count+1), parameterEntry.Index, parameterEntry.SubIndex);
            
                series.ChannelIndex = (byte)(Model.Series.Count + 1);

                Model.Series.Add(series);

                Model.InvalidatePlot(true);
            }
        }

        private async void OnEntryRemoved(object sender, ObdEventAddedEventArgs e)
        {
            var entry = e.Entry;

            var lineSeries = FindLineSeries(entry);

            if(lineSeries!=null)
            {
                await EposVcs.DeactivateChannel(lineSeries.ChannelIndex);

                Model.Series.Remove(lineSeries);

                Model.InvalidatePlot(false);
            }
        }

        #endregion

        #region Methods

        private void UpdateIsRecorderRunningAsync()
        {
            Task.Run(async () => {
                IsRunning = await IsRecorderRunning();
            });
        }

        protected override Task Update()
        {
            UpdateIsRecorderRunningAsync();

            return base.Update();
        }

        private ObdOutputsViewModel CreateOutputsViewModel()
        {
            var result = new ObdOutputsViewModel(this);

            result.IsEditable = false;

            result.EntryAdded += OnEntryAdded;
            result.EntryRemoved += OnEntryRemoved;

            return result;
        }
        
        private ObdInputsViewModel CreateInputsViewModel()
        {
            var result = new ObdInputsViewModel(this);

            result.SearchTextChanged += OnSearchTextChanged;
            result.SearchTextCanceled += OnSearchTextCanceled;

            return result;
        }

        public static PlotModel OneSeries()
        {
            var model = new PlotModel { Title = "", LegendSymbolLength = 24 };

            var xAxis = new LinearAxis()
            {
                Key = "XAxis",
                Minimum = 0,
                TickStyle = TickStyle.None,
                Position = AxisPosition.Bottom
            };

            var yAxis = new LinearAxis()
            {
                Key = "YAxis",                
                Position = AxisPosition.Left
            };

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            return model;
        }

        private async void UpdateRecorderSamples()
        {
            foreach(var series in Model.Series)
            {
                if (series is FixDivLineSeries fixDivLine)
                { 
                    var dataRecorderSamples = await EposVcs.ReadChannelDataVector(fixDivLine.ChannelIndex);

                    if (dataRecorderSamples != null)
                    {
                        var lastTimestamp = dataRecorderSamples.LastTimestamp;

                        fixDivLine.Points.Clear();

                        if(dataRecorderSamples.Samples.Count>1)
                        {
                            var firstSample = dataRecorderSamples.Samples[0];
                            var secondSample = dataRecorderSamples.Samples[1];
                            var samplingPeriod = (double)secondSample.Timestamp - (double)firstSample.Timestamp;

                            var axis = Model.Axes.First(x => x.Key.Equals("XAxis"));

                            axis.Minimum = firstSample.Timestamp;
                            axis.Maximum = lastTimestamp;
                            
                            foreach (var sample in dataRecorderSamples.Samples)
                            {
                                //double timestamp = ((double)sample.Timestamp - (double)firstSample.Timestamp) / samplingPeriod;
                                double timestamp = sample.Timestamp;

                                fixDivLine.Points.Add(new DataPoint(timestamp, sample.Value));                                
                            }
                        }
                    }
                }
            }

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => 
            {
                Model.ResetAllAxes();
                Model.InvalidatePlot(true); 
            });
        }

        private OxyColor GetSeriesColor()
        {
            OxyColor result = OxyColors.Transparent;

            switch (Model.Series.Count)
            {
                case 0:
                    result = OxyColors.Blue;
                    break;
                case 1:
                    result = OxyColors.Coral;
                    break;
                case 2:
                    result = OxyColors.GreenYellow;
                    break;
                case 3:
                    result = OxyColors.IndianRed;
                    break;
            }

            return result;
        }

        private FixDivLineSeries FindLineSeries(ObdEntry entry)
        {
            FixDivLineSeries result = null;

            foreach (var series in Model.Series)
            {
                if (series is FixDivLineSeries fixDivLine)
                {
                    if (fixDivLine.Entry.UniqueId == entry.UniqueId)
                    {
                        result = fixDivLine;
                        break;
                    }
                }
            }

            return result;
        }

        private void ClearPlot()
        {
            foreach (var series in Model.Series)
            {
                if (series is FixDivLineSeries fixDivLine)
                {
                    fixDivLine.Points.Clear();
                }
            }

            Model.ResetAllAxes();
            Model.InvalidatePlot(true);
        }

        public async void StartRecorder()
        {
            if (Vcs != null)
            {                               
                if(await EposVcs.StartRecorder())
                {
                    ClearPlot();                    
                }                
            }
        }

        public async void StopRecorder()
        {
            if (Vcs != null)
            {
                await EposVcs.StopRecorder();
            }            
        }

        public async void TriggerRecorder()
        {
            if (Vcs != null)
            {
                await EposVcs.StartRecorder(true);
            }            
        }

        private async Task<bool> IsRecorderRunning()
        {
            bool running = false;

            if (Vcs != null)
            { 
                var executeResult = await EposVcs.IsRecorderRunning();

                if (executeResult != null && executeResult.Result)
                {
                    executeResult.Parameters[0].GetValue(ref running);
                }
            }

            return running;
        }

        public override Task Show()
        {
            UpdateIsRecorderRunningAsync();

            return base.Show();
        }

        #endregion
    }
}
