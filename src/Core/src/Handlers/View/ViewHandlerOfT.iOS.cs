using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		UIView? INativeViewHandler.NativeView => this.GetWrappedNativeView();
		UIView? INativeViewHandler.ContainerView => ContainerView;

		public new WrapperView? ContainerView
		{
			get => (WrapperView?)base.ContainerView;
			protected set => base.ContainerView = value;
		}

		public UIViewController? ViewController { get; set; }

		public override void NativeArrange(Rectangle rect)
		{
			var nativeView = this.GetWrappedNativeView();

			if (nativeView == null)
				return;

			// We set Center and Bounds rather than Frame because Frame is undefined if the CALayer's transform is 
			// anything other than the identity (https://developer.apple.com/documentation/uikit/uiview/1622459-transform)
			nativeView.Center = new CoreGraphics.CGPoint(rect.Center.X, rect.Center.Y);

			// The position of Bounds is usually (0,0), but in some cases (e.g., UIScrollView) it's the content offset.
			// So just leave it a whatever value iOS thinks it should be.
			nativeView.Bounds = new CoreGraphics.CGRect(nativeView.Bounds.X, nativeView.Bounds.Y, rect.Width, rect.Height);

			Invoke(nameof(IView.Frame), rect);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetWrappedNativeView().GetDesiredSize(VirtualView, widthConstraint, heightConstraint);

		protected override void SetupContainer()
		{
			if (NativeView == null || ContainerView != null)
				return;

			var oldParent = (UIView?)NativeView.Superview;

			var oldIndex = oldParent?.IndexOfSubview(NativeView);
			NativeView.RemoveFromSuperview();

			ContainerView ??= new WrapperView(NativeView.Bounds);
			ContainerView.AddSubview(NativeView);

			if (oldIndex is int idx && idx >= 0)
				oldParent?.InsertSubview(ContainerView, idx);
			else
				oldParent?.AddSubview(ContainerView);
		}

		protected override void RemoveContainer()
		{
			if (NativeView == null || ContainerView == null || NativeView.Superview != ContainerView)
				return;

			var oldParent = (UIView?)ContainerView.Superview;

			var oldIndex = oldParent?.IndexOfSubview(ContainerView);
			ContainerView.RemoveFromSuperview();

			ContainerView = null;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.InsertSubview(NativeView, idx);
			else
				oldParent?.AddSubview(NativeView);
		}
	}
}