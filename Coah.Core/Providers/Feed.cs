using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public abstract class Feed
	{
		public FeedProvider Provider => Summary.Provider;

		public virtual FeedSummary Summary
		{
			get;
		}

		public virtual bool IsLoaded => Articles != null;

		public virtual IList<ArticleSummary> Articles
		{
			get;
			set;
		}

		public Feed(FeedSummary summary) =>
			Summary = summary;

		public abstract Task<ArticleSummary> GetArticleSummaryInstance(Uri uri);

		/// <summary>
		/// 現在のフィードを最新の情報に更新し、新着記事を取得します。
		/// </summary>
		/// <param name="cancellationToken">処理のキャンセルに使用するトークン。</param>
		/// <param name="progress">処理の進捗報告に使用する <see cref="IProgress{ProgressInfo}"/>、または null。</param>
		/// <returns>前回取得時から新しく追加された、新着記事のコレクション。</returns>
		public abstract Task<ArticleSummary[]> Refresh(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);

		/// <summary>
		/// 現在のフィードに含まれる、取得済みの既得記事を取得します。。
		/// </summary>
		/// <param name="cancellationToken">処理のキャンセルに使用するトークン。</param>
		/// <param name="progress">処理の進捗報告に使用する <see cref="IProgress{ProgressInfo}"/>、または null。</param>
		/// <returns>既得記事のコレクション。</returns>
		public abstract Task<ArticleSummary[]> GetCachedArticles(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);

		public IReadOnlyList<ArticleSummary> GetSimilarArticles(string title)
		{
			var words = new HashSet<string>(StringUtil.GetWords(title));

			return Articles?.Select(_ => new
			{
				Article = _,
				Similarity = StringUtil.IntersectWords(_.Title, words),
			})
			.Where(_ => _.Similarity.Count > words.Count / 2)
			.OrderByDescending(_ => _.Similarity.Count)
			.Select(_ => _.Article)
			.ToArray();
		}

		public virtual async Task DeleteCache()
		{
			await Summary.DeleteCache();
			Articles = null;
		}
	}
}
