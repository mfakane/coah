using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public class ArticlePage : NotifyBase, IPage
	{
		public IViewer Viewer
		{
			get;
			set;
		}

		public ArticleSummary ArticleSummary { get; }
		public ProgressState ProgressState { get; } = new ProgressState();

		public ObservableCollection<IReadOnlyCollection<ArticleComment>> Comments
		{
			get => GetValue<ObservableCollection<IReadOnlyCollection<ArticleComment>>>();
			private set => SetValue(value);
		}

		public ICollection<DisplayRange> DisplayRanges
		{
			get => GetValue<ICollection<DisplayRange>>();
			set => SetValue(value);
		}

		public Article Article
		{
			get => GetValue<Article>();
			set => SetValue(value);
		}

		public bool HasUnread
		{
			get => GetValue<bool>();
			set => SetValue(value);
		}

		public int NewCommentCount
		{
			get;
			set;
		}

		public int? FocusedCommentIndex
		{
			get => GetValue<int?>();
			set => SetValue(value);
		}

		public ArticlePage(ArticleSummary articleSummary)
		{
			ArticleSummary = articleSummary;
			DisplayRanges = new[] { DisplayRange.Latest50 };
		}

		public async Task DeleteCache()
		{
			Comments = null;

			if (Article != null)
				await Article.DeleteCache().ConfigureAwait(false);
			else
				await ArticleSummary.DeleteCache().ConfigureAwait(false);
		}

		public async Task Refresh(CancellationToken cancellationToken)
		{
			try
			{
				ProgressState.Report(ProgressInfo.Download(ArticleSummary));
				await RefreshCore(cancellationToken, ProgressState).ConfigureAwait(false);
				ProgressState.Report(ProgressInfo.Done(ArticleSummary, NewCommentCount));
			}
			catch (OperationCanceledException)
			{
				ProgressState.Report(ProgressInfo.Canceled(ArticleSummary));
			}
			catch (Exception ex)
			{
				ProgressState.Report(ProgressInfo.Error(ArticleSummary, ex));
			}
		}

		async Task RefreshCore(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			var article = Article;

			if (article == null)
				article = await ArticleSummary.GetArticle(cancellationToken, progress).ConfigureAwait(false);

			var cachedCount = ArticleSummary.ReadCommentCount ?? 0;

			await article.Refresh(cancellationToken, progress).ConfigureAwait(false);
			Article = article;

			if (Comments == null || ArticleSummary.CachedCommentCount < cachedCount)
				Comments = new ObservableCollection<IReadOnlyCollection<ArticleComment>>
				{
					article.Comments.ToArray(),
				};
			else if (ArticleSummary.CachedCommentCount > cachedCount)
				Comments.Add(article.Comments.Skip(cachedCount).ToArray());

			NewCommentCount = ArticleSummary.CommentCount - cachedCount;
			FocusedCommentIndex = ArticleSummary.ReadCommentCount;
			ArticleSummary.SetReadAll();
			await ArticleSummary.Save();
		}
	}
}
