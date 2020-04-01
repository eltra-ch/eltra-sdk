using Xamarin.Forms;

namespace EltraNotKauf.Controls.Behaviors
{
    internal class EntryBehaviour : Behavior<Entry>
    {
        private Entry _view;

        protected override void OnAttachedTo(Entry view)
        {
            _view = view;

            _view.TextChanged += OnTextChanged;

            base.OnAttachedTo(view);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(!_view.IsFocused)
            {
                _view.Focus();
            }
        }
    }
}
