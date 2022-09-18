using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EltraUiCommon.Controls.Parameters;
using Microsoft.Xaml.Behaviors;

namespace EltraXamCommon.Controls.Parameters.Behaviors 
{    
    public class ParameterValidationBehavior : Behavior<TextBox> 
    {
        #region Private fields

        private ParameterEditViewModel _viewModel;
        private string _text;

        #endregion

        #region Events handling

        private async void OnCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text != _text)
                {
                    _text = textBox.Text;

                    await _viewModel.TextChanged(_text);
                }
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (sender as TextBox)?.DataContext as ParameterEditViewModel;
        }

        private async void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text != _text)
                {
                    _text = textBox.Text;

                    if (!string.IsNullOrWhiteSpace(_text))
                    {
                        if (_viewModel != null && IsNumberDataType(_viewModel))
                        {
                            var isValid = IsValidNumber(_text);

                            _viewModel.IsValid = isValid;

                            if (!isValid)
                            {
                                var text = _text.Remove(_text.Length - 1);

                                ((TextBox)sender).Text = text;
                            }
                            else
                            {
                                await _viewModel.TextChanged(_text);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        protected override void OnAttached()
        {
            AssociatedObject.TextChanged += OnEntryTextChanged;
            AssociatedObject.DataContextChanged += OnDataContextChanged;
            AssociatedObject.ManipulationCompleted += OnCompleted;

            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= OnEntryTextChanged;
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
            AssociatedObject.ManipulationCompleted -= OnCompleted;

            base.OnDetaching();
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

        #endregion
    }
}