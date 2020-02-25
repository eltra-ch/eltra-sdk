using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraNavigo.Views.Obd.Outputs;
using OxyPlot;
using OxyPlot.Series;
using System;

namespace EltraNavigo.Views.DataRecorder.Series
{
    class FixDivLineSeries : LineSeries
    {
        private readonly ObdEntry _entry;
        private readonly XddParameter _parameter;

        public FixDivLineSeries(ObdEntry entry, OxyColor lineColor)
        {
            _entry = entry;
            _parameter = entry.ParameterEntry as XddParameter;

            Title = entry.Name;

            Color = lineColor;
            
            LineJoin = LineJoin.Bevel;
            LineStyle = LineStyle.Solid;

            /*MarkerType = MarkerType.Diamond;
            MarkerSize = 6;
            MarkerStroke = OxyColors.White;
            MarkerFill = lineColor;
            MarkerStrokeThickness = 1.5;
            */
            if (GetRange(out var minValue, out var maxValue))
            {
                //MinY = minValue;
                //MaxY = maxValue;
            }
        }

        public ObdEntry Entry => _entry;

        public byte ChannelIndex { get; set; }

        private bool GetRange(out double min, out double max)
        {
            bool result = false;

            min = 0;
            max = 0;

            if (_parameter != null)
            {
                switch (_parameter.DataType.Type)
                {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        long minvalue = 0;
                        long maxvalue = 0;

                        if (_parameter.GetRange(ref minvalue, ref maxvalue))
                        {
                            min = minvalue;
                            max = maxvalue;

                            result = true;
                        }
                    break;
                }
            }

            return result;
        }
    }
}
