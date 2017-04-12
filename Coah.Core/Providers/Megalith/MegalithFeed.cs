using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalith
{
	public class MegalithFeed : Feed
	{
		readonly WeakInstanceStore<Uri, MegalithArticleSummary> instances = new WeakInstanceStore<Uri, MegalithArticleSummary>();

		public MegalithFeedProvider FeedProvider => (MegalithFeedProvider)Provider;
		public MegalithFeedSummary FeedSummary => (MegalithFeedSummary)Summary;

		MegalithFeed(MegalithFeedSummary summary)
			: base(summary)
		{
		}

		public static async Task<MegalithFeed> LoadFile(MegalithFeedSummary summary, CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			var rt = new MegalithFeed(summary);
			var data = await summary.FeedProvider.LocalStorage.ReadAllText(summary.ParsedUri.DataPath, MegalithFeedProvider.Encoding).ConfigureAwait(false);

			if (data != null)
				await rt.Parse(data, progress);

			return rt;
		}

		public override Task<ArticleSummary> GetArticleSummaryInstance(Uri uri) =>
			GetArticleSummaryInstance(new MegalithArticleUri(uri));

		public async Task<ArticleSummary> GetArticleSummaryInstance(MegalithArticleUri pu) =>
			await instances.GetOrAddAsync(pu.AbsoluteUri, _ => MegalithArticleSummary.LoadFile(FeedSummary, pu)).ConfigureAwait(false);

		public override async Task<ArticleSummary[]> GetCachedArticles(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			var directory = await FeedProvider.LocalStorage.GetDirectory(FeedSummary.ParsedUri.DataBasePath).ConfigureAwait(false);

			if (directory == null) return new ArticleSummary[0];

			var files = await directory.GetFiles("*.aft.dat");

			return await files.Select(_ => GetArticleSummaryInstance(new MegalithArticleUri(FeedSummary, _.Name.Substring(0, _.Name.IndexOf('.')), int.Parse(Summary.AbsoluteUri.Query("log"))))).WhenAll();
		}

		public override async Task<ArticleSummary[]> Refresh(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			using (var hc = FeedProvider.CreateClient())
			{
				progress?.Report(ProgressInfo.Download(FeedSummary));
				hc.DefaultRequestHeaders.IfModifiedSince = Summary.LastModified;

				var oldArticles = new HashSet<Uri>(Articles?.Select(_ => _.AbsoluteUri) ?? Enumerable.Empty<Uri>());

				using (var res = await hc.GetAsync(FeedSummary.ParsedUri.FeedId >= FeedSummary.Location.Items.Count
					? FeedSummary.ParsedUri.DataUriIfNewest
					: FeedSummary.ParsedUri.DataUri, cancellationToken).ConfigureAwait(false))
					if (res.IsSuccessStatusCode)
					{
						var data = await res.Content.ReadAsByteArrayAsync();

						await FeedProvider.LocalStorage.WriteAllBytes(FeedSummary.ParsedUri.DataPath, data);
						await Parse(MegalithFeedProvider.GetString(data), progress, _ => !oldArticles.Contains(_.AbsoluteUri));
						Summary.ArticleCount = Articles.Count;
						Summary.LastModified = res.Content.Headers.LastModified;
						await Summary.Save(); ;
					}
					else if (res.StatusCode == HttpStatusCode.NotModified)
					{
						var data = await FeedProvider.LocalStorage.ReadAllText(FeedSummary.ParsedUri.DataPath, MegalithFeedProvider.Encoding);

						if (data != null)
							await Parse(data, progress, _ => !oldArticles.Contains(_.AbsoluteUri));
					}
					else
						res.EnsureSuccessStatusCode();
			}

			return Articles.Where(_ => _.IsNew).ToArray();
		}

		async Task Parse(string content, IProgress<ProgressInfo> progress, Func<ArticleSummary, bool> isNew = null)
		{
			var lines = content.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			var count = 0;

			Articles = await lines.Reverse()
								  .Select(async (i, idx) =>
								  {
									  var summary = await MegalithArticleSummary.Parse(this, i, idx + 1);

									  summary.IsNew = isNew?.Invoke(summary) ?? false;
									  ProgressInfo.Download(Summary, count++, lines.Length);

									  return summary;
								  })
								  .WhenAll();
		}
	}
}