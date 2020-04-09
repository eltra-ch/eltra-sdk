using Xamarin.Forms;
using System;

namespace EltraNotKauf.Controls.Behaviors
{
    internal class ButtonBehaviors : Behavior<Xamarin.Forms.Button>
    {
        private Xamarin.Forms.Button _view;
        private ToolViewModel _viewModel;

        protected override void OnAttachedTo(Xamarin.Forms.Button view)
        {
            _view = view;

            _view.Pressed += OnButtonPressed;
            _view.Released += OnButtonReleased;
            _view.BindingContextChanged += OnBindingContextChanged;

            base.OnAttachedTo(view);
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            _view = sender as Xamarin.Forms.Button;
            _viewModel = _view?.BindingContext as ToolViewModel;
        }

        private void OnButtonReleased(object sender, EventArgs e)
        {
            _viewModel.ButtonReleased(_view.ClassId);
        }

        private void OnButtonPressed(object sender, EventArgs e)
        {
            _viewModel.ButtonPressed(_view.ClassId);
        }

        protected override void OnDetachingFrom(Xamarin.Forms.Button bindable)
        {
            _view.Pressed -= OnButtonPressed;
            _view.Released -= OnButtonReleased;

            base.OnDetachingFrom(bindable);
        }
    }
}
