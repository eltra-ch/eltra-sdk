using EltraUiCommon.Controls;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace EltraWpfCommon.Controls.Behaviors
{
    public class ToolViewShowBehavior : Behavior<UserControl>
    {
        private ToolViewBaseModel _viewModel;
        
        protected override void OnAttached()
        {
            _viewModel = AssociatedObject.DataContext as ToolViewBaseModel;

            AssociatedObject.DataContextChanged += OnDataContextChanged;
            
            base.OnAttached();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = AssociatedObject?.DataContext as ToolViewBaseModel;            
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
            
            base.OnDetaching();
        }        
    }
}
