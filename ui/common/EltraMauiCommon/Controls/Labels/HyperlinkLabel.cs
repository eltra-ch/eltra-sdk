using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace EltraMauiCommon.Controls.Labels
{
    public class HyperlinkLabel : Label
    {
        public static readonly BindableProperty UrlProperty =
            BindableProperty.Create(nameof(Url), typeof(string), typeof(HyperlinkLabel), null);

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public HyperlinkLabel()
        {
            TextDecorations = TextDecorations.Underline;
            TextColor = Colors.Blue;

            GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await Browser.Default.OpenAsync(Url, BrowserLaunchMode.SystemPreferred))
            });
        }
    }
}
