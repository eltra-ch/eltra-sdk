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

                    if (dataType.Type == TypeCode.String || dataType.Type == TypeCode.Object)
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
                    var isValid = IsValidNumber(args.NewTextValue);

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

        private static bool IsValidNumber(string textValue)
        {
            var chars = textValue.ToCharArray();
            bool isValid = false;

            if (chars.Length > 0)
            {
                isValid = true;
                foreach (var character in chars)
                {
                    if (!char.IsDigit(character) && character != '-')
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            return isValid;
        }
    }
}