using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace EltraMauiCommon.Controls.Helpers
{
    internal class UwpHelper
    {
		public static void FixElements<T>(List<T> elements)
		{
            if (DeviceInfo.Current.Platform != DevicePlatform.WinUI)
			{
				return;
			}

            MainThread.BeginInvokeOnMainThread(() =>
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

		public static void DeepSearch<T>(IReadOnlyList<IVisualTreeElement> children, ref List<T> entries)
		{
			foreach (var child in children)
			{
				if (child is T entry)
				{
					entries.Add(entry);
				}
				else if (child is Layout layout)
				{
                    DeepSearch(layout.GetVisualTreeDescendants(), ref entries);
				}
			}
		}

		
	}
}
