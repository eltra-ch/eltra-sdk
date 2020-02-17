using System.Collections.Generic;

namespace EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters
{
    class ParameterComparer : IComparer<ParameterBase>
    {
        #region Methods 
        private int CompareParameter(Parameter x, Parameter y)
        {
            int result = 0;

            if (x == null)
            {
                if (y == null)
                {
                    result = 0;
                }
                else
                {
                    result = -1;
                }
            }
            else if (y != null && x.Index == y.Index && x.SubIndex == y.SubIndex)
            {
                result = 0;
            }
            else
            {
                if (y == null)
                {
                    result = -1;
                }
                else
                {
                    if (y.Index > x.Index)
                    {
                        result = -1;
                    }
                    else if (y.Index < x.Index)
                    {
                        result = 1;
                    }
                    else
                    {
                        if (y.SubIndex > x.SubIndex)
                        {
                            result = -1;
                        }
                        else if (y.SubIndex < x.SubIndex)
                        {
                            result = 1;
                        }
                    }
                }
            }

            return result;
        }

        private int CompareStructuredParameter(StructuredParameter x, StructuredParameter y)
        {
            int result = 0;

            if (x == null)
            {
                if (y == null)
                {
                    result = 0;
                }
                else
                {
                    result = -1;
                }
            }
            else if (y != null && x.Index == y.Index)
            {
                result = 0;
            }
            else
            {
                if (y == null)
                {
                    result = -1;
                }
                else
                {
                    if (y.Index > x.Index)
                    {
                        result = -1;
                    }
                    else if (y.Index < x.Index)
                    {
                        result = 1;
                    }
                }
            }

            return result;
        }

        public int Compare(ParameterBase x, ParameterBase y)
        {
            int result = 1;

            if (x is Parameter px && y is Parameter py)
            {
                result = CompareParameter(px, py);
            }
            else if (x is StructuredParameter spx && y is StructuredParameter spy)
            {
                result = CompareStructuredParameter(spx, spy);
            }
            else if(x is Parameter px1 && y is StructuredParameter spy1)
            {
                result = CompareParameterToStructuredParameter(px1, spy1);
            }
            else if (x is StructuredParameter sx1 && y is Parameter py1)
            {
                result = CompareStructuredParameterToParameter(sx1, py1);
            }

            return result;
        }

        private int CompareStructuredParameterToParameter(StructuredParameter x, Parameter y)
        {
            int result = 0;

            if (x == null)
            {
                if (y == null)
                {
                    result = 0;
                }
                else
                {
                    result = -1;
                }
            }
            else if (y != null && x.Index == y.Index)
            {
                result = -1;
            }
            else
            {
                if (y == null)
                {
                    result = -1;
                }
                else
                {
                    if (y.Index > x.Index)
                    {
                        result = -1;
                    }
                    else if (y.Index < x.Index)
                    {
                        result = 1;
                    }
                }
            }

            return result;
        }

        private int CompareParameterToStructuredParameter(Parameter x, StructuredParameter y)
        {
            int result = 0;

            if (x == null)
            {
                if (y == null)
                {
                    result = 0;
                }
                else
                {
                    result = -1;
                }
            }
            else if (y != null && x.Index == y.Index)
            {
                result = -1;
            }
            else
            {
                if (y == null)
                {
                    result = -1;
                }
                else
                {
                    if (y.Index > x.Index)
                    {
                        result = -1;
                    }
                    else if (y.Index < x.Index)
                    {
                        result = 1;
                    }
                }
            }

            return result;
        }
        
        #endregion
    }
}
