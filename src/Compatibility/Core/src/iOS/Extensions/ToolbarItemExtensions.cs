using System.ComponentModel;
using CoreGraphics;
using ObjCRuntime;
using UIKit;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public static class ToolbarItemExtensions
	{
		public static UIBarButtonItem ToUIBarButtonItem(this ToolbarItem item, bool forceName = false, bool forcePrimary = false) =>
			Microsoft.Maui.Controls.Platform.ToolbarItemExtensions.ToNative(item, forceName, forcePrimary);
	}
}