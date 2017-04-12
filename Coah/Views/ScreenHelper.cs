using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace Linearstar.Coah.Views
{
	class ScreenHelper
	{
		readonly Screen screen;

		public Int32Rect Bounds
		{
			get
			{
				var rt = screen.Bounds;

				return new Int32Rect(rt.Left, rt.Top, rt.Width, rt.Height);
			}
		}

		public Int32Rect WorkingArea
		{
			get
			{
				var rt = screen.WorkingArea;

				return new Int32Rect(rt.Left, rt.Top, rt.Width, rt.Height);
			}
		}

		public string DeviceName => screen.DeviceName;
		public bool IsPrimary => screen.Primary;
		public int BitsPerPixel => screen.BitsPerPixel;

		public ScreenHelper(Screen screen) =>
			this.screen = screen;

		public static IList<ScreenHelper> GetScreens() =>
			Screen.AllScreens.Select(_ => new ScreenHelper(_)).ToArray();

		public static ScreenHelper GetPrimaryScreen() =>
			new ScreenHelper(Screen.PrimaryScreen);

		public static ScreenHelper GetScreenFromWindow(Window window)
		{
			var wih = new WindowInteropHelper(window);

			return new ScreenHelper(Screen.FromHandle(wih.Handle));
		}

		public static Point GetTransformToDevice(Visual contextVisual)
		{
			var source = PresentationSource.FromVisual(contextVisual);

			return new Point(source.CompositionTarget.TransformToDevice.M11, source.CompositionTarget.TransformToDevice.M22);
		}
	}
}
