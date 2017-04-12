using Linearstar.Coah.ViewModels;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Linearstar.Coah.Views
{
	public partial class FeedPageView : UserControl
	{
		CompositeDisposable disposables;
		FrameworkElement clickedRow;
		DateTime clickedTime;
		bool isDescending;
		object sortKey;

		FeedPageViewModel ViewModel => (FeedPageViewModel)DataContext;

		public FeedPageView()
		{
			InitializeComponent();
		}

		void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			var mainView = (ViewerView)Window.GetWindow(this);

			disposables = new CompositeDisposable
			(
				ViewModel.OnPropertyChanged(nameof(ViewModel.IsActive)).Where(_ => ViewModel.IsActive).Subscribe(_ =>
				{
					mainView.MergeMenu(FeedMenuItem);
					mainView.MergeStatusBar(this, Resources["FeedStatusBarItem"]);
				}),
				ViewModel.OnPropertyChanged(nameof(ViewModel.Parent)).Where(_ => ViewModel.Parent == null).Subscribe(_ =>
				{
					disposables.Dispose();
					mainView.UnmergeMenu(FeedMenuItem);
					mainView.MergeStatusBar(this, null);
				}),
				ViewModel.Model.OnPropertyChanged(nameof(ViewModel.Model.FilterPredicate)).ObserveOnDispatcher().Subscribe(e2 =>
					ArticleList.Items.Filter = _ => ViewModel.Model.FilterPredicate?.Invoke(((ArticleSummaryViewModel)_).Model) ?? true)
			);
		}

		void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			disposables.Dispose();
		}

		void ArticleList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.None)
			{
				clickedRow = VisualTreeHelper.HitTest(ArticleList, e.GetPosition(ArticleList))?.VisualHit?.FindAncestor<ListBoxItem>().FirstOrDefault();
				clickedTime = DateTime.Now;
			}
		}

		void ArticleList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (clickedRow != null)
			{
				var row = VisualTreeHelper.HitTest(ArticleList, e.GetPosition(ArticleList))?.VisualHit?.FindAncestor<ListBoxItem>().FirstOrDefault();

				if (clickedRow == row && (DateTime.Now - clickedTime).TotalMilliseconds < System.Windows.Forms.SystemInformation.DoubleClickTime)
					((ArticleSummaryViewModel)row.DataContext).Show();

				clickedRow = null;
			}
		}

		void ArticleList_GridViewColumnHeaderClick(object sender, RoutedEventArgs e)
		{
			var header = (GridViewColumnHeader)e.OriginalSource;

			foreach (var i in ((GridView)ArticleList.View).Columns.Select(_ => _.Header).OfType<GridViewColumnHeader>())
				i.Tag = null;

			if (header == TitleHeader)
				ViewModel.SortArticles(_ => _.Model.Title, header == sortKey ? (isDescending = !isDescending) : (isDescending = false));
			else if (header == AuthorHeader)
				ViewModel.SortArticles(_ => _.Model.Author, header == sortKey ? (isDescending = !isDescending) : (isDescending = false));
			else if (header == CommentCountHeader)
				ViewModel.SortArticles(_ => _.Model.CommentCount, header == sortKey ? (isDescending = !isDescending) : (isDescending = true));
			else if (header == NewCommentCountHeader)
				ViewModel.SortArticles(_ => _.Model.NewCommentCount, header == sortKey ? (isDescending = !isDescending) : (isDescending = true));
			else if (header == DateTimeHeader)
				ViewModel.SortArticles(_ => _.Model.DateTime, header == sortKey ? (isDescending = !isDescending) : (isDescending = true));

			header.Tag = isDescending ? "Descending" : "Ascending";

			sortKey = header;
		}
	}
}
