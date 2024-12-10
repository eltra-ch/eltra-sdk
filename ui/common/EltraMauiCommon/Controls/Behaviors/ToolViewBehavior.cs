using EltraUiCommon.Controls;
using EltraUiCommon.Helpers;
using EltraMauiCommon.Controls.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EltraMauiCommon.Controls.Behaviors
{
    public class ToolViewBehavior : Behavior<ContentView>
    {
        private ContentView _page;
        private ToolViewModel _toolViewModel;
        
        protected override void OnAttachedTo(ContentView page)
        {
            _page = page;

            if (_page != null)
            {
                _page.BindingContextChanged += OnPageBindingContextChanged;
                _page.SizeChanged += OnPageLayoutChanged;
            }

            base.OnAttachedTo(page);
        }

        protected override void OnDetachingFrom(ContentView bindable)
        {
            if (_page != null)
            {
                _page.BindingContextChanged -= OnPageBindingContextChanged;
                _page.SizeChanged -= OnPageLayoutChanged;
            }

            base.OnDetachingFrom(bindable);
        }

        private void OnPageLayoutChanged(object sender, EventArgs e)
        {
            if (_toolViewModel != null && !_toolViewModel.IsVisible)
            {
                //_toolViewModel?.Show();
            }

            _page?.Focus();
        }

        private void FixControls()
        {
            var entries = new List<Entry>();
            
            UwpHelper.DeepSearch(_page.Children, ref entries);
            
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
