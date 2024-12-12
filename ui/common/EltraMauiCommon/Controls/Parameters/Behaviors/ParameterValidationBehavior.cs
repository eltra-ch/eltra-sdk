using System;
using Microsoft.Maui.Controls;
using EltraUiCommon.Controls.Parameters;

namespace EltraMauiCommon.Controls.Parameters.Behaviors 
{    
    public class ParameterValidationBehavior : Behavior<Entry> {

        protected override void OnAttachedTo(Entry entry) 
        {
            entry.TextChanged += OnEntryTextChanged;
            
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry) 
        {
            entry.TextChanged -= OnEntryTextChanged;
            
            base.OnDetachingFrom(entry);
        }

        private static bool IsNumberDataType(ParameterEditViewModel viewModel)
        {
            bool result = false;

            if (viewModel != null)
            {
                var parameter = viewModel.Parameter;

                if (parameter != null && parameter.DataType != null)
                {
                    result = true;

                    var dataType = parameter.DataType;

                    if (dataType.Type == TypeCode.String || 
                        dataType.Type == TypeCode.Object || 
                        dataType.Type == TypeCode.DBNull ||
                        dataType.Type == TypeCode.DateTime ||
                        dataType.Type == TypeCode.Empty ||
                        dataType.Type == TypeCode.Boolean)
                    {
                        result = false;
                    }
                }
            }
            
            return result;
        }

        private async static void OnEntryTextChanged(object sender, TextChangedEventArgs args) 
        {
            if(!string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                if (((Entry)sender).BindingContext is ParameterEditViewModel viewModel && IsNumberDataType(viewModel))
                {
                    var isValid = IsValidNumber(args.NewTextValue, viewModel);

                    viewModel.IsValid = isValid;

                    if (!isValid)
                    {
                        var text = args.NewTextValue.Remove(args.NewTextValue.Length - 1);

                        ((Entry)sender).Text = text;
                    }
                    else
                    {
                        await viewModel.TextChanged(args.NewTextValue);
                    }
                }
            }
        }

        private static bool IsValidNumber(string textValue, ParameterEditViewModel viewModel)
        {
            bool isValid = false;

            if (textValue.Length > 0)
            {
                isValid = true;
                var parameter = viewModel?.Parameter;

                if (parameter != null)
                {
                    var type = parameter.DataType.Type;

                    switch(type)
                    {
                        case TypeCode.Byte:
                            isValid = byte.TryParse(textValue, out var _);
                            break;
                        case TypeCode.SByte:
                            isValid = sbyte.TryParse(textValue, out var _);
                            break;
                        case TypeCode.Int64:
                            isValid = long.TryParse(textValue, out var _);
                            break;
                        case TypeCode.UInt64:
                            isValid = ulong.TryParse(textValue, out var _);
                            break;
                        case TypeCode.Int32:
                            isValid = int.TryParse(textValue, out var _);
                            break;
                        case TypeCode.UInt32:
                            isValid = uint.TryParse(textValue, out var _);
                            break;
                        case TypeCode.Int16:
                            isValid = short.TryParse(textValue, out var _);
                            break;
                        case TypeCode.UInt16:
                            isValid = ushort.TryParse(textValue, out var _);
                            break;
                        case TypeCode.Single:
                            isValid = Single.TryParse(textValue, out var _);
                            break;
                        case TypeCode.Double:
                            isValid = double.TryParse(textValue, out double _);
                            break;
                    }
                }
            }

            return isValid;
        }
    }
}