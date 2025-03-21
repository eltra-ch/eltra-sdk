﻿using Microsoft.Maui.Controls;
using EltraUiCommon.Controls.Parameters;

namespace EltraMauiCommon.Controls.Parameters.Triggers
{
    class StepperValueChangedTrigger : TriggerAction<Stepper>
    {
        protected override async void Invoke(Stepper sender)
        {
            if (sender.BindingContext is ParameterEditViewModel viewModel)
            {
                double value = sender.Value;

                await viewModel.ValueChanged(value);
            }
        }
    }
}
