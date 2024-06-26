using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace EltraMauiCommon.Controls.Parameters
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ParameterEditView : ContentView
    {
        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
                                                         propertyName: "FontSize",
                                                         returnType: typeof(double),
                                                         declaringType: typeof(ParameterEditView),
                                                         defaultValue: 14.0,
                                                         defaultBindingMode: BindingMode.OneWay,
                                                         propertyChanged: FontSizePropertyChanged);

        public ParameterEditView()
        {
            InitializeComponent();
        }

        public double FontSize
        {
            get { return Convert.ToDouble(GetValue(FontSizeProperty)); }
            set { SetValue(FontSizeProperty, value); }
        }

        private static void FontSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ParameterEditView)bindable;

            double fontSize = Convert.ToDouble(newValue);

            control.EditBox.FontSize = fontSize;
            control.Label.FontSize = fontSize;
            control.Unit.FontSize = fontSize;
        }
    }
}