using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalopolis
{
	public class MegalopolisFeed : Feed
	{
		readonly WeakInstanceStore<Uri, MegalopolisArticleSummary> instances = new WeakInstanceStore<Uri, MegalopolisArticleSummary>();

		public MegalopolisFeedProvider FeedProvider => (MegalopolisFeedProvider)Provider;
		public MegalopolisFeedSummary FeedSummary => (MegalopolisFeedSummary)Summary;

		MegalopolisFeed(MegalopolisFeedSummary summary)
			: base(summary)
		{
		}

		public static async Task<MegalopolisFeed> LoadFile(MegalopolisFeedSummary summary, CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
            cancellationToken.ThrowIfCancellationRequested();

            var rt = new MegalopolisFeed(summary);
			var data = await summary.FeedProvider.LocalStorage.GetFile(summary.ParsedUri.DataPath).ConfigureAwait(false);

			if (data != null)
				using (var fs = await data.Open(FileAccessMode.Read))
					await rt.Parse((SubjectResponse)new DataContractJsonSerializer(typeof(SubjectResponse)).ReadObject(fs), progress);

			return rt;
		}

		public override Task<ArticleSummary> GetArticleSummaryInstance(Uri uri) =>
			GetArticleSummaryInstance(new MegalopolisArticleUri(uri));

		public async Task<ArticleSummary> GetArticleSummaryInstance(MegalopolisArticleUri pu) =>
			await instances.GetOrAddAsync(pu.AbsoluteUri, _ => MegalopolisArticleSummary.LoadFile(FeedSummary, pu)).ConfigureAwait(false);

		public override async Task<ArticleSummary[]> GetCachedArticles(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			var directory = await FeedProvider.LocalStorage.GetDirectory(FeedSummary.ParsedUri.DataBasePath).ConfigureAwait(false);

			if (directory == null) return new ArticleSummary[0];

			var files = await directory.GetFiles("*.json");

			return await files.Select(x => GetArticleSummaryInstance(new MegalopolisArticleUri(FeedSummary, Path.GetFileNameWithoutExtension(x.Name)))).WhenAll();
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

						using (var ms = new MemoryStream(data))
							await Parse((SubjectResponse)new DataContractJsonSerializer(typeof(SubjectResponse)).ReadObject(ms), progress, x => !oldArticles.Contains(x.AbsoluteUri));

						Summary.ArticleCount = Articles.Count;
						Summary.LastModified = res.Content.Headers.LastModified;
						await Summary.Save();
					}
					else if (res.StatusCode == HttpStatusCode.NotModified)
					{
						var data = await FeedProvider.LocalStorage.GetFile(FeedSummary.ParsedUri.DataPath);

						if (data != null)
							using (var fs = await data.Open(FileAccessMode.Read))
								await Parse((SubjectResponse)new DataContractJsonSerializer(typeof(SubjectResponse)).ReadObject(fs), progress, x => !oldArticles.Contains(x.AbsoluteUri));
					}
					else
						res.EnsureSuccessStatusCode();
			}

			return Articles.Where(x => x.IsNew).ToArray();
		}

		async Task Parse(SubjectResponse subj, IProgress<ProgressInfo> progress, Func<ArticleSummary, bool> isNew = null)
		{
			var count = 0;

			Articles = await subj.Entries.Select(async (i, idx) =>
										 {
											 var summary = await MegalopolisArticleSummary.Parse(this, i, idx + 1);

											 summary.IsNew = isNew?.Invoke(summary) ?? false;
                                             progress.Report(ProgressInfo.Download(Summary, count++, subj.Entries.Length));

											 return summary;
										 })
										 .WhenAll();
		}
	}
}