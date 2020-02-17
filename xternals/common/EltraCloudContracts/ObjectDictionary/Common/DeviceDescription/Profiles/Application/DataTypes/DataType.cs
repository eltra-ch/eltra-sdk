using System;
using System.Runtime.Serialization;

namespace EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes
{
    [DataContract]
    public class DataType
    {
        private TypeCode _type;
        private event EventHandler TypeChanged;

        #region Constructors

        public DataType()
        {
        }

        public DataType(int type)
        {
            Type = (TypeCode)type;
        }
        
        public DataType(int type, uint sizeInBytes)
        {
            Type = (TypeCode)type;

            if(SizeInBytes != sizeInBytes && sizeInBytes > 0)
            {
                SizeInBytes = sizeInBytes;
            }
        }

        #endregion

        #region Properties

        [DataMember]
        public TypeCode Type
        {
            get => _type;
            set
            {
                _type = value;
                OnTypeChanged();
            }
        }

        [DataMember]
        public uint SizeInBytes { get; set; }

        [DataMember]
        public uint SizeInBits { get; set; }

        public DataTypeReference Reference { get; set; }

        #endregion

        #region Events handling

        protected virtual void OnTypeChanged()
        {
            switch (_type)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Char:
                    SizeInBits = 0;
                    SizeInBytes = sizeof(byte);
                    break;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    SizeInBits = 0;
                    SizeInBytes = sizeof(ushort);
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    SizeInBits = 0;
                    SizeInBytes = sizeof(uint);
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    SizeInBits = 0;
                    SizeInBytes = sizeof(long);
                    break;
                case TypeCode.Double:
                    SizeInBits = 0;
                    SizeInBytes = sizeof(double);
                    break;
                case TypeCode.Boolean:
                    SizeInBytes = 0;
                    SizeInBits = 1;
                    break;
                case TypeCode.Object:
                case TypeCode.String:
                    break;
                default:
                    SizeInBits = 0;
                    SizeInBytes = 0;
                    break;
            }

            TypeChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public virtual bool Parse()
        {
            return false;
        }

        public bool Clone(DataType source)
        {
            Type = source.Type;
            SizeInBytes = source.SizeInBytes;
            SizeInBits = source.SizeInBits;
            
            return true;
        }
        
        #endregion
    }
}
