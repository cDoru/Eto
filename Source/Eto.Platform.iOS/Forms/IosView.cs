using System;
using Eto.Drawing;
using Eto.Forms;
using Eto.Platform.iOS.Drawing;
using SD = System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Eto.Platform.Mac.Forms;

namespace Eto.Platform.iOS.Forms
{
	public interface IIosView
	{
		Size PositionOffset { get; }
		UIViewController Controller { get; }
	}

	public abstract class IosView<TControl, TWidget> : MacObject<TControl, TWidget>, IControl, IIosView, IMacAutoSizing
		where TControl: UIResponder
		where TWidget: Control
	{
		Size? naturalSize;
		UIViewController controller;

		public UIViewController Controller
		{ 
			get { return controller ?? (controller = CreateController()); } 
			protected set { controller = value; }
		}

		protected virtual UIViewController CreateController()
		{
			return new RotatableViewController { View = ContainerControl };
		}

		public virtual UIView ContentControl { get { return ContainerControl; } }

		public virtual UIView EventControl { get { return ContainerControl; } }

		public abstract UIView ContainerControl { get; }

		public virtual bool AutoSize { get; protected set; }

		public Size? PreferredSize { get; set; }

		public virtual Size MinimumSize { get; set; }

		public virtual Size? MaximumSize { get; set; }

		public virtual Size Size
		{
			get { return ContainerControl.Frame.Size.ToEtoSize(); }
			set
			{ 
				var oldSize = GetPreferredSize(Size.MaxValue);
				this.PreferredSize = value;

				var newSize = ContainerControl.Frame.Size;
				if (value.Width >= 0)
					newSize.Width = value.Width;
				if (value.Height >= 0)
					newSize.Height = value.Height;
				ContainerControl.SetFrameSize(newSize);

				AutoSize = value.Width == -1 && value.Height == -1;
				CreateTracking();
				LayoutIfNeeded(oldSize);
			}
		}

		protected virtual bool LayoutIfNeeded(Size? oldPreferredSize = null, bool force = false)
		{
			naturalSize = null;
			if (Widget.Loaded)
			{
				var oldSize = oldPreferredSize ?? ContainerControl.Frame.Size.ToEtoSize();
				var newSize = GetPreferredSize(Size.MaxValue);
				if (newSize != oldSize || force)
				{
					var container = Widget.Parent.GetMacContainer();
					if (container != null)
						container.LayoutParent(true);
					return true;
				}
			}
			return false;
		}

		protected virtual Size GetNaturalSize(Size availableSize)
		{
			if (naturalSize != null)
				return naturalSize.Value;
			var control = Control as UIView;
			if (control != null)
			{
				SD.SizeF? size = (Widget.Loaded) ? (SD.SizeF?)control.Frame.Size : null;
				control.SizeToFit();
				naturalSize = control.Frame.Size.ToEtoSize();
				if (size != null)
					control.SetFrameSize(size.Value);
				return naturalSize.Value;
			}
			return Size.Empty;
		}

		public virtual Size GetPreferredSize(Size availableSize)
		{
			var size = GetNaturalSize(availableSize);
			if (!AutoSize && PreferredSize != null)
			{
				var preferredSize = PreferredSize.Value;
				if (preferredSize.Width >= 0)
					size.Width = preferredSize.Width;
				if (preferredSize.Height >= 0)
					size.Height = preferredSize.Height;
			}
			if (MinimumSize != Size.Empty)
				size = Size.Max(size, MinimumSize);
			if (MaximumSize != null)
				size = Size.Min(size, MaximumSize.Value);
			return size;
		}

		protected IosView()
		{
			this.AutoSize = true;
		}

		public virtual Size PositionOffset { get { return Size.Empty; } }

		void CreateTracking()
		{
			/*
			 * use TOUCHES
			if (!mouseMove)
				return;
			 if (tracking != null)
				Control.RemoveTrackingArea (tracking);
			
			mouseDelegate = new MouseDelegate{ Widget = this.Widget, View = Control };
			tracking = new NSTrackingArea (new SD.RectangleF (new SD.PointF (0, 0), Control.Frame.Size), 
				NSTrackingAreaOptions.ActiveAlways | NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.EnabledDuringMouseDrag, 
			    mouseDelegate, 
				new NSDictionary ());
			Control.AddTrackingArea (tracking);
			*/
		}

		public virtual void SetParent(Container parent)
		{
		}

		static readonly NSString frameKey = new NSString("frame");

		public override void AttachEvent(string handler)
		{
			switch (handler)
			{
				case Eto.Forms.Control.MouseDownEvent:
				case Eto.Forms.Control.MouseUpEvent:
				case Eto.Forms.Control.MouseDoubleClickEvent:
				case Eto.Forms.Control.MouseEnterEvent:
				case Eto.Forms.Control.MouseLeaveEvent:
				case Eto.Forms.Control.KeyDownEvent:
				case Eto.Forms.Control.GotFocusEvent:
				case Eto.Forms.Control.LostFocusEvent:
					break;
				case Eto.Forms.Control.MouseMoveEvent:
				//mouseMove = true;
					CreateTracking();
					break;
				case Eto.Forms.Control.SizeChangedEvent:
					this.AddControlObserver(frameKey, e =>
					{
						var h = e.Handler as IosView<TControl,TWidget>;
						h.Widget.OnSizeChanged(EventArgs.Empty);
					});
				/*UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
				this.AddObserver(null, UIDevice.OrientationDidChangeNotification, delegate {
					Widget.OnSizeChanged (EventArgs.Empty);
				});*/
				/*Control.Window.PostsFrameChangedNotifications = true;
				this.AddObserver (UIView.UIViewFrameDidChangeNotification, delegate {
					Widget.OnSizeChanged (EventArgs.Empty); 
				});*/
					break;
				default:
					base.AttachEvent(handler);
					break;
			}
		}

		public virtual void Invalidate()
		{
			EventControl.SetNeedsDisplay();
		}

		public virtual void Invalidate(Rectangle rect)
		{
			EventControl.SetNeedsDisplayInRect(rect.ToSDRectangleF());
		}

		public Graphics CreateGraphics()
		{
			throw new NotSupportedException();
		}

		public virtual void SuspendLayout()
		{
		}

		public virtual void ResumeLayout()
		{
		}

		public void Focus()
		{
			Control.BecomeFirstResponder();
		}

		public virtual Color BackgroundColor
		{
			get { return ContainerControl.BackgroundColor.ToEto(); }
			set { ContainerControl.BackgroundColor = value.ToUI(); }
		}

		public virtual bool Enabled
		{
			get { return EventControl.UserInteractionEnabled; }
			set { EventControl.UserInteractionEnabled = value; }
		}

		public bool HasFocus
		{
			get { return Control.IsFirstResponder; }
		}

		public bool Visible
		{
			get { return !ContainerControl.Hidden; }
			set { ContainerControl.Hidden = !value; }
		}

		public virtual Font Font
		{ 
			get;
			set;
		}

		public virtual void OnPreLoad(EventArgs e)
		{
		}

		public virtual void OnLoad(EventArgs e)
		{
		}

		public virtual void OnLoadComplete(EventArgs e)
		{
		}

		public virtual void OnUnLoad(EventArgs e)
		{
		}

		public void MapPlatformAction(string systemAction, BaseAction action)
		{
		}

		public PointF PointFromScreen(PointF point)
		{
			var sdpoint = point.ToSD();
			sdpoint = ContainerControl.ConvertPointFromView(sdpoint, null);
			sdpoint.Y = ContainerControl.Frame.Height - sdpoint.Y;
			return sdpoint.ToEto();
		}

		public PointF PointToScreen(PointF point)
		{
			var sdpoint = point.ToSD();
			sdpoint.Y = ContainerControl.Frame.Height - sdpoint.Y;
			sdpoint = ContainerControl.ConvertPointToView(sdpoint, null);
			return sdpoint.ToEto();
		}

		public Point Location
		{
			get { return ContainerControl.Frame.Location.ToEtoPoint(); }
		}
	}
}

