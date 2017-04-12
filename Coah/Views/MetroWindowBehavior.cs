using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shell;

namespace Linearstar.Coah.Views
{
	class MetroWindowBehavior : Behavior<Window>
	{
		HwndSource hWndSource;
		WindowState previousState;
		readonly CompositeDisposable disposables = new CompositeDisposable();

		protected Window TargetWindow => AssociatedObject;

		public static Thickness PaddedBorderThickness
		{
			get
			{
				var paddedBorder = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXPADDEDBORDER);

				return new Thickness(SystemParameters.WindowResizeBorderThickness.Left + paddedBorder, SystemParameters.WindowResizeBorderThickness.Top + paddedBorder, SystemParameters.WindowResizeBorderThickness.Right + paddedBorder, SystemParameters.WindowResizeBorderThickness.Bottom + paddedBorder);
			}
		}

		public int TitleBarHeight
		{
			get;
			set;
		} = 32;

		public bool UseColoredBorder
		{
			get;
			set;
		} = true;

		protected override void OnAttached()
		{
			TargetWindow.SourceInitialized += AssociatedObject_SourceInitialized;

			switch (TargetWindow.ResizeMode)
			{
				case ResizeMode.NoResize:
					WindowOptions.SetMaximizeBox(TargetWindow, false);
					WindowOptions.SetMinimizeBox(TargetWindow, false);

					break;
				case ResizeMode.CanMinimize:
					WindowOptions.SetMaximizeBox(TargetWindow, false);

					break;
				case ResizeMode.CanResize:
					TargetWindow.ResizeMode = ResizeMode.CanResizeWithGrip;

					break;
			}

			base.OnAttached();
		}

		void AddBorder()
		{
			Window border = null;
			var closed = Observable.FromEvent<EventHandler, EventArgs>(_ => (sender, e) => _(e), _ => border.Closed += _, _ => border.Closed -= _);

			TargetWindow.BorderThickness = new Thickness(0);

			border = CreateBorderWindow();
			disposables.Add(Observable.FromEvent<EventHandler, EventArgs>(_ => (sender, e) => _(e), _ => TargetWindow.LocationChanged += _, _ => TargetWindow.LocationChanged -= _)
									  .Merge(Observable.FromEvent<EventHandler, EventArgs>(_ => (sender, e) => _(e), _ => TargetWindow.Activated += _, _ => TargetWindow.Activated -= _))
									  .Merge(Observable.FromEvent<SizeChangedEventHandler, EventArgs>(_ => (sender, e) => _(e), _ => TargetWindow.SizeChanged += _, _ => TargetWindow.SizeChanged -= _))
									  .Merge(Observable.FromEvent<EventHandler, EventArgs>(_ => (sender, e) => _(e), _ => TargetWindow.StateChanged += _, _ => TargetWindow.StateChanged -= _))
									  .Merge(Observable.FromEvent<DependencyPropertyChangedEventHandler, DependencyPropertyChangedEventArgs>(_ => (sender, e) => _(e), _ => TargetWindow.IsVisibleChanged += _, _ => TargetWindow.IsVisibleChanged -= _).Select(_ => EventArgs.Empty))
									  .Subscribe(_ => SetBorderWindowLocation(border)));
			SetBorderWindowLocation(border);
			border.Show();

			disposables.Add(Observable.FromEvent<EventHandler, EventArgs>(_ => (sender, e) => _(e), _ => TargetWindow.Closed += _, _ => TargetWindow.Closed -= _)
									  .Subscribe(_ => border.Close()));
		}

		async void SetBorderWindowLocation(Window border)
		{
			const int borderWidth = 9;
			var owner = TargetWindow;

			if (owner.Visibility == Visibility.Visible && owner.WindowState == WindowState.Normal)
			{
				border.Top = owner.Top - borderWidth;
				border.Left = owner.Left - borderWidth;
				border.Width = owner.Width + borderWidth * 2;
				border.Height = owner.Height + borderWidth * 2;

				if (previousState != WindowState.Normal && SystemParameters.MinimizeAnimation)
					await Task.Delay(previousState == WindowState.Minimized ? 250 : 150);

				border.Visibility = Visibility.Visible;

				if (owner.IsVisible)
				{
					border.Show();
					owner.Show();
				}
			}
			else
				border.Visibility = Visibility.Collapsed;

			previousState = owner.WindowState;
		}

		Window CreateBorderWindow()
		{
			const int borderWidth = 9;
			var rt = new Window
			{
				WindowStyle = WindowStyle.None,
				AllowsTransparency = true,
				Background = Brushes.Transparent,
				ResizeMode = ResizeMode.NoResize,
				IsHitTestVisible = TargetWindow.ResizeMode != ResizeMode.NoResize,
				ShowActivated = false,
				SnapsToDevicePixels = true,
				UseLayoutRounding = true,
				Owner = TargetWindow,
				Content = new Border
				{
					Margin = new Thickness(borderWidth - 1),
					BorderThickness = new Thickness(1),
					BorderBrush = (Brush)Application.Current.FindResource("MetroAccentBrush"),
					Effect = new DropShadowEffect
					{
						Color = ((SolidColorBrush)Application.Current.FindResource("MetroAccentBrush")).Color,
						BlurRadius = borderWidth - 1,
						ShadowDepth = 0,
					},
				},
			};

			((Border)rt.Content).SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush")
			{
				Source = TargetWindow,
			});
			BindingOperations.SetBinding((DropShadowEffect)((Border)rt.Content).Effect, DropShadowEffect.ColorProperty, new Binding("BorderBrush.Color")
			{
				Source = TargetWindow,
			});

			HwndSource borderSource = null;

			rt.SizeChanged += (sender, e) =>
			{
				var bounds = new RectangleGeometry(new Rect(0, 0, rt.ActualWidth, rt.ActualHeight));
				var exclude = new RectangleGeometry(new Rect(borderWidth, borderWidth, rt.ActualWidth - borderWidth * 2, rt.ActualHeight - borderWidth * 2));

				rt.Clip = Geometry.Combine(bounds, exclude, GeometryCombineMode.Exclude, null);
			};
			rt.SourceInitialized += (sender, e) =>
			{
				borderSource = (HwndSource)PresentationSource.FromVisual(rt);

				var handle = borderSource.Handle;

				handle.SetWindowLong(NativeMethods.GWL_EXSTYLE, new IntPtr(handle.GetWindowLong(NativeMethods.GWL_EXSTYLE).ToInt32() ^ NativeMethods.WS_EX_APPWINDOW | NativeMethods.WS_EX_NOACTIVATE));
				borderSource.AddHook((IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled) =>
				{
					switch (message)
					{
						case /* WM_MOUSEACTIVATE */ 0x21:
							handled = true;

							return new IntPtr(3);
						case /* WM_LBUTTONDOWN */ 0x201:
							if (TargetWindow.ResizeMode != ResizeMode.NoResize && hWndSource != null)
							{
								var pt = new Point((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF);

								hWndSource.Handle.PostMessage(/* WM_NCLBUTTONDOWN */ 0xA1, new IntPtr(GetHitBorder(rt, pt)), IntPtr.Zero);
							}

							break;
						case  /* WM_NCHITTEST */ 0x84:
							if (TargetWindow.ResizeMode != ResizeMode.NoResize)
							{
								var pt = rt.PointFromScreen(new Point((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF));
								Cursor cursor = null;

								switch (GetHitBorder(rt, pt))
								{
									case 10:
									case 11:
										cursor = Cursors.SizeWE;

										break;
									case 12:
									case 15:
										cursor = Cursors.SizeNS;

										break;
									case 13:
									case 17:
										cursor = Cursors.SizeNWSE;

										break;
									case 14:
									case 16:
										cursor = Cursors.SizeNESW;

										break;
								}

								if (rt.Cursor != cursor)
									rt.Cursor = cursor;
							}

							break;
					}

					return IntPtr.Zero;
				});
			};
			rt.Closed += (sender, e) => borderSource.Dispose();

			return rt;
		}

		int GetHitBorder(Window border, Point pt)
		{
			var size = new Size(border.ActualWidth, border.ActualHeight);
			const int borderWidth = 9;

			if (pt.Y < borderWidth)
				if (pt.X < borderWidth)
					return /* HTTOPLEFT */ 13;
				else if (pt.X > size.Width - borderWidth * 2)
					return /* HTTOPRIGHT */ 14;
				else
					return /* HTTOP */ 12;
			else if (pt.Y > size.Height - borderWidth * 2)
				if (pt.X < borderWidth)
					return /* HTBOTTOMLEFT */ 16;
				else if (pt.X > size.Width - borderWidth * 2)
					return /* HTBOTTOMRIGHT */ 17;
				else
					return /* HTBOTTOM */ 15;
			else
				if (pt.X < borderWidth)
				return /* HTLEFT */ 10;
			else if (pt.X > size.Width - borderWidth * 2)
				return /* HTRIGHT */ 11;
			else
				return /* HTNOWHERE */ 0;
		}

		void UpdateWindowChrome()
		{
			WindowChrome.SetWindowChrome(TargetWindow, new WindowChrome
			{
				CornerRadius = new CornerRadius(),
				GlassFrameThickness = new Thickness(UseColoredBorder ? 0 : -1),
				NonClientFrameEdges = NonClientFrameEdges.None,
				CaptionHeight = TitleBarHeight,
				ResizeBorderThickness = new Thickness(UseColoredBorder ? 0 : 4),
				UseAeroCaptionButtons = false,
			});
		}

		protected override void OnDetaching()
		{
			TargetWindow.SourceInitialized -= AssociatedObject_SourceInitialized;
			disposables.Dispose();
			base.OnDetaching();
		}

		void AssociatedObject_SourceInitialized(object sender, EventArgs e)
		{
			UpdateWindowChrome();

			if (UseColoredBorder)
				AddBorder();

			disposables.Add(hWndSource = (HwndSource)PresentationSource.FromVisual(TargetWindow));
		}
	}
}
