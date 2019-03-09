using System;
using System.Windows;
using System.Windows.Interop;

namespace Linearstar.Coah.Views
{
	static class WindowOptions
	{
		static readonly PropertyChangedCallback UpdateProperty = (sender, e) =>
		{
            if (!(sender is Window w))
                return;

            w.SourceInitialized -= SourceInitialized;
			w.SourceInitialized += SourceInitialized;
		};
		public static readonly DependencyProperty ShowIconProperty = DependencyProperty.RegisterAttached("ShowIcon", typeof(bool?), typeof(WindowOptions), new UIPropertyMetadata(null, UpdateProperty));
		public static readonly DependencyProperty MaximizeBoxProperty = DependencyProperty.RegisterAttached("MaximizeBox", typeof(bool?), typeof(WindowOptions), new UIPropertyMetadata(null, UpdateProperty));
		public static readonly DependencyProperty MinimizeBoxProperty = DependencyProperty.RegisterAttached("MinimizeBox", typeof(bool?), typeof(WindowOptions), new UIPropertyMetadata(null, UpdateProperty));

		const int WS_MINIMIZEBOX = 0x20000;
		const int WS_MAXIMIZEBOX = 0x10000;
		const int WS_EX_DLGMODALFRAME = 0x0001;

		[AttachedPropertyBrowsableForType(typeof(Window))]
		public static bool? GetShowIcon(DependencyObject obj) =>
			(bool?)obj.GetValue(ShowIconProperty);

		[AttachedPropertyBrowsableForType(typeof(Window))]
		public static void SetShowIcon(DependencyObject obj, bool? value) =>
			obj.SetValue(ShowIconProperty, value);

		[AttachedPropertyBrowsableForType(typeof(Window))]
		public static bool? GetMaximizeBox(DependencyObject obj) =>
			(bool?)obj.GetValue(MaximizeBoxProperty);

		[AttachedPropertyBrowsableForType(typeof(Window))]
		public static void SetMaximizeBox(DependencyObject obj, bool? value) =>
			obj.SetValue(MaximizeBoxProperty, value);

		[AttachedPropertyBrowsableForType(typeof(Window))]
		public static bool? GetMinimizeBox(DependencyObject obj) =>
			(bool?)obj.GetValue(MinimizeBoxProperty);

		[AttachedPropertyBrowsableForType(typeof(Window))]
		public static void SetMinimizeBox(DependencyObject obj, bool? value) =>
			obj.SetValue(MinimizeBoxProperty, value);

		static void SourceInitialized(object sender, EventArgs e)
		{
			var w = (Window)sender;
			var hWnd = new WindowInteropHelper(w).Handle;

			var windowStyle = hWnd.GetWindowLong(NativeMethods.GWL_STYLE).ToInt32();
			var extendedStyle = hWnd.GetWindowLong(NativeMethods.GWL_EXSTYLE).ToInt32();

			if (GetMaximizeBox(w) is bool maximizeBox)
				if (maximizeBox)
					windowStyle |= WS_MAXIMIZEBOX;
				else
					windowStyle &= ~WS_MAXIMIZEBOX;

			if (GetMinimizeBox(w) is bool minimizeBox)
				if (minimizeBox)
					windowStyle |= WS_MINIMIZEBOX;
				else
					windowStyle &= ~WS_MINIMIZEBOX;

			if (GetShowIcon(w) is bool showIcon)
				if (showIcon)
					extendedStyle &= ~WS_EX_DLGMODALFRAME;
				else
					extendedStyle |= WS_EX_DLGMODALFRAME;

			windowStyle &= ~WS_MAXIMIZEBOX;
			hWnd.SetWindowLong(NativeMethods.GWL_STYLE, new IntPtr(windowStyle));
			hWnd.SetWindowLong(NativeMethods.GWL_EXSTYLE, new IntPtr(extendedStyle));
			hWnd.SetWindowPos(IntPtr.Zero, 0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_FRAMECHANGED | NativeMethods.SWP_NOACTIVATE);
		}
	}
}
