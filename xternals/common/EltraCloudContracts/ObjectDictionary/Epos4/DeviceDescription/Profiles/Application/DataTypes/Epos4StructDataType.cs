using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.DataTypes
{
    class Epos4StructDataType : Epos4DataTypeReference
    {
        #region Private fields

        private readonly DataTypeList _dataTypeList;
        private readonly XmlNode _source;
        private List<VarDeclaration> _varDeclarations;

        public List<VarDeclaration> VarDeclarations { get => _varDeclarations ?? (_varDeclarations = new List<VarDeclaration>()); }

        #endregion

        #region Constructors

        public Epos4StructDataType(XmlNode source, DataTypeList dataTypeList)
        {
            _source = source;
            _dataTypeList = dataTypeList;
        }

        #endregion

        #region Properties
        
        public uint SizeInBytes { get; set; }

        public uint SizeInBits { get; set; }

        #endregion

        #region Methods

        public bool GetStructVariableValue<T>(string variableId, ParameterValue parameterValue, out T value)
        {
            bool result = false;

            value = default;

            uint offsetInBytes = 0;

            var parameterValueByteArray = Convert.FromBase64String(parameterValue.Value);

            if (parameterValueByteArray.Length > 0)
            {
                foreach (var varDeclaration in VarDeclarations)
                {
                    if (varDeclaration.UniqueId == variableId)
                    {
                        byte[] bytes = new byte[varDeclaration.DataType.SizeInBytes];

                        Array.Copy(parameterValueByteArray, offsetInBytes, bytes, offsetInBytes,
                            varDeclaration.DataType.SizeInBytes);

                        try
                        {
                            result = varDeclaration.GetValue(parameterValueByteArray, out value);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        
                        break;
                    }

                    offsetInBytes += varDeclaration.DataType.SizeInBytes;
                }
            }

            return result;
        }

        public override bool Parse()
        {
            bool result = true;

            if (_source.Attributes != null)
            {
                UniqueId = _source.Attributes["uniqueID"].InnerXml;
                Name = _source.Attributes["name"].InnerXml;
            }

            foreach (XmlNode childNode in _source.ChildNodes)
            {
                if (childNode.Name == "varDeclaration")
                {
                    var varDeclaration = new VarDeclaration(_dataTypeList);
                    
                    if (!varDeclaration.Parse(childNode))
                    {
                        result = false;
                        break;
                    }
                    
                    VarDeclarations.Add(varDeclaration);
                }
            }

            if (result && VarDeclarations.Count > 0)
            {
                result = CalculateSizes();
            }

            return result;
        }

        private bool CalculateSizes()
        {
            bool result = true;

            const int bitsInByte = 8;
            uint sizeInBits = 0;
            uint sizeInBytes = 0;

            foreach (var varDeclaration in VarDeclarations)
            {
                var dataType = varDeclaration.DataType;
                if (dataType != null)
                {
                    if (dataType.SizeInBits > 0)
                    {
                        sizeInBits += dataType.SizeInBits;
                    }
                    else
                    {
                        sizeInBytes += dataType.SizeInBytes;
                    }
                }
            }

            if (sizeInBytes == 0 && sizeInBits == 0)
            {
                result = false;
            }

            if (sizeInBits > 0 && (sizeInBits % bitsInByte) != 0)
            {
                //fix
                while ((sizeInBits % bitsInByte) != 0)
                {
                    sizeInBits++;
                }
            }

            if (sizeInBits > 0 && sizeInBytes == 0)
            {
                sizeInBytes = sizeInBits / bitsInByte;
            }

            if (result)
            {
                SizeInBytes = sizeInBytes;
                SizeInBits = sizeInBits;
            }

            return result;
        }

        #endregion

        
    }
}
