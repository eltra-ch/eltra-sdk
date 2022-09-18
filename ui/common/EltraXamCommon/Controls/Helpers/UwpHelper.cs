using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace EltraXamCommon.Controls.Helpers
{
    internal class UwpHelper
    {
		public static void FixElements<T>(List<T> elements)
		{
			if (Device.RuntimePlatform != Device.UWP)
			{
				return;
			}

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				foreach (var element in elements)
				{
					Task.Delay(100);

					if (element is VisualElement visualElement)
					{
						visualElement.InvalidateSize();
					}
				}

			});
		}

		public static void DeepSearch<T>(List<Element> children, ref List<T> entries)
		{
			foreach (var child in children)
			{
				if (child is T entry)
				{
					entries.Add(entry);
				}
				else if (child is Layout layout)
				{
					DeepSearch(layout.LogicalChildren.ToList(), ref entries);
				}
			}
		}

		
	}
}
