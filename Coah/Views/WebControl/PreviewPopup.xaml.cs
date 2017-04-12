using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using CefSharp;
using Linearstar.Coah.ViewModels;

namespace Linearstar.Coah.Views
{
	public partial class PreviewPopup : Window
	{
		const int CloseTimerInterval = 100;
		const int RightPadding = 32;

		readonly ArticleWebControl WebControl;
		HwndSource hWndSource;

		public Article Article
		{
			get;
			set;
		}

		public IReadOnlyList<IReadOnlyCollection<ArticleCommentViewModel>> Comments
		{
			get;
			set;
		}

		public ICollection<DisplayRange> DisplayRanges
		{
			get;
			set;
		}

		public ArticleWebControl PopupParent
		{
			get => WebControl.CurrentPopupParent;
			set => WebControl.CurrentPopupParent = value;
		}

		public PreviewPopup()
		{
			InitializeComponent();

			if (!DesignerProperties.GetIsInDesignMode(this))
				WebControlContainer.Child = WebControl = CreateWebControl();
		}

		IEnumerable<PreviewPopup> FindOwner()
		{
			var rt = this;

			while (rt != null)
			{
				yield return rt;

				rt = rt.Owner as PreviewPopup;
			}
		}

		bool GetIsMouseHover()
		{
			var rect = new System.Windows.Rect(0, 0, Width, Height);

			rect.Inflate(16, 32);

			return rect.Contains(Mouse.GetPosition(this));
		}

		ArticleWebControl CreateWebControl()
		{
			var web = new ArticleWebControl
			{
				Skin = ((ClientSettings)App.Current.Client.Settings).CurrentSkin,
			};

			web.FrameLoadEnd += async (sender, e) =>
			{
				if (!e.Frame.IsMain) return;

				await web.EvaluateScriptAsync(
					"document.documentElement.style.overflow = 'auto';" +
					"document.documentElement.style.whiteSpace = 'nowrap';" +
					"document.documentElement.className = 'popup';"
				);

				var rt = (IList<object>)(await web.EvaluateScriptAsync("[document.body.scrollWidth, document.body.scrollHeight]")).Result;
				
				web.ExecuteScriptAsync(
					"document.documentElement.style.overflow = null;" +
					"document.documentElement.style.whiteSpace = null;"
				);

				Dispatcher.Invoke(() =>
				{
					var screen = ScreenHelper.GetScreenFromWindow(this).WorkingArea;
					var elemSize = new Size((int)rt[0] + RightPadding + BorderThickness.Left + BorderThickness.Right, (int)rt[1] + BorderThickness.Top + BorderThickness.Bottom);
					var windowSize = new Size(Math.Min(elemSize.Width, screen.Width), Math.Min(elemSize.Height, screen.Height));

					if (windowSize.Height < elemSize.Height)
						windowSize.Width = Math.Min(windowSize.Width + (int)SystemParameters.HorizontalScrollBarButtonWidth, screen.Width);

					if (windowSize.Width < elemSize.Width)
						windowSize.Height = Math.Min(windowSize.Height + (int)SystemParameters.VerticalScrollBarButtonHeight, screen.Height);

					var windowPos = new Point(Left + windowSize.Width > screen.X + screen.Width ? screen.X + screen.Width - windowSize.Width : Left, Math.Max(screen.Y, Top - windowSize.Height));

					Left = windowPos.X;
					Top = windowPos.Y;
					Width = windowSize.Width;
					Height = windowSize.Height;
					Visibility = Visibility.Visible;

					Observable.Interval(TimeSpan.FromMilliseconds(CloseTimerInterval))
							  .TakeUntil(Observable.FromEvent<EventHandler, EventArgs>(_ => (sender2, e2) => _(e2), _ => Closed += _, _ => Closed -= _))
							  .ObserveOnDispatcher()
							  .Subscribe(t =>
							  {
								  if (App.Current.Windows.OfType<PreviewPopup>().Where(_ => _.FindOwner().Contains(this)).All(_ => !_.GetIsMouseHover()))
									  Close();
							  });
				});
			};

			return web;
		}

		public void InitializeControl()
		{
			WebControl.SetBinding(ArticleWebControl.ArticleProperty, new Binding(nameof(Article))
			{
				Source = this,
			});
			WebControl.SetBinding(ArticleWebControl.CommentsProperty, new Binding(nameof(Comments))
			{
				Source = this,
			});
			WebControl.SetBinding(ArticleWebControl.DisplayRangesProperty, new Binding(nameof(DisplayRanges))
			{
				Source = this,
			});
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			hWndSource = (HwndSource)PresentationSource.FromVisual(this);
			hWndSource.Handle.SetWindowLong(NativeMethods.GWL_EXSTYLE, new IntPtr(hWndSource.Handle.GetWindowLong(NativeMethods.GWL_EXSTYLE).ToInt32() ^ NativeMethods.WS_EX_APPWINDOW | NativeMethods.WS_EX_NOACTIVATE));
			hWndSource.AddHook((IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled) =>
			{
				switch (message)
				{
					case /* WM_MOUSEACTIVATE */ 0x21:
						handled = true;

						return new IntPtr(3);
				}

				return IntPtr.Zero;
			});
		}

		void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Visibility = Visibility.Hidden;
		}

		void Window_Closed(object sender, EventArgs e)
		{
			hWndSource.Dispose();
		}
	}
}
