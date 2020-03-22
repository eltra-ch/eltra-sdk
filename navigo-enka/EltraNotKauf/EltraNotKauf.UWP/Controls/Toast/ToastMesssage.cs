using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using EltraNotKauf.Controls.Toast;

namespace EltraNotKauf.UWP.Controls.Toast
{
    public class ToastMessage : IToastMessage {
        
        private const double LongDelay = 3.5;
        private const double ShortDelay = 2.0;

        public void LongAlert(string message) =>
            ShowMessage(message, LongDelay);

        public void ShortAlert(string message) =>
            ShowMessage(message, ShortDelay);

        private void ShowMessage(string message, double duration) 
        {
            var label = new TextBlock 
            {
                Text = message,
                Foreground = new SolidColorBrush(Windows.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            
            var style = new Style { TargetType = typeof(FlyoutPresenter) };
            
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Windows.UI.Colors.Black)));
            style.Setters.Add(new Setter(FrameworkElement.MaxHeightProperty, 1));
            
            var flyout = new Flyout 
            {
                Content = label,
                Placement = FlyoutPlacementMode.Full,
                FlyoutPresenterStyle = style,
            };

            flyout.ShowAt(Window.Current.Content as FrameworkElement);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(duration) };
            timer.Tick += (sender, e) => {
                timer.Stop();
                flyout.Hide();
            };

            timer.Start();
        }
    }
}
