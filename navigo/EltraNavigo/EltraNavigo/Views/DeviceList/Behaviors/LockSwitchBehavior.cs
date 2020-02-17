using System;
using Xamarin.Forms;

namespace EltraNavigo.Views.DeviceList.Behaviors
{
    public class LockSwitchBehavior : Behavior<Switch>
    {
        private DeviceViewModel _deviceViewModel;
        public Switch AssociatedObject { get; private set; }

        protected override void OnAttachedTo (Switch control)
        {
            base.OnAttachedTo (control);

            AssociatedObject = control;
            
            _deviceViewModel = control.BindingContext as DeviceViewModel;

            control.BindingContextChanged += OnContextChanged;
            control.Toggled += OnToggled;
        }
        
        protected override void OnDetachingFrom (Switch control)
        {
            base.OnDetachingFrom (control);
            
            control.BindingContextChanged += OnContextChanged;
            control.Toggled -= OnToggled;
        }
 
        private void OnContextChanged(object sender, EventArgs e)
        {
            if (AssociatedObject != null)
            {
                _deviceViewModel = AssociatedObject.BindingContext as DeviceViewModel;
            }
        }
        
        private void OnToggled(object sender, ToggledEventArgs e)
        {
            _deviceViewModel?.LockToggle(e.Value);
        }

    }
}
