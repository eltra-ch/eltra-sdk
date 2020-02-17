using System;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common
{
    public class Epos4Value
    {
        #region Private fields

        private readonly ParameterValue _actualValue;
        private readonly DataType _dataType;

        #endregion

        #region Constructors

        public Epos4Value()
        {
        }

        public Epos4Value(ParameterValue actualValue, DataType dataType)
        {
            _actualValue = actualValue;
            _dataType = dataType;

            Value = _actualValue.Value;
        }

        #endregion

        #region Properties

        public ParameterValue ActualValue => _actualValue;

        public string Value { get; set; }

        protected DataType DataType => _dataType;

        #endregion

        #region Methods

        public bool Parse(XmlNode childNode)
        {
            bool result = false;
            string valueAsString = childNode.InnerText;
            long rawValue = 0;

            if (valueAsString.StartsWith("0x"))
            {
                rawValue = Convert.ToInt64(valueAsString.Substring(2), 16);
                result = true;
            }
            else if (long.TryParse(childNode.InnerText, out var value))
            {
                rawValue = value;
                result = true;
            }

            if (result)
            {
                Value = Convert.ToBase64String(BitConverter.GetBytes(rawValue), Base64FormattingOptions.None);
            }

            return result;
        }

        public double ToDouble()
        {
            double result = double.NaN;
            
            try
            {
                var byteArray = Convert.FromBase64String(Value);

                if (DataType != null)
                {
                    if (TypeCode.Double == DataType.Type)
                    {
                        result = BitConverter.ToInt32(byteArray, 0);
                    }
                    else
                    {
                        MsgLogger.WriteError("Epos4Value - ToDouble", $"Cannot convert base64 value = {Value}, to data type {DataType.Type}!");                        
                    }
                }
                else
                {
                    MsgLogger.WriteWarning("Epos4Value - ToDouble", $"Cannot convert base64 value = {Value}, unknown data type!");

                    result = BitConverter.ToInt32(byteArray, 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Epos4Value - ToDouble", e);
            }

            return result;
        }

        public long ToLong()
        {
            long result = long.MinValue;

            try
            {
                var byteArray = Convert.FromBase64String(Value);

                if (DataType != null)
                {
                    switch (DataType.Type)
                    {
                        case TypeCode.Byte:
                            result = BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.SByte:
                            result = BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.Int16:
                            result = BitConverter.ToInt16(byteArray, 0);
                            break;
                        case TypeCode.Int32:
                            result = BitConverter.ToInt32(byteArray, 0);
                            break;
                        case TypeCode.Int64:
                            result = BitConverter.ToInt64(byteArray, 0);
                            break;
                        default:
                            MsgLogger.WriteError("Epos4Value - ToLong", $"Cannot convert base64 value = {Value}, to data type {DataType.Type}!");
                            break;
                    }
                }
                else
                {
                    MsgLogger.WriteWarning("Epos4Value - ToLong", $"Cannot convert base64 value = {Value}, unknown data type!");

                    result = BitConverter.ToInt64(byteArray, 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Epos4Value - ToLong", e);
            }
            
            return result;
        }

        public ulong ToULong()
        {
            ulong result = ulong.MinValue;

            try
            {
                var byteArray = Convert.FromBase64String(Value);

                if (DataType != null)
                {
                    switch (DataType.Type)
                    {
                        case TypeCode.Byte:
                            result = BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.SByte:
                            result = BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.UInt16:
                            result = BitConverter.ToUInt16(byteArray, 0);
                            break;
                        case TypeCode.UInt32:
                            result = BitConverter.ToUInt32(byteArray, 0);
                            break;
                        case TypeCode.UInt64:
                            result = BitConverter.ToUInt64(byteArray, 0);
                            break;
                        default:
                            MsgLogger.WriteError("Epos4Value - ToULong", $"Cannot convert base64 value = {Value}, to data type {DataType.Type}!");
                            break;
                    }
                }
                else
                {
                    MsgLogger.WriteWarning("Epos4Value - ToULong", $"Cannot convert base64 value = {Value}, unknown data type!");

                    result = BitConverter.ToUInt64(byteArray, 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Epos4Value - ToULong", e);
            }

            return result;
        }

        public int ToInt()
        {
            int result = int.MinValue;

            try
            {
                var byteArray = Convert.FromBase64String(Value);

                if (DataType != null)
                {
                    switch (DataType.Type)
                    {
                        case TypeCode.Byte:
                            result = BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.SByte:
                            result = BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.Int16:
                            result = BitConverter.ToInt16(byteArray, 0);
                            break;
                        case TypeCode.Int32:
                            result = BitConverter.ToInt32(byteArray, 0);
                            break;
                        default:
                            MsgLogger.WriteError("Epos4Value - ToInt", $"Cannot convert base64 value = {Value}, to data type {DataType.Type}!");
                            break;
                    }
                }
                else
                {
                    MsgLogger.WriteWarning("Epos4Value - ToInt", $"Cannot convert base64 value = {Value}, unknown data type!");

                    result = BitConverter.ToInt32(byteArray, 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Epos4Value - ToInt", e);
            }

            return result;
        }

        public uint ToUInt()
        {
            uint result = uint.MinValue;

            try
            {
                var byteArray = Convert.FromBase64String(Value);

                if (DataType != null)
                {
                    switch (DataType.Type)
                    {
                        case TypeCode.Byte:
                            result = BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.SByte:
                            result = BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.UInt16:
                            result = BitConverter.ToUInt16(byteArray, 0);
                            break;
                        case TypeCode.UInt32:
                            result = BitConverter.ToUInt32(byteArray, 0);
                            break;
                        default:
                            MsgLogger.WriteError("Epos4Value - ToUInt", $"Cannot convert base64 value = {Value}, to data type {DataType.Type}!");
                            break;
                    }
                }
                else
                {
                    MsgLogger.WriteWarning("Epos4Value - ToUInt", $"Cannot convert base64 value = {Value}, unknown data type!");

                    result = BitConverter.ToUInt32(byteArray, 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Epos4Value - ToUInt", e);
            }

            return result;
        }

        public short ToShort()
        {
            short result = short.MinValue;

            try
            {
                var byteArray = Convert.FromBase64String(Value);

                if (DataType != null)
                {
                    switch (DataType.Type)
                    {
                        case TypeCode.Byte:
                            result = (short)BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.SByte:
                            result = (short)BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.Int16:
                            result = BitConverter.ToInt16(byteArray, 0);
                            break;
                        default:
                            MsgLogger.WriteError("Epos4Value - ToShort", $"Cannot convert base64 value = {Value}, to data type {DataType.Type}!");
                            break;
                    }
                }
                else
                {
                    MsgLogger.WriteWarning("Epos4Value - ToShort", $"Cannot convert base64 value = {Value}, unknown data type!");

                    result = BitConverter.ToInt16(byteArray, 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Epos4Value - ToShort", e);
            }

            return result;
        }

        public ushort ToUShort()
        {
            ushort result = ushort.MinValue;

            try
            {
                var byteArray = Convert.FromBase64String(Value);

                if (DataType != null)
                {
                    switch (DataType.Type)
                    {
                        case TypeCode.Byte:
                            result = BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.SByte:
                            result = BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.UInt16:
                            result = BitConverter.ToUInt16(byteArray, 0);
                            break;
                        default:
                            MsgLogger.WriteError("Epos4Value - ToUShort", $"Cannot convert base64 value = {Value}, to data type {DataType.Type}!");
                            break;
                    }
                }
                else
                {
                    MsgLogger.WriteWarning("Epos4Value - ToUShort", $"Cannot convert base64 value = {Value}, unknown data type!");

                    result = BitConverter.ToUInt16(byteArray, 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Epos4Value - ToUShort", e);
            }

            return result;
        }

        public byte ToByte()
        {
            byte result = byte.MinValue;

            try
            {
                var byteArray = Convert.FromBase64String(Value);

                if (DataType != null)
                {
                    switch (DataType.Type)
                    {
                        case TypeCode.Byte:
                            result = (byte)BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.SByte:
                            result = (byte)BitConverter.ToChar(byteArray, 0);
                            break;
                        default:
                            MsgLogger.WriteError("Epos4Value - ToByte", $"Cannot convert base64 value = {Value}, to data type {DataType.Type}!");
                            break;
                    }
                }
                else
                {
                    MsgLogger.WriteWarning("Epos4Value - ToByte", $"Cannot convert base64 value = {Value}, unknown data type!");

                    result = (byte)BitConverter.ToChar(byteArray, 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Epos4Value - ToByte", e);
            }

            return result;
        }

        public sbyte ToSByte()
        {
            sbyte result = sbyte.MinValue;

            try
            {
                var byteArray = Convert.FromBase64String(Value);

                if (DataType != null)
                {
                    switch (DataType.Type)
                    {
                        case TypeCode.Byte:
                            result = (sbyte)BitConverter.ToChar(byteArray, 0);
                            break;
                        case TypeCode.SByte:
                            result = (sbyte)BitConverter.ToChar(byteArray, 0);
                            break;
                        default:
                            MsgLogger.WriteError("Epos4Value - ToUbyte", $"Cannot convert base64 value = {Value}, to data type {DataType.Type}!");
                            break;
                    }
                }
                else
                {
                    MsgLogger.WriteWarning("Epos4Value - ToUbyte", $"Cannot convert base64 value = {Value}, unknown data type!");

                    result = (sbyte)BitConverter.ToChar(byteArray, 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Epos4Value - ToUbyte", e);
            }

            return result;
        }

        #endregion
    }
}
