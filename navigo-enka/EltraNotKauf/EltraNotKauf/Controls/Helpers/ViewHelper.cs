using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace EltraNotKauf.Controls.Helpers
{
	internal static class ViewHelper
    {
		public static void InvalidateSize(this VisualElement view)
		{
			if (view != null)
			{
				var viewType = typeof(VisualElement);
				var methods = viewType.GetTypeInfo().DeclaredMethods;

				var method = methods.FirstOrDefault(m => m.Name == "InvalidateMeasure");

				if (method != null)
				{
					method.Invoke(view, null);
				}
			}
		}
	}
}
