using System;
using System.Text;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS1591

namespace EltraCloud.Pages.ObjectDictionary.Models
{
    public class ParameterModel
    {
        #region Private fields

        private const string DateTimeFormat = "dd MMM HH:mm:ss";

        private readonly ParameterBase _parameter;

        #endregion

        #region Constructors

        public ParameterModel()
        {
        }

        public ParameterModel(ParameterBase parameter)
        {
            _parameter = parameter;
        }

        #endregion

        #region Properties

        [BindProperty]
        public string UniqueId { get; set; }

        [BindProperty]
        public string Index { get; set; }

        [BindProperty]
        public string SubIndex { get; set; }

        public string Name { get; set; }

        public string AccessMode { get; set; }

        public string DataType { get; set; }

        [BindProperty]
        public string Value { get; set; }

        [BindProperty]
        public string Unit { get; set; }

        public string Modified { get; set; }

        public bool IsVisible { get; set; }

        #endregion

        #region Methods

        public void Build()
        {
            UpdateParameterIdentification();

            ConvertAccessMode();

            ConvertDataType();

            UpdateValue(_parameter as Parameter);

            UpdateUnit();

            UpdateVisibility();
        }

        private void UpdateUnit()
        {
            if (_parameter is Epos4Parameter epos4Parameter)
            {
                Unit = epos4Parameter.Unit.Label;
            }
        }

        private void UpdateParameterIdentification()
        {
            UniqueId = _parameter.UniqueId;

            Index = $"0x{_parameter.Index:X4}";
            Name = _parameter.Label;

            if (_parameter is Parameter parameterEntry)
            {
                SubIndex = $"0x{parameterEntry.SubIndex:X2}";
            }
        }

        private void UpdateVisibility()
        {
            if (_parameter is Epos4Parameter epos4Parameter)
            {
                IsVisible = (epos4Parameter.VisibleBy("operator") || 
                             epos4Parameter.VisibleBy("engineer") || 
                             epos4Parameter.VisibleByAny());
            }
            else if (_parameter is StructuredParameter)
            {
                int visibleCount = 0;

                foreach (var subParameter in _parameter.Parameters)
                {
                    if (subParameter is Epos4Parameter epos4SubParameter)
                    {
                        bool isVisible = (epos4SubParameter.VisibleBy("operator") ||
                                          epos4SubParameter.VisibleBy("engineer") ||
                                          epos4SubParameter.VisibleByAny());


                        if (isVisible)
                        {
                            visibleCount++;
                        }
                    }
                }

                if (visibleCount > 1)
                {
                    IsVisible = true;
                }
            }
        }

        public void UpdateValue(Parameter parameter)
        {
            if (parameter is Epos4Parameter parameterEntry)
            {
                var actualValue = parameterEntry.ActualValue;

                if (actualValue != null)
                {
                    FormatValue(actualValue);

                    Modified = actualValue.Modified.ToLongTimeString();
                }
            }
        }

        #region Format text

        private string FormatBooleanValue(ParameterValue actualValue)
        {
            var boolValue = BitConverter.ToBoolean(Convert.FromBase64String(actualValue.Value));

            var result = boolValue ? "true" : "false";

            return result;
        }

        private string FormatByteValue(ParameterValue actualValue)
        {
            string result = string.Empty;
            var byteValues = Convert.FromBase64String(actualValue.Value);
            var byteValue = 0;

            if (_parameter is Epos4Parameter epos4Parameter)
            {
                var multiplier = epos4Parameter.Unit.Multiplier;
                var decimalPlaces = epos4Parameter.Unit.DecimalPlaces;

                if (!string.IsNullOrEmpty(actualValue.Value))
                {
                    byteValue = byteValues[0];
                }

                if (epos4Parameter.DisplayFormat == "hex")
                {
                    result = $"0x{byteValue:X}";
                }
                else
                {
                    result = Math.Abs(multiplier) <= 1
                        ? $"{byteValue}"
                        : FormatDoubleValue(decimalPlaces, byteValue * multiplier);
                }
            }

            result = result.Trim();

            return result;
        }

        private string FormatSByteValue(ParameterValue actualValue)
        {
            string result = string.Empty;
            var sbyteValues = Convert.FromBase64String(actualValue.Value);
            var sbyteValue = 0;

            if (_parameter is Epos4Parameter epos4Parameter)
            {
                var multiplier = epos4Parameter.Unit.Multiplier;
                var decimalPlaces = epos4Parameter.Unit.DecimalPlaces;

                if (!string.IsNullOrEmpty(actualValue.Value))
                {
                    sbyteValue = sbyteValues[0];
                }

                if (epos4Parameter.DisplayFormat == "hex")
                {
                    result = $"0x{sbyteValue:X}";
                }
                else
                {
                    result = Math.Abs(multiplier) <= 1
                        ? $"{sbyteValue}"
                        : FormatDoubleValue(decimalPlaces, sbyteValue * multiplier);
                }
            }

            result = result.Trim();

            return result;
        }

        private string FormatInt16Value(ParameterValue actualValue)
        {
            string result = string.Empty;
            var int16Value = BitConverter.ToInt16(Convert.FromBase64String(actualValue.Value));

            if (_parameter is Epos4Parameter epos4Parameter)
            {
                var multiplier = epos4Parameter.Unit.Multiplier;
                var decimalPlaces = epos4Parameter.Unit.DecimalPlaces;

                if (epos4Parameter.DisplayFormat == "hex")
                {
                    result = $"0x{int16Value:X2}";
                }
                else
                {
                    result = Math.Abs(multiplier) <= 1
                        ? $"{int16Value}"
                        : FormatDoubleValue(decimalPlaces, int16Value * multiplier);
                }
            }

            result = result.Trim();

            return result;
        }

        private string FormatInt32Value(ParameterValue actualValue)
        {
            string result = string.Empty;
            var int32Value = BitConverter.ToInt32(Convert.FromBase64String(actualValue.Value));

            if (_parameter is Epos4Parameter epos4Parameter)
            {
                var multiplier = epos4Parameter.Unit.Multiplier;
                var decimalPlaces = epos4Parameter.Unit.DecimalPlaces;

                if (epos4Parameter.DisplayFormat == "hex")
                {
                    result = $"0x{int32Value:X4}";
                }
                else
                {
                    result = Math.Abs(multiplier) <= 1
                        ? $"{int32Value}"
                        : FormatDoubleValue(decimalPlaces, int32Value * multiplier);
                }
            }

            result = result.Trim();

            return result;
        }

        private string FormatInt64Value(ParameterValue actualValue)
        {
            string result = string.Empty;
            var int64Value = BitConverter.ToInt32(Convert.FromBase64String(actualValue.Value));

            if (_parameter is Epos4Parameter epos4Parameter)
            {
                var multiplier = epos4Parameter.Unit.Multiplier;
                var decimalPlaces = epos4Parameter.Unit.DecimalPlaces;

                if (epos4Parameter.DisplayFormat == "hex")
                {
                    result = $"0x{int64Value:X8}";
                }
                else
                {
                    result = Math.Abs(multiplier) <= 1
                        ? $"{int64Value}"
                        : FormatDoubleValue(decimalPlaces, int64Value * multiplier);
                }
            }

            result = result.Trim();

            return result;
        }

        private string FormatUInt16Value(ParameterValue actualValue)
        {
            string result = string.Empty;
            var uint16Value = BitConverter.ToUInt16(Convert.FromBase64String(actualValue.Value));

            if (_parameter is Epos4Parameter epos4Parameter)
            {
                var multiplier = epos4Parameter.Unit.Multiplier;
                var decimalPlaces = epos4Parameter.Unit.DecimalPlaces;

                if (epos4Parameter.DisplayFormat == "hex")
                {
                    result = $"0x{uint16Value:X2}";
                }
                else
                {
                    result = Math.Abs(multiplier) <= 1
                        ? $"{uint16Value}"
                        : FormatDoubleValue(decimalPlaces, uint16Value * multiplier);
                }
            }

            result = result.Trim();

            return result;
        }

        private string FormatUInt32Value(ParameterValue actualValue)
        {
            string result = string.Empty;
            var uint32Value = BitConverter.ToUInt32(Convert.FromBase64String(actualValue.Value));

            if (_parameter is Epos4Parameter epos4Parameter)
            {
                var multiplier = epos4Parameter.Unit.Multiplier;
                var decimalPlaces = epos4Parameter.Unit.DecimalPlaces;

                if (epos4Parameter.DisplayFormat == "hex")
                {
                    result = $"0x{uint32Value:X4}";
                }
                else
                {
                    result = Math.Abs(multiplier) <= 1
                        ? $"{uint32Value}"
                        : FormatDoubleValue(decimalPlaces, uint32Value * multiplier);
                }
            }

            result = result.Trim();

            return result;
        }

        private string FormatUInt64Value(ParameterValue actualValue)
        {
            string result = string.Empty;
            var uint64Value = BitConverter.ToUInt32(Convert.FromBase64String(actualValue.Value));

            if (_parameter is Epos4Parameter epos4Parameter)
            {
                var multiplier = epos4Parameter.Unit.Multiplier;
                var decimalPlaces = epos4Parameter.Unit.DecimalPlaces;

                if (epos4Parameter.DisplayFormat == "hex")
                {
                    result = $"0x{uint64Value:X8}";
                }
                else
                {
                    result = Math.Abs(multiplier) <= 1
                        ? $"{uint64Value}"
                        : FormatDoubleValue(decimalPlaces, uint64Value * multiplier);
                }
            }

            result = result.Trim();

            return result;
        }

        private string FormatStringValue(ParameterValue actualValue)
        {
            return actualValue.Value;
        }

        private string FormatObjectValue(ParameterValue actualValue)
        {
            string result = string.Empty;

            if (_parameter is Epos4Parameter epos4Parameter)
            {
                switch (epos4Parameter.DataType.SizeInBytes)
                {
                    case 1:
                        var byteValues1 = Convert.FromBase64String(actualValue.Value);
                        var byteValue1 = 0;

                        if (!string.IsNullOrEmpty(actualValue.Value))
                        {
                            byteValue1 = byteValues1[0];
                        }

                        result = $"0x{byteValue1:X}";
                        break;
                    case 2:
                        var uint16Value1 = BitConverter.ToUInt16(Convert.FromBase64String(actualValue.Value));
                        result = $"0x{uint16Value1:X2}";
                        break;
                    case 4:
                        var uint32Value1 = BitConverter.ToUInt32(Convert.FromBase64String(actualValue.Value));
                        result = $"0x{uint32Value1:X4}";
                        break;
                    case 8:
                        var uint64Value1 = BitConverter.ToUInt64(Convert.FromBase64String(actualValue.Value));
                        result = $"0x{uint64Value1:X8}";
                        break;
                }
            }

            result = result.Trim();

            return result;
        }

        private TypeCode GetParameterType()
        {
            TypeCode result = TypeCode.Empty;

            if (_parameter is Epos4Parameter epos4Parameter)
            {
                result = epos4Parameter.DataType.Type;
            }

            return result;
        }

        private void FormatValue(ParameterValue actualValue)
        {
            try
            {              
                switch (GetParameterType())
                {
                    case TypeCode.Boolean:
                        Value = FormatBooleanValue(actualValue);
                        break;
                    case TypeCode.Byte:
                        Value = FormatByteValue(actualValue);
                        break;
                    case TypeCode.SByte:
                        Value = FormatSByteValue(actualValue);
                        break;
                    case TypeCode.Int16:
                        Value = FormatInt16Value(actualValue);
                        break;
                    case TypeCode.Int32:
                        Value = FormatInt32Value(actualValue);
                        break;
                    case TypeCode.Int64:
                        Value = FormatInt64Value(actualValue);
                        break;
                    case TypeCode.UInt16:
                        Value = FormatUInt16Value(actualValue);
                        break;
                    case TypeCode.UInt32:
                        Value = FormatUInt32Value(actualValue);
                        break;
                    case TypeCode.UInt64:
                        Value = FormatUInt64Value(actualValue);
                        break;
                    case TypeCode.String:
                        Value = FormatStringValue(actualValue);
                        break;
                    case TypeCode.Object:
                        Value = FormatObjectValue(actualValue);
                        break;
                    case TypeCode.DateTime:
                        Value = FormatDateTimeValue(actualValue);
                        break;
                    case TypeCode.Double:
                        Value = FormatDoubleValue(actualValue);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private string FormatDateTimeValue(ParameterValue actualValue)
        {
            string result = string.Empty;
            DateTime val = DateTime.MinValue;

            if (actualValue.GetValue(ref val))
            {
                result = val.ToString(DateTimeFormat);
            }

            return result;

        }

        private string FormatDoubleValue(ParameterValue actualValue)
        {
            string result = string.Empty;
            double val = 0;

            if(actualValue.GetValue(ref val))
            {
                result = FormatDoubleValue(2, val);
            }

            return result;
        }

        private string FormatDoubleValue(int decimalPlaces, double val)
        {
            string decimalPlacesFormat = "{0:0.";
            string result;

            if (decimalPlaces > 0)
            {
                for (int i = 0; i < decimalPlaces; i++)
                {
                    decimalPlacesFormat += "#";
                }

                decimalPlacesFormat += "}";

                result = string.Format(decimalPlacesFormat, val);
            }
            else
            {
                result = $"{(long)Math.Abs(val)}";
            }

            return result;
        }

        private void ConvertDataType()
        {
            if (_parameter is Parameter parameterEntry)
            {
                switch (parameterEntry.DataType.Type)
                {
                    case TypeCode.Boolean:
                        DataType = "Boolean";
                        break;
                    case TypeCode.Byte:
                        DataType = "Byte";
                        break;
                    case TypeCode.Char:
                        DataType = "Char";
                        break;
                    case TypeCode.DateTime:
                        DataType = "DateTime";
                        break;
                    case TypeCode.DBNull:
                        DataType = "DBNull";
                        break;
                    case TypeCode.Decimal:
                        DataType = "Decimal";
                        break;
                    case TypeCode.Double:
                        DataType = "Double";
                        break;
                    case TypeCode.Empty:
                        DataType = "Empty";
                        break;
                    case TypeCode.Int16:
                        DataType = "Int16";
                        break;
                    case TypeCode.Int32:
                        DataType = "Int32";
                        break;
                    case TypeCode.Int64:
                        DataType = "Int64";
                        break;
                    case TypeCode.Object:
                        DataType = "Object";
                        break;
                    case TypeCode.SByte:
                        DataType = "SByte";
                        break;
                    case TypeCode.Single:
                        DataType = "Single";
                        break;
                    case TypeCode.String:
                        DataType = "String";
                        break;
                    case TypeCode.UInt16:
                        DataType = "UInt16";
                        break;
                    case TypeCode.UInt32:
                        DataType = "UInt32";
                        break;
                    case TypeCode.UInt64:
                        DataType = "UInt64";
                        break;
                }
            }
        }

        private void ConvertAccessMode()
        {
            if (_parameter is Parameter parameterEntry)
            {
                switch (parameterEntry.Flags.Access)
                {
                    case EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common.AccessMode.ReadOnly:
                        AccessMode = "RO";
                        break;
                    case EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common.AccessMode.ReadWrite:
                        AccessMode = "RW";
                        break;
                    case EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common.AccessMode.WriteOnly:
                        AccessMode = "WO";
                        break;
                }
            }
        }

        #endregion

        #endregion
    }
}
