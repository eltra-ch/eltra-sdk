using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Behaviors;
using System.ComponentModel;

namespace EltraMauiCommon.Controls.Labels
{
    public class LabelLink : ContentView
    {
        //private P42.Uno.Controls.Label _label;
        private Label _label;
        private string _htmlText;

        public static readonly BindableProperty PhoneProperty =  BindableProperty.Create("Phone", typeof(string), typeof(EventToCommandBehavior), null, propertyChanged: OnPhoneChanged);
        
        public static readonly BindableProperty EmailProperty = BindableProperty.Create("Email", typeof(string), typeof(EventToCommandBehavior), null, propertyChanged: OnEmailChanged);

        public LabelLink()
        {
            /*_label = new P42.Uno.Controls.Label
            {   
            };*/

            _label = new Label
            {   
            };

            /*TODO 
             * _label.ActionTagTapped += (object sender, P42.Uno.Controls.ActionTagEventArgs e) =>
            {
                var id = e.Id;
                var href = e.Href;
                var uri = new Uri(e.Href);

                try
                {
                    string prefix = "tel:";
                    if (e.Href.StartsWith(prefix))
                    {
                        PhoneDialer.Open(e.Href.Substring(prefix.Length));
                    }
                }
                catch (ArgumentNullException)
                {   
                }
                catch (FeatureNotSupportedException)
                {
                }
                catch (Exception)
                {
                }
            };*/

            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                Children = {
                    _label
                }
            };

            PropertyChanged += OnLabelPropertyChanged;
        }

        static void OnPhoneChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if(bindable is LabelLink labelLink)
            {
                labelLink.HtmlText = $"<a id=\"phone\" href=\"tel:{newValue.ToString()}\">{newValue.ToString()}</a>";
            }
        }

        static void OnEmailChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LabelLink labelLink)
            {
                labelLink.HtmlText = $"<a id=\"email\" href=\"mailto:{newValue.ToString()}\">{newValue.ToString()}</a>";
            }
        }

        private void OnLabelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HtmlText")
            {
                _label.TextType = Microsoft.Maui.TextType.Html;
                _label.Text = HtmlText;
            }
        }

        public string Phone
        {
            get { return (string)GetValue(PhoneProperty); }
            set { SetValue(PhoneProperty, value); }
        }

        public string Email
        {
            get { return (string)GetValue(EmailProperty); }
            set { SetValue(EmailProperty, value); }
        }

        public string HtmlText
        {
            get => _htmlText;
            set
            {
                _htmlText = value;
                OnPropertyChanged("HtmlText");
            }
        }
    }
}
