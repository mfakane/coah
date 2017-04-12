using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.JBBS
{
	[DataContract]
	public class JBBSArticleSummary : JsonArticleSummaryBase<JBBSFeedProvider, JBBSArticleUri>
	{
		protected override IDirectory LocalStorage => FeedProvider.LocalStorage;
		protected override Func<ArticleSummary, CancellationToken, IProgress<ProgressInfo>, Task<Article>> LoadArticleFromFile =>
			async (summary, cancellationToken, progress) => await JBBSArticle.LoadFile((JBBSArticleSummary)summary, cancellationToken, progress);

		JBBSArticleSummary(FeedSummary feedSummary, JBBSArticleUri uri)
			: base(feedSummary, uri) =>
			DateTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(long.Parse(uri.ArticleId)).ToLocalTime();

		public override async Task DeleteCache()
		{
			await FeedProvider.LocalStorage.DeleteFile(ParsedUri.DataPath).ConfigureAwait(false);
			await base.DeleteCache();
		}

		public static async Task<JBBSArticleSummary> LoadFile(JBBSFeedSummary feedSummary, JBBSArticleUri uri)
		{
			var rt = await LoadMeta<JBBSArticleSummary>(feedSummary, feedSummary.FeedProvider.LocalStorage, uri).ConfigureAwait(false);

			if (rt != null)
				return rt;
			else
			{
				var lines = await feedSummary.FeedProvider.LocalStorage.ReadAllLines(uri.DataPath, JBBSFeedProvider.Encoding);

				rt = new JBBSArticleSummary(feedSummary, uri);

				if (lines != null)
				{
					rt.CachedCommentCount = lines.Count(_ => !string.IsNullOrEmpty(_));
					rt.CommentCount = Math.Max(rt.CommentCount, rt.CachedCommentCount ?? 0);

					if (rt.CachedCommentCount > 0)
						rt.Title = JBBSArticleComment.Parse(uri.AbsoluteUri, lines.First()).Title;

					await rt.Save();
				}

				return rt;
			}
		}

		public static async Task<JBBSArticleSummary> Parse(JBBSFeed feed, string line, int? index = null)
		{
			var sl = line.Split(new[] { ',' }, 2);
			var sidx = sl[1].LastIndexOf('(');
			var pl = new
			{
				Id = sl[0].Replace(".cgi", null),
				Title = WebUtility.HtmlDecode(sl[1].Substring(0, sidx).Replace("&amp#", "&#")),
				CommentCount = int.Parse(sl[1].Substring(sidx).Trim('(', ')')),
			};
			var instance = (JBBSArticleSummary)await feed.GetArticleSummaryInstance(new JBBSArticleUri(feed.FeedSummary, pl.Id)).ConfigureAwait(false);

			instance.Title = pl.Title;
			instance.CommentCount = Math.Max(instance.CommentCount, pl.CommentCount);
			instance.Index = index;

			return instance;
		}
	}
}