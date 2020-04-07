using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace EltraNotKauf.Controls.Helpers
{
	internal class UwpHelper
    {
		public static void FixElements<T>(List<T> elements)
		{
			if (Device.RuntimePlatform != Device.UWP)
			{
				return;
			}

			Device.BeginInvokeOnMainThread(async () =>
			{
				foreach (var element in elements)
				{
					await Task.Delay(100).ConfigureAwait(true);

					if (element is VisualElement visualElement)
					{
						/*var h = visualElement.Width;

						visualElement.WidthRequest = h - 1;
						
						await Task.Delay(10).ConfigureAwait(true);

						visualElement.WidthRequest = h;

						await Task.Delay(10).ConfigureAwait(true);*/

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
