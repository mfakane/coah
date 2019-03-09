using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CefSharp;
using CefSharp.Wpf;
using Linearstar.Coah.ViewModels;

namespace Linearstar.Coah.Views
{
	public partial class ArticleWebControl : ChromiumWebBrowser, IContextMenuHandler
	{
		const string ScriptObjectName = "Coah";

		bool hasLoaded;
		IDisposable commentsCollectionChangedHandler;
		string selectedText;

		static readonly WebScriptIdentifier ScriptObjectIdentifier = new WebScriptIdentifier(ScriptObjectName);

		public static readonly DependencyProperty ArticleProperty = DependencyProperty.Register(nameof(Article), typeof(Article), typeof(ArticleWebControl), new PropertyMetadata((d, e) =>
			((ArticleWebControl)d).SetArticle((Article)e.NewValue)));
		public static readonly DependencyProperty CommentsProperty = DependencyProperty.Register(nameof(Comments), typeof(IReadOnlyList<IReadOnlyCollection<ArticleCommentViewModel>>), typeof(ArticleWebControl), new PropertyMetadata((d, e) =>
		{
			var sender = (ArticleWebControl)d;

			sender.commentsCollectionChangedHandler?.Dispose();
			sender.UpdateComments();

			if (e.NewValue is INotifyCollectionChanged)
				sender.commentsCollectionChangedHandler = ((INotifyCollectionChanged)e.NewValue).OnCollectionChanged().Where(cce => cce.Action == NotifyCollectionChangedAction.Add).Subscribe(cce =>
				{
					var items = cce.NewItems.Cast<IReadOnlyCollection<ArticleCommentViewModel>>().SelectMany(i => i).Select(x => x.Comment).ToArray();

					sender.RenderComments(items);
				});
		}));
		public static readonly DependencyProperty DisplayRangesProperty = DependencyProperty.Register(nameof(DisplayRanges), typeof(ICollection<DisplayRange>), typeof(ArticleWebControl), new PropertyMetadata((d, e) =>
			((ArticleWebControl)d).UpdateComments()));
		public static readonly DependencyProperty FocusedCommentIndexProperty = DependencyProperty.Register(nameof(FocusedCommentIndex), typeof(int?), typeof(ArticleWebControl), new PropertyMetadata((d, e) =>
			((ArticleWebControl)d).UpdateFocusedComment()));

		public int? FocusedCommentIndex
		{
			get => (int?)GetValue(FocusedCommentIndexProperty);
			set => SetValue(FocusedCommentIndexProperty, value);
		}

		public ICollection<DisplayRange> DisplayRanges
		{
			get => (ICollection<DisplayRange>)GetValue(DisplayRangesProperty);
			set => SetValue(DisplayRangesProperty, value);
		}

		public IReadOnlyList<IReadOnlyCollection<ArticleCommentViewModel>> Comments
		{
			get => (IReadOnlyList<IReadOnlyCollection<ArticleCommentViewModel>>)GetValue(CommentsProperty);
			set => SetValue(CommentsProperty, value);
		}

		public Article Article
		{
			get => (Article)GetValue(ArticleProperty);
			set => SetValue(ArticleProperty, value);
		}

		public SkinResource SkinResource
		{
			get;
			private set;
		}

		public Skin Skin
		{
			get;
			set;
		}

		public PreviewPopup CurrentPopup
		{
			get;
			private set;
		}

		public ArticleWebControl CurrentPopupParent
		{
			get;
			set;
		}

		public ArticleWebControl()
		{
			InitializeComponent();

			MenuHandler = this;
			Background = Brushes.Transparent;
			BrowserSettings = new BrowserSettings
			{
				ApplicationCache = CefState.Disabled,
				Databases = CefState.Disabled,
				Javascript = CefState.Enabled,
				LocalStorage = CefState.Disabled,
				Plugins = CefState.Disabled,
				WebGl = CefState.Disabled,
			};
			IsBrowserInitializedChanged += ArticleWebControl_IsBrowserInitializedChanged;
			FrameLoadStart += ArticleWebControl_FrameLoadStart;
			ConsoleMessage += (sender, e) => Debug.WriteLine($"[{e.Source}, {e.Line}] {e.Message}");
			StatusMessage += (sender, e) => Dispatcher.Invoke(() =>
			{
				if (CurrentPopup == null && Address != null && e.Value.StartsWith(new Uri(Address).GetLeftPart(UriPartial.Path)))
				{
					var uri = new Uri(e.Value);

					if (!string.IsNullOrEmpty(uri.Fragment))
						ShowPopupByFragment(uri.Fragment);
				}
			});

            JavascriptObjectRepository.Register(ScriptObjectName, new WebScriptObject(this));
		}

		void ArticleWebControl_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e) =>
			SetArticle(Article);

		void ArticleWebControl_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
		{
			var uri = new Uri(e.Url);

			if (!string.IsNullOrEmpty(uri.Fragment))
			{
				NavigateByFragment(uri.Fragment);

				return;
			}

			if (!e.Frame.IsMain || !e.Url.StartsWith("file:")) return;

			hasLoaded = true;
			Dispatcher.Invoke(() =>
			{
				UpdateComments();
				UpdateFocusedComment();
			});
		}

		void CopyMenuItem_Click(object sender, RoutedEventArgs e) =>
			this.Copy();

		void NavigateByFragment(string fragment)
		{
			if (fragment.StartsWith("#res-"))
				JumpToComment(int.Parse(fragment.Split('-', ',')[1]));
			else if (fragment.StartsWith("#menu-"))
			{
				var sl = fragment.Split('-');
				var mode = sl.Length == 2 ? "menu" : sl[1];
				var idx = int.Parse(sl.Last());

				Dispatcher.Invoke(() =>
				{
					if (idx > 0 && idx <= Comments.Sum(x => x.Count))
					{
						var ctx = (ContextMenu)Resources[mode == "id" ? "ArticleCommentIdentifierContextMenu" : "ArticleCommentContextMenu"];

						ctx.DataContext = Comments.AsParallel().SelectMany(i => i).First(x => x.Comment.Index == idx);
						ctx.IsOpen = true;
					}
				});
			}
			else if (fragment.StartsWith("#ref-"))
			{
				var idx = int.Parse(fragment.Substring(5));

				ShowCommentsPopup(Dispatcher.Invoke(() => Comments)
					.AsParallel()
					.AsOrdered()
					.SelectMany(i => i)
					.Select(x => x.Comment)
					.Where(x => x.GetReferences().Contains(idx))
					.Select(x => new DisplayRange(x.Index))
					.ToArray());
			}
			else if (fragment.StartsWith("#id-"))
			{
				var id = fragment.Substring(4);

				ShowCommentsPopup(Dispatcher.Invoke(() => Comments)
					.AsParallel()
					.AsOrdered()
					.SelectMany(i => i)
					.Select(x => x.Comment)
					.Where(x => x.Sender.Identifier == id)
					.Select(x => new DisplayRange(x.Index))
					.ToArray());
			}
		}

		void ShowPopupByFragment(string fragment)
		{
			if (fragment.StartsWith("#res-"))
				ShowCommentsPopup(fragment.Substring(5).Split(',').Select(x =>
				{
					if (x.Contains("-"))
					{
						var sl = x.Split('-');

						return new DisplayRange(int.Parse(sl[0]), int.Parse(sl[1]));
					}
					else
						return new DisplayRange(int.Parse(x));
				}).ToArray());
		}

		void ShowCommentsPopup(ICollection<DisplayRange> displayRanges)
		{
			Dispatcher.Invoke(async () =>
			{
				if (CurrentPopup != null)
					CurrentPopup.Close();

				if (!Window.GetWindow(GetRootWebControl()).IsActive)
					return;

				var mouse = Mouse.GetPosition(this);
				var elem = await this.EvaluateScriptAsync(WebScriptFormatProvider.Format($"(function(rect) {{ return [rect.left, rect.top]; }})(document.elementFromPoint({mouse.X}, {mouse.Y}).getBoundingClientRect());"));
				var arr = (IList<object>)elem.Result;
				var pt = PointToScreen(new Point(arr[0] is double ? (double)arr[0] : (int)arr[0], arr[1] is double ? (double)arr[1] : (int)arr[1]));
				var view = CurrentPopup = new PreviewPopup
				{
					Owner = Window.GetWindow(this),
					Left = pt.X,
					Top = pt.Y,
					MaxWidth = GetRootWebControl().ActualWidth,
					Article = Article,
					Comments = Comments,
					DisplayRanges = displayRanges,
					PopupParent = this,
				};

				view.Unloaded += (sender2, e2) => CurrentPopup = null;
				view.InitializeControl();
				view.Show();
			});
		}

		public void ScrollToTop() =>
			this.ExecuteScriptAsync("window.scrollTo(0, 0);");

		public void ScrollToBottom() =>
			this.ExecuteScriptAsync("window.scrollTo(0, document.body.scrollHeight);");

		ArticleWebControl GetRootWebControl()
		{
			var parent = CurrentPopupParent;

			if (parent == null)
				return this;

			while (parent.CurrentPopupParent != null)
				parent = parent.CurrentPopupParent;

			return parent;
		}

		void SetArticle(Article article)
		{
			if (!IsBrowserInitialized || article == null) return;

			SkinResource = new SkinResource(article);
			this.LoadHtml(Skin.GetHtml(SkinResource), new Uri(Skin.FullName + "\\").AbsoluteUri);
		}

		void UpdateComments()
		{
			var comments = Comments;
			var displayRanges = DisplayRanges;

			if (!hasLoaded || SkinResource == null || comments == null || displayRanges == null) return;

			var length = comments.Sum(i => i.Count);
			var items = displayRanges.SelectMany(r =>
			{
				var skip = (r.Begin ?? (r.Latest.HasValue ? length - r.Latest : 1)) - 1 ?? 0;
				var take = (r.End ?? length) - skip;

				return comments.SelectMany(i => i).Select(i => i.Comment).Skip(Math.Max(0, skip)).Take(take);
			}).ToArray();

			ClearComments();
			RenderComments(items);
		}

		void UpdateFocusedComment()
		{
			var comments = Comments;
			var displayRanges = DisplayRanges;
			var focusedCommentIndex = FocusedCommentIndex;

			if (!hasLoaded || SkinResource == null || comments == null || displayRanges == null || !focusedCommentIndex.HasValue) return;

			var length = comments.Sum(i => i.Count);

			if (!displayRanges.Any(r => r.Latest.HasValue
				? focusedCommentIndex >= length - r.Latest
				: (r.Begin ?? 1) <= focusedCommentIndex && focusedCommentIndex <= (r.End ?? length)))
				return;

			JumpToComment(focusedCommentIndex.Value);
		}

		void JumpToComment(int idx)
		{
			if (CurrentPopupParent != null)
			{
				var root = GetRootWebControl();

				Dispatcher.Invoke(() => root.CurrentPopup.Close());
				root.JumpToComment(idx);
			}
			else
				Dispatcher.Invoke(() =>
				{
					if (CurrentPopup != null)
						CurrentPopup.Close();

					if (DisplayRanges.Count == 1)
					{
						var range = DisplayRanges.First();

						if (range.Begin.HasValue && idx < range.Begin)
							DisplayRanges = new[] { new DisplayRange(idx, range.End) };
						else if (range.End.HasValue && idx > range.End)
							DisplayRanges = new[] { new DisplayRange(range.Begin, idx) };
						else if (range.Latest.HasValue && idx < Comments.Sum(x => x.Count) - range.Latest)
							DisplayRanges = new[] { new DisplayRange(idx, null) };
					}

					ExecuteScriptAsync($"{ScriptObjectIdentifier}.jumpToComment({idx});");
				});
		}

		void RenderComments(IList<ArticleComment> comments)
		{
			ExecuteScriptAsync($"{ScriptObjectIdentifier}.renderComments({SkinResource.CommentMarkerId}, {comments.AsParallel().AsOrdered().Select(x => SkinResource.CommentHelper(x).ToString())});");
			UpdateSenderIdentifierCounter(comments);
			UpdateReferenceCounter(comments);
		}

		void ClearComments() =>
			ExecuteScriptAsync($"{ScriptObjectIdentifier}.clearComments({SkinResource.CommentMarkerId});");

		void UpdateSenderIdentifierCounter(IEnumerable<ArticleComment> comments)
		{
			var countById = Dispatcher.Invoke(() => Comments).AsParallel().SelectMany(i => i).Select(x => x.Comment).ToLookup(x => x.Sender.Identifier);

			foreach (var i in comments.GroupBy(x => x.Sender.Identifier))
				ExecuteScriptAsync($"{ScriptObjectIdentifier}.updateSenderIdentifierCounter({i.Key}, {countById[i.Key].Select(x => x.Index)});");
		}

		void UpdateReferenceCounter(IEnumerable<ArticleComment> comments)
		{
			var indices = new HashSet<int>(comments.Select(x => x.Index));
			var referenceByIndex = Dispatcher.Invoke(() => Comments).AsParallel().SelectMany(i => i).SelectMany(x => x.Comment.GetReferences().Select(idx => Tuple.Create(x.Comment.Index, idx))).Where(x => indices.Contains(x.Item2)).ToLookup(x => x.Item2, x => x.Item1);

			foreach (var i in indices)
				ExecuteScriptAsync($"{ScriptObjectIdentifier}.updateReferenceCounter({i}, {referenceByIndex[i]});");
		}

		void ExecuteScriptAsync(FormattableString script) =>
			this.ExecuteScriptAsync(WebScriptFormatProvider.Format(script));

		Task<JavascriptResponse> EvaluateScriptAsync(FormattableString script) =>
			this.EvaluateScriptAsync(WebScriptFormatProvider.Format(script));

		#region IContextMenuHandler

		void IContextMenuHandler.OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
		{
			model.Clear();

			if (!string.IsNullOrEmpty(selectedText = parameters.SelectionText))
				Dispatcher.Invoke(() => ((ContextMenu)Resources["WebControlContextMenu"]).IsOpen = true);
		}

		bool IContextMenuHandler.OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags) =>
			false;

		void IContextMenuHandler.OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
		{
		}

		bool IContextMenuHandler.RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback) =>
			false;

		#endregion
	}
}
