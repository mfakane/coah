using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.JBBS
{
	public class JBBSFeed : Feed
	{
		readonly WeakInstanceStore<Uri, JBBSArticleSummary> instances = new WeakInstanceStore<Uri, JBBSArticleSummary>();

		public JBBSFeedProvider FeedProvider => (JBBSFeedProvider)Provider;
		public JBBSFeedSummary FeedSummary => (JBBSFeedSummary)Summary;

		JBBSFeed(JBBSFeedSummary summary)
			: base(summary)
		{
		}

		public static async Task<JBBSFeed> LoadFile(JBBSFeedSummary summary, CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
            cancellationToken.ThrowIfCancellationRequested();

            var rt = new JBBSFeed(summary);
			var data = await summary.FeedProvider.LocalStorage.ReadAllText(summary.ParsedUri.DataPath, JBBSFeedProvider.Encoding).ConfigureAwait(false);

			if (data != null)
				await rt.Parse(data, progress);

			return rt;
		}

		public override Task<ArticleSummary> GetArticleSummaryInstance(Uri uri) =>
			GetArticleSummaryInstance(new JBBSArticleUri(uri));

		public async Task<ArticleSummary> GetArticleSummaryInstance(JBBSArticleUri pu) =>
			await instances.GetOrAddAsync(pu.AbsoluteUri, _ => JBBSArticleSummary.LoadFile(FeedSummary, pu)).ConfigureAwait(false);

		public override async Task<ArticleSummary[]> GetCachedArticles(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			var directory = await FeedProvider.LocalStorage.GetDirectory(FeedSummary.ParsedUri.DataBasePath).ConfigureAwait(false);

			if (directory == null) return new ArticleSummary[0];

			var files = await directory.GetFiles("*.dat");

			return await files.Select(x => GetArticleSummaryInstance(new JBBSArticleUri(FeedSummary, Path.GetFileNameWithoutExtension(x.Name)))).WhenAll();
		}

		public override async Task<ArticleSummary[]> Refresh(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			using (var hc = FeedProvider.CreateClient())
			{
				progress?.Report(ProgressInfo.Download(FeedSummary));
				hc.DefaultRequestHeaders.IfModifiedSince = Summary.LastModified;

				var oldArticles = new HashSet<Uri>(Articles?.Select(x => x.AbsoluteUri) ?? Enumerable.Empty<Uri>());

				using (var res = await hc.GetAsync(FeedSummary.ParsedUri.DataUri, cancellationToken).ConfigureAwait(false))
					if (res.IsSuccessStatusCode)
					{
						var data = await res.Content.ReadAsByteArrayAsync();

						await FeedProvider.LocalStorage.WriteAllBytes(FeedSummary.ParsedUri.DataPath, data);
						await Parse(JBBSFeedProvider.GetString(data), progress, x => !oldArticles.Contains(x.AbsoluteUri));
						Summary.ArticleCount = Articles.Count;
						Summary.LastModified = res.Content.Headers.LastModified;
						await Summary.Save();
					}
					else if (res.StatusCode == HttpStatusCode.NotModified)
					{
						var data = await FeedProvider.LocalStorage.GetFile(FeedSummary.ParsedUri.DataPath);

						if (data != null)
							await Parse(await data.ReadAllText(JBBSFeedProvider.Encoding), progress, x => !oldArticles.Contains(x.AbsoluteUri));
					}
					else
						res.EnsureSuccessStatusCode();
			}

			return Articles.Where(x => x.IsNew).ToArray();
		}

		async Task Parse(string content, IProgress<ProgressInfo> progress, Func<ArticleSummary, bool> isNew = null)
		{
			var lines = content.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
			var count = 0;

			Articles = await lines.Select(async (i, idx) =>
								  {
									  var summary = await JBBSArticleSummary.Parse(this, i, idx + 1);

									  summary.IsNew = isNew?.Invoke(summary) ?? false;
                                      progress.Report(ProgressInfo.Download(Summary, count++, lines.Length));

									  return summary;
								  })
								  .WhenAll();
		}
	}
}