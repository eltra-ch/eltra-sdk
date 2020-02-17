using System;
using System.Runtime.Serialization;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters
{
    [DataContract]
    public class ParameterValue : IEquatable<ParameterValue>
    {
        private string _value;

        public ParameterValue()
        {
        }

        public ParameterValue(byte[] data)
        {
            Value = Convert.ToBase64String(data, Base64FormattingOptions.None);
        }

        public ParameterValue(ParameterValue parameterValue)
        {
            Value = parameterValue.Value;
        }

        public ParameterValue(TypeCode type, string defaultValue)
        {
            SetDefaultValue(type, defaultValue);
        }

        public ParameterValue(DataType type, string defaultValue)
        {
            SetDefaultValue(type, defaultValue);
        }

        [DataMember]
        public string Value
        {
            get => _value ?? (_value = string.Empty);
            set
            {
                _value = value;
                OnValueChanged();
            }
        }

        [DataMember]
        public DateTime Modified { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(Value);
        
        public bool Equals(ParameterValue other)
        {
            return other != null && other.Value == Value;
        }

        public ParameterValue Clone()
        {
            return new ParameterValue(this);
        }

        public bool SetDefaultValue(DataType dataType, string defaultValue)
        {
            var result = SetDefaultValue(dataType.Type, defaultValue);

            return result;
        }

        private bool SetDefaultValue(TypeCode type, string defaultValue)
        {
            bool result = true;

            if (!string.IsNullOrEmpty(defaultValue))
            {
                if (type == TypeCode.String)
                {
                    Value = defaultValue;
                }
                else
                {
                    if (defaultValue.StartsWith("0x"))
                    {
                        switch (type)
                        {
                            case TypeCode.Byte:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToByte(defaultValue.Substring(2), 16)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.SByte:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToSByte(defaultValue.Substring(2), 16)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.UInt16:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToUInt16(defaultValue.Substring(2), 16)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.UInt32:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToUInt32(defaultValue.Substring(2), 16)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.UInt64:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToUInt64(defaultValue.Substring(2), 16)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Int16:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToInt16(defaultValue.Substring(2), 16)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Int32:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToInt32(defaultValue.Substring(2), 16)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Int64:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToInt64(defaultValue.Substring(2), 16)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Object:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToInt64(defaultValue.Substring(2), 16)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Boolean:
                                result = false;
                                break;
                            default:
                                result = false;
                                break;
                        }
                    }
                    else
                    {
                        switch (type)
                        {
                            case TypeCode.Byte:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToByte(defaultValue)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.SByte:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToSByte(defaultValue)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.UInt16:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToUInt16(defaultValue)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.UInt32:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToUInt32(defaultValue)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.UInt64:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToUInt64(defaultValue)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Int16:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToInt16(defaultValue)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Int32:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToInt32(defaultValue)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Int64:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToInt64(defaultValue)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Object:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToInt64(defaultValue)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Double:
                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToDouble(defaultValue)), Base64FormattingOptions.None);
                                break;
                            case TypeCode.Boolean:

                                if (defaultValue == "0")
                                {
                                    defaultValue = "false";
                                }
                                else if (defaultValue == "1")
                                {
                                    defaultValue = "true";
                                }

                                Value = Convert.ToBase64String(BitConverter.GetBytes(Convert.ToBoolean(defaultValue)), Base64FormattingOptions.None);
                                break;
                            default:
                                result = false;
                                break;
                        }

                    }
                }
            }

            return result;
        }

        public bool SetValue<T>(T value)
        {
            bool result = true;

            if (typeof(T) == typeof(bool))
            {
                var byteArray = BitConverter.GetBytes((bool)(object)value);

                Value = Convert.ToBase64String(byteArray);

            }
            else if (typeof(T) == typeof(byte))
            {
                var byteArray = BitConverter.GetBytes((byte)(object)value);

                Value = Convert.ToBase64String(byteArray);
            }
            else if (typeof(T) == typeof(sbyte))
            {
                var byteArray = BitConverter.GetBytes((sbyte)(object)value);

                Value = Convert.ToBase64String(byteArray);
            }
            else if (typeof(T) == typeof(char))
            {
                var byteArray = BitConverter.GetBytes((char)(object)value);

                Value = Convert.ToBase64String(byteArray);
            }
            else if (typeof(T) == typeof(Int16))
            {
                var byteArray = BitConverter.GetBytes((Int16)(object)value);

                Value = Convert.ToBase64String(byteArray);
            }
            else if (typeof(T) == typeof(Int32))
            {
                var byteArray = BitConverter.GetBytes((Int32)(object)value);

                Value = Convert.ToBase64String(byteArray);
            }
            else if (typeof(T) == typeof(Int64))
            {
                var byteArray = BitConverter.GetBytes((Int64)(object)value);

                Value = Convert.ToBase64String(byteArray);
            }
            else if (typeof(T) == typeof(UInt16))
            {
                var byteArray = BitConverter.GetBytes((UInt16)(object)value);

                Value = Convert.ToBase64String(byteArray);
            }
            else if (typeof(T) == typeof(UInt32))
            {
                var byteArray = BitConverter.GetBytes((UInt32)(object)value);

                Value = Convert.ToBase64String(byteArray);
            }
            else if (typeof(T) == typeof(UInt64))
            {
                var byteArray = BitConverter.GetBytes((UInt64)(object)value);

                Value = Convert.ToBase64String(byteArray);
            }
            else if (typeof(T) == typeof(double))
            {
                var byteArray = BitConverter.GetBytes((double)(object)value);

                Value = Convert.ToBase64String(byteArray);
            }
            else if (typeof(T) == typeof(String))
            {
                string s = (string)(object)value;

                Value = s;
            }
            else if (typeof(T) == typeof(byte[]))
            {
                byte[] s = (byte[])(object)value;

                Value = Convert.ToBase64String(s);
            }
            else if (typeof(T) == typeof(DateTime))
            {
                DateTime dt = (DateTime)(object)value;
                Value = Convert.ToBase64String(BitConverter.GetBytes(dt.Ticks));
            }
            else
            {
                result = false;
            }

            return result;
        }

        public bool GetValue<T>(ref T value)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(Value))
            {
                if (typeof(T) == typeof(bool))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)(byteArray[0] > 0);
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(byte))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)byteArray[0];
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(sbyte))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        sbyte[] signed = Array.ConvertAll(byteArray, b => unchecked((sbyte)b));
                        value = (T)(object)signed[0];
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(char))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)BitConverter.ToChar(byteArray, 0);
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(Int16))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)BitConverter.ToInt16(byteArray, 0);
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(Int32))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)BitConverter.ToInt32(byteArray, 0);
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(Int64))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)BitConverter.ToInt64(byteArray, 0);
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(UInt16))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)BitConverter.ToUInt16(byteArray, 0);
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(UInt32))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)BitConverter.ToUInt32(byteArray, 0);
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(UInt64))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)BitConverter.ToUInt64(byteArray, 0);
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(double))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)BitConverter.ToDouble(byteArray, 0);
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(String))
                {
                    value = (T)(object)Value;
                    result = true;
                }
                else if (typeof(T) == typeof(byte[]))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        value = (T)(object)byteArray;
                        result = true;
                    }
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    var byteArray = Convert.FromBase64String(Value);
                    if (byteArray.Length > 0)
                    {
                        long dateData = BitConverter.ToInt64(byteArray, 0);

                        var val = DateTime.FromBinary(dateData);

                        value = (T)(object)val;

                        result = true;
                    }
                }
            }
            
            return result;
        }

        protected virtual void OnValueChanged()
        {
            Modified = DateTime.Now;
        }
    }
}
