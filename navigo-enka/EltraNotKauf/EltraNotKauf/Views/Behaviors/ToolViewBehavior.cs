using EltraNotKauf.Controls;
using EltraNotKauf.Controls.Helpers;
using EltraNotKauf.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace EltraNotKauf.Views.Behaviors
{
    class ToolViewBehavior : Behavior<ContentView>
    {
        private ContentView _page;
        private ToolViewModel _toolViewModel;
        
        protected override void OnAttachedTo(ContentView page)
        {
            _page = page;

            page.BindingContextChanged += OnPageBindingContextChanged;

            base.OnAttachedTo(page);

            _page.LayoutChanged += (sender, args) => { OnPageAppearing(sender, args); };
        }

        private void OnPageAppearing(object sender, EventArgs e)
        {
            if (_toolViewModel != null && !_toolViewModel.IsVisible)
            {
                _toolViewModel?.Show();
            }

            _page?.Focus();            
        }

        private void FixControls()
        {
            var entries = new List<Entry>();
            
            UwpHelper.DeepSearch(_page.Children.ToList(), ref entries);
            
            UwpHelper.FixElements(entries);
        }

        private void OnPageBindingContextChanged(object sender, EventArgs e)
        {
            if (_page.BindingContext is ToolViewModel model)
            {
                _toolViewModel = model;

                _toolViewModel.VisibilityChanged += OnViewModelVisibilityChanged;
            }
        }

        private void OnViewModelVisibilityChanged(object sender, EventArgs e)
        {
            if (_toolViewModel.IsVisible)
            {
                ThreadHelper.RunOnMainThread(()=> {
                    FixControls();
                });
            }
        }
    }
}
