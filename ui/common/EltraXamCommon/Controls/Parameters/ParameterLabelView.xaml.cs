using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EltraXamCommon.Controls.Parameters
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ParameterLabelView : ContentView
    {
        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(NamedSize), typeof(ParameterLabelView), null, propertyChanged: (bindingObject, oldValue, newValue) =>
        {
            SetFontSize(bindingObject, newValue);
        });

        private static void SetFontSize(BindableObject bindingObject, object newValue)
        {
            FontSizeConverter myFontSizeConverter = new FontSizeConverter();

            var fontSize = (double)myFontSizeConverter.ConvertFromInvariantString(newValue.ToString());

            ((ParameterLabelView)bindingObject).ContentControl.FontSize = fontSize;
            ((ParameterLabelView)bindingObject).LabelControl.FontSize = fontSize;
            ((ParameterLabelView)bindingObject).UnitControl.FontSize = fontSize;
        }

        public NamedSize FontSize
        {
            get { return (NamedSize)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public ParameterLabelView()
        {
            InitializeComponent();

            SetDefaultFontSize();
        }

        private void SetDefaultFontSize()
        {
            SetFontSize(this, "Default");
        }
    }
}