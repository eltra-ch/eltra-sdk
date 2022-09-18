using EltraUiCommon.Controls;
using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EltraWpfCommon.Controls.Behaviors
{
    public class ToolViewBehavior : Behavior<UserControl>
    {        
        private ToolViewModel _toolViewModel;
        
        protected override void OnAttached()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.DataContextChanged += OnDataContextChanged;
                AssociatedObject.LayoutUpdated += OnPageLayoutChanged;
            }

            base.OnAttached();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject!= null && AssociatedObject.DataContext is ToolViewModel model)
            {
                _toolViewModel = model;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.DataContextChanged -= OnDataContextChanged;
                AssociatedObject.LayoutUpdated -= OnPageLayoutChanged;
            }

            base.OnDetaching();
        }

        private void OnPageLayoutChanged(object sender, EventArgs e)
        {
            AssociatedObject?.Focus();
        }
    }
}
