using System;
using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters
{
    public class XddRange
    {
        private List<XddValueEntry> _minValues;
        private List<XddValueEntry> _maxValues;

        public List<XddValueEntry> MinValues { get => _minValues ?? (_minValues = new List<XddValueEntry>()); }
        public List<XddValueEntry> MaxValues { get => _maxValues ?? (_maxValues = new List<XddValueEntry>()); }
        
        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "minValue")
                {
                    var minValue = new XddValueEntry();

                    if (!minValue.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    MinValues.Add(minValue);
                }
                else if (childNode.Name == "maxValue")
                {
                    var minValue = new XddValueEntry();

                    if (!minValue.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    MaxValues.Add(minValue);
                }
            }
            
            return result;
        }

        public void Limit<T>(ref T minValue, ref T maxValue)
        {
            foreach (var minVal in MinValues)
            {
                if (minVal.Value != null)
                {
                    if (typeof(T) == typeof(byte))
                    {
                        byte rawValue = (byte)(object)(minValue);

                        if (rawValue < minVal.Value.ToByte())
                        {
                            rawValue = minVal.Value.ToByte();

                            minValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(sbyte))
                    {
                        sbyte rawValue = (sbyte)(object)(minValue);

                        if (rawValue < minVal.Value.ToSByte())
                        {
                            rawValue = minVal.Value.ToSByte();

                            minValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(short))
                    {
                        short rawValue = (short)(object)(minValue);

                        if (rawValue < minVal.Value.ToShort())
                        {
                            rawValue = minVal.Value.ToShort();

                            minValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(ushort))
                    {
                        ushort rawValue = (ushort)(object)(minValue);

                        if (rawValue < minVal.Value.ToUShort())
                        {
                            rawValue = minVal.Value.ToUShort();

                            minValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    } 
                    else if (typeof(T) == typeof(int))
                    {
                        int rawValue = (int)(object)(minValue);

                        if (rawValue < minVal.Value.ToInt())
                        {
                            rawValue = minVal.Value.ToInt();

                            minValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(uint))
                    {
                        uint rawValue = (uint)(object)(minValue);

                        if (rawValue < minVal.Value.ToUInt())
                        {
                            rawValue = minVal.Value.ToUInt();

                            minValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(long))
                    {
                        long rawValue = (long)(object)(minValue);

                        if (rawValue < minVal.Value.ToLong())
                        {
                            rawValue = minVal.Value.ToLong();

                            minValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(ulong))
                    {
                        ulong rawValue = (ulong)(object)(minValue);

                        if (rawValue < minVal.Value.ToULong())
                        {
                            rawValue = minVal.Value.ToULong();

                            minValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(double))
                    {
                        double rawValue = (double)(object)(minValue);

                        if (rawValue < minVal.Value.ToDouble())
                        {
                            rawValue = minVal.Value.ToDouble();

                            minValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                }
            }

            foreach (var maxVal in MaxValues)
            {
                if (maxVal.Value != null)
                {
                    if (typeof(T) == typeof(byte))
                    {
                        byte rawValue = (byte)(object)(maxValue);

                        if (rawValue > maxVal.Value.ToByte())
                        {
                            rawValue = maxVal.Value.ToByte();

                            maxValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(sbyte))
                    {
                        sbyte rawValue = (sbyte)(object)(maxValue);

                        if (rawValue > maxVal.Value.ToSByte())
                        {
                            rawValue = maxVal.Value.ToSByte();

                            maxValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    } else if (typeof(T) == typeof(short))
                    {
                        short rawValue = (short)(object)(minValue);

                        if (rawValue > maxVal.Value.ToShort())
                        {
                            rawValue = maxVal.Value.ToShort();

                            maxValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(ushort))
                    {
                        ushort rawValue = (ushort)(object)(minValue);

                        if (rawValue > maxVal.Value.ToUShort())
                        {
                            rawValue = maxVal.Value.ToUShort();

                            maxValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    } 
                    else if (typeof(T) == typeof(int))
                    {
                        long rawValue = (int)(object)(maxValue);

                        if (rawValue > maxVal.Value.ToInt())
                        {
                            rawValue = maxVal.Value.ToInt();

                            maxValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(uint))
                    {
                        uint rawValue = (uint)(object)(maxValue);

                        if (rawValue > maxVal.Value.ToUInt())
                        {
                            rawValue = maxVal.Value.ToUInt();

                            maxValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    } else if (typeof(T) == typeof(long))
                    {
                        long rawValue = (long)(object)(maxValue);

                        if (rawValue > maxVal.Value.ToLong())
                        {
                            rawValue = maxVal.Value.ToLong();

                            maxValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(ulong))
                    {
                        ulong rawValue = (ulong)(object)(maxValue);

                        if (rawValue > maxVal.Value.ToULong())
                        {
                            rawValue = maxVal.Value.ToULong();

                            maxValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(double))
                    {
                        double rawValue = (double)(object)(maxValue);

                        if (rawValue > maxVal.Value.ToDouble())
                        {
                            rawValue = maxVal.Value.ToDouble();

                            maxValue = (T)Convert.ChangeType(rawValue, typeof(T));
                        }
                    }
                }
            }
        }

        
    }
}
