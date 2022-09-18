using EltraUiCommon.Controls.Parameters;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace EltraXamCommon.Controls.Parameters.Behaviors
{
    public class ParameterComboBehavior : Behavior<ComboBox>
    {
        #region Private fields

        private ParameterComboViewModel _viewModel;

        #endregion

        #region Events handling

        private void OnSelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.SelectedIndexChanged();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (sender as ComboBox)?.DataContext as ParameterComboViewModel;
        }

        #endregion

        #region Methods

        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += OnSelectedIndexChanged;
            AssociatedObject.DataContextChanged += OnDataContextChanged;

            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectedIndexChanged;
            AssociatedObject.DataContextChanged -= OnDataContextChanged;

            base.OnDetaching();
        }

        #endregion
    }
}
