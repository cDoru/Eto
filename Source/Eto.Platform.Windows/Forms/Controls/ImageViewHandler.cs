using System;
using SWF = System.Windows.Forms;
using SD = System.Drawing;
using Eto.Forms;
using Eto.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Eto.Platform.Windows.Drawing;

namespace Eto.Platform.Windows.Forms
{
	public class ImageViewHandler : WindowsControl<SWF.PictureBox, ImageView>, IImageView
	{
		Image image;
		bool sizeSet;
		
		public ImageViewHandler ()
		{
			Control = new SWF.PictureBox {
				BorderStyle = SWF.BorderStyle.None,
				SizeMode = SWF.PictureBoxSizeMode.Zoom
			};
		}

		public override Size Size
		{
			get
			{
				return base.Size;
			}
			set
			{
				base.Size = value;
				sizeSet = true;
				SetImage ();
			}
		}
		void SetImage ()
		{
			if (image != null) {
				var handler = image.Handler as IWindowsImage;
				if (handler != null)
					Control.Image = handler.GetImageWithSize (null);
				else
					Control.Image = null;
			}
			else
				Control.Image = null;

			if (!sizeSet && Control.Image != null)
				Control.Size = Control.Image.Size;
		}

		public Image Image {
			get {
				return image;
			}
			set {
				image = value;
				SetImage ();
			}
		}
	}
}

