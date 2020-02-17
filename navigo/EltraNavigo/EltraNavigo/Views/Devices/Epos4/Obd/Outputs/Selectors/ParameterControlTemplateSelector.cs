using EltraNavigo.Controls.Parameters;
using Xamarin.Forms;

namespace EltraNavigo.Views.Obd.Outputs.Selectors
{
    public class ParameterControlTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EditDataTemplate { get; set; }

        public DataTemplate ReadOnlyDataTemplate { get; set; }

        #endregion

        #region Methods

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            DataTemplate result = ReadOnlyDataTemplate;

            if (item is ParameterEditViewModel)
            {
                result = EditDataTemplate;
            }

            return result;
        }

        #endregion
    }
}
