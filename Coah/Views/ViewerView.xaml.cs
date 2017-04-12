using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dragablz;
using Linearstar.Coah.Models;
using Linearstar.Coah.ViewModels;

namespace Linearstar.Coah.Views
{
	public partial class ViewerView : Window
	{
		MenuItem previousFeedMenuItem;
		MenuItem previousArticleMenuItem;
		MenuItem previousArticleToolsMenuItem;

		public IInterTabClient TabClient { get; } = new InterTabClient();

		public ViewerView() =>
			InitializeComponent();

		public void MergeMenu(MenuItem item)
		{
			if (item.DataContext is FeedPageViewModel)
				if (previousFeedMenuItem != item)
					UnmergeMenu(previousFeedMenuItem);
				else
					return;
			else if (item.DataContext is ArticlePageViewModel)
				if (previousArticleMenuItem != item)
					UnmergeMenu(previousArticleMenuItem);
				else
					return;

			var target = item.DataContext is FeedPageViewModel ? FeedMenuItem : ArticleMenuItem;
			var items = item.Items.Cast<FrameworkElement>().ToArray();

			item.Items.Clear();
			NameScope.SetNameScope(target, NameScope.GetNameScope(item));
			target.ItemsSource = items;
			Caliburn.Micro.Action.SetTarget(target, item.DataContext);

			if (item.DataContext is FeedPageViewModel)
				previousFeedMenuItem = item;
			else
				previousArticleMenuItem = item;
		}

		public void UnmergeMenu(MenuItem item)
		{
			if (item == null ||
				item.Items.Count > 0)
				return;

			var target = item.DataContext is FeedPageViewModel ? FeedMenuItem : ArticleMenuItem;

			foreach (var i in target.ItemsSource)
				item.Items.Add(i);

			NameScope.SetNameScope(target, null);
			target.ItemsSource = null;
			Caliburn.Micro.Action.SetTarget(target, null);

			if (item.DataContext is FeedPageViewModel)
				previousFeedMenuItem = null;
			else
				previousArticleMenuItem = null;
		}

		public void MergeToolsMenu(MenuItem item)
		{
			if (previousArticleToolsMenuItem != null)
				UnmergeToolsMenu(previousArticleToolsMenuItem);

			var target = (CollectionViewSource)ToolsMenuItem.Resources["MergeItems"];
			var items = item.Items.Cast<FrameworkElement>().ToArray();

			foreach (var i in items)
			{
				NameScope.SetNameScope(i, NameScope.GetNameScope(item));

				if (Caliburn.Micro.Action.GetTargetWithoutContext(i) == null)
					Caliburn.Micro.Action.SetTarget(i, item.DataContext);
			}

			item.Items.Clear();
			target.Source = items;
			previousArticleToolsMenuItem = item;
		}

		public void UnmergeToolsMenu(MenuItem item)
		{
			var target = (CollectionViewSource)ToolsMenuItem.Resources["MergeItems"];

			if (target.Source != null)
				foreach (var i in (IReadOnlyList<FrameworkElement>)target.Source)
					item.Items.Add(i);

			target.Source = null;
			previousArticleToolsMenuItem = null;
		}

		public void MergeStatusBar(FrameworkElement scope, object content)
		{
			if (content is DependencyObject)
				NameScope.SetNameScope((DependencyObject)content, NameScope.GetNameScope(scope));

			if (scope is FeedPageView)
			{
				FeedStatusBarItem.DataContext = scope?.DataContext;
				FeedStatusBarItem.Content = content;
			}
			else
			{
				ArticleStatusBarItem.DataContext = scope?.DataContext;
				ArticleStatusBarItem.Content = content;
			}
		}

		class InterTabClient : IInterTabClient
		{
			public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
			{
				var currentViewModel = (ViewerViewModel)source.DataContext;
				var newModel = new Viewer(currentViewModel.Model.Client);

				currentViewModel.Client.Model.Viewers.Add(newModel);

				var view = Application.Current.Windows.OfType<ViewerView>().First(_ => ((ViewerViewModel)_.DataContext).Model == newModel);

				return new NewTabHost<Window>(view, view.TabablzControl);
			}

			public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window) =>
				TabEmptiedResponse.CloseWindowOrLayoutBranch;
		}
	}
}
