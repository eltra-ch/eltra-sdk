using EltraUiCommon.Controls;
using System;
using Xamarin.Forms;

namespace EltraXamCommon.Controls.Behaviors
{
    public class ToolViewShow : Behavior<ContentView>
    {
        private ToolViewBaseModel _viewModel;
        private ContentView _page;

        protected override void OnAttachedTo(ContentView page)
        {
            _page = page;
            _viewModel = page.BindingContext as ToolViewBaseModel;

            page.BindingContextChanged += OnBindingContextChanged;
            
            if(_viewModel!=null)
            {
                _viewModel.VisibilityChanged += OnControlVisibilityChanged;
            }

            base.OnAttachedTo(page);
        }

        private void OnControlVisibilityChanged(object sender, EventArgs e)
        {            
        }

        protected override void OnDetachingFrom(ContentView page)
        {
            _page = page;

            page.BindingContextChanged -= OnBindingContextChanged;
            
            base.OnDetachingFrom(page);
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            _page = sender as ContentView;
            _viewModel = _page?.BindingContext as ToolViewBaseModel;

            if (_viewModel != null)
            {
                _viewModel.VisibilityChanged += OnControlVisibilityChanged;
            }
        }        
    }
}
