using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CefSharp;
using Linearstar.Coah.ViewModels;

namespace Linearstar.Coah.Views
{
	public partial class ArticlePageView : UserControl
	{
		readonly ArticleWebControl WebControl;
		string previousSearchText;
		bool? previousSearchCaseSensitive;
		CompositeDisposable disposables;

		ArticlePageViewModel ViewModel => (ArticlePageViewModel)DataContext;

		public ArticlePageView()
		{
			InitializeComponent();

			if (!DesignerProperties.GetIsInDesignMode(this))
				WebControlContainer.Child = WebControl = CreateWebControl();
		}

		ArticleWebControl CreateWebControl()
		{
			var web = new ArticleWebControl
			{
				Skin = ((ClientSettings)App.Current.Client.Settings).CurrentSkin,
			};

			web.SetBinding(ArticleWebControl.ArticleProperty, new Binding($"{nameof(DataContext)}.{nameof(ArticlePageViewModel.Model)}.{nameof(ArticlePage.Article)}") { Source = this });
			web.SetBinding(ArticleWebControl.CommentsProperty, new Binding($"{nameof(DataContext)}.{nameof(ArticlePageViewModel.Comments)}") { Source = this });
			web.SetBinding(ArticleWebControl.DisplayRangesProperty, new Binding($"{nameof(DataContext)}.{nameof(ArticlePageViewModel.Model)}.{nameof(ArticlePage.DisplayRanges)}") { Source = this });
			web.SetBinding(ArticleWebControl.FocusedCommentIndexProperty, new Binding($"{nameof(DataContext)}.{nameof(ArticlePageViewModel.Model)}.{nameof(ArticlePage.FocusedCommentIndex)}") { Source = this });

			return web;
		}

		void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			var mainView = (ViewerView)Window.GetWindow(this);
			var articleStatusBarItem = mainView.ArticleStatusBarItem;

			disposables = new CompositeDisposable
			(
				ViewModel.OnPropertyChanged(nameof(ViewModel.IsActive)).Where(_ => ViewModel.IsActive).Subscribe(_ =>
				{
					mainView.MergeMenu(ArticleMenuItem);
					mainView.MergeStatusBar(this, Resources["ArticleStatusBarItem"]);
				}),
				ViewModel.OnPropertyChanged(nameof(ViewModel.Parent)).Where(_ => ViewModel.Parent == null).Subscribe(_ =>
				{
					disposables.Dispose();
					mainView.UnmergeMenu(ArticleMenuItem);
					mainView.MergeStatusBar(this, null);
				})
			);
		}

		void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			disposables.Dispose();
		}

		public void ScrollToTop() =>
			WebControl.ScrollToTop();

		public void ScrollToBottom() =>
			WebControl.ScrollToBottom();

		public void ToggleSearchBar()
		{
			if (SearchToolBar.IsVisible)
				HideSearchBar();
			else
				ShowSearchBar();
		}

		public void ShowSearchBar()
		{
			SearchToolBar.Visibility = Visibility.Visible;
			SearchTextBox.SelectAll();
			SearchTextBox.Focus();

			if (!string.IsNullOrEmpty(SearchTextBox.Text))
				ClearSearchText();
		}

		public void HideSearchBar()
		{
			WebControl.StopFinding(false);
			SearchToolBar.Visibility = Visibility.Collapsed;
		}

		void SearchText(bool isForward, bool? findNext = null)
		{
			if (string.IsNullOrEmpty(SearchTextBox.Text))
				ClearSearchText();
			else
			{
				WebControl.Find(0, SearchTextBox.Text, isForward, SearchCaseSensitiveCheckBox.IsChecked ?? false,
					findNext ?? previousSearchText == SearchTextBox.Text && previousSearchCaseSensitive == SearchCaseSensitiveCheckBox.IsChecked);
				previousSearchText = SearchTextBox.Text;
				previousSearchCaseSensitive = SearchCaseSensitiveCheckBox.IsChecked;
			}
		}

		void ClearSearchText()
		{
			WebControl.StopFinding(false);
			previousSearchText = null;
			previousSearchCaseSensitive = null;
		}

		public void SearchNext() =>
			SearchText(true);

		public void SearchPrevious() =>
			SearchText(false);

		public void SearchIncremental()
		{
			if (SearchTextBox.Text != previousSearchText)
				SearchText(true, false);
		}
	}
}
