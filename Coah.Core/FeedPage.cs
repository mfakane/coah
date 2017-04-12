using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public class FeedPage : NotifyBase, IPage
	{
		public IViewer Viewer
		{
			get;
			set;
		}

		public FeedSummary FeedSummary { get; }
		public ProgressState ProgressState { get; } = new ProgressState();
		public ObservableCollection<FeedPageArticleFilter> Filters { get; } = new ObservableCollection<FeedPageArticleFilter>();

		public IReadOnlyList<ArticleSummary> Articles
		{
			get => GetValue<IReadOnlyList<ArticleSummary>>();
			private set => SetValue(value);
		}

		public Feed Feed
		{
			get => GetValue<Feed>();
			set => SetValue(value);
		}

		public string SearchString
		{
			get => GetValue<string>();
			set
			{
				SetValue(value);
				ApplyFilter();
			}
		}

		public Func<ArticleSummary, bool> FilterPredicate
		{
			get => GetValue<Func<ArticleSummary, bool>>();
			private set => SetValue(value);
		}

		public FeedPage(FeedSummary feedSummary)
		{
			FeedSummary = feedSummary;
			Filters = new ObservableCollection<FeedPageArticleFilter>();
			Filters.CollectionChanged += (sender, e) => ApplyFilter();
		}

		void ApplyFilter()
		{
			if (Feed == null) return;

			var filteredArticles = Filters.Aggregate(Articles, (x, y) => y.Filter(this, x));
			var q = SearchString;

			FilterPredicate = _ => (filteredArticles?.Contains(_) ?? true) && (q == null || _.Title.IndexOf(q, StringComparison.OrdinalIgnoreCase) > -1);
		}

		public void GetSimilarArticles(string title)
		{
			Filters.Clear();
			Filters.Add(new FeedPageArticleFilter(title + " に類似する項目", (page, articles) => Feed.GetSimilarArticles(title)));
		}

		public async Task Refresh(bool getCached, CancellationToken cancellationToken)
		{
			try
			{
				ProgressState.Report(ProgressInfo.Download(FeedSummary));
				await RefreshCore(getCached, cancellationToken, ProgressState).ConfigureAwait(false);
				ProgressState.Report(ProgressInfo.Done(FeedSummary));
			}
			catch (OperationCanceledException)
			{
				ProgressState.Report(ProgressInfo.Canceled(FeedSummary));
			}
			catch (Exception ex)
			{
				ProgressState.Report(ProgressInfo.Error(FeedSummary, ex));
			}
		}

		async Task RefreshCore(bool getCached, CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			if (Feed == null)
			{
				Feed = await FeedSummary.GetFeed(cancellationToken, progress).ConfigureAwait(false);
				ApplyFilter();
			}

			if (getCached)
				Articles = await Feed.GetCachedArticles(cancellationToken, progress).ConfigureAwait(false);
			else
			{
				await Feed.Refresh(cancellationToken, progress).ConfigureAwait(false);
				Articles = Feed.Articles.ToArray();
			}
		}
	}
}
