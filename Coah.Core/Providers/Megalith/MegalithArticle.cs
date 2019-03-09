using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalith
{
	public class MegalithArticle : Article
	{
		public MegalithFeedProvider FeedProvider => (MegalithFeedProvider)Provider;
		public MegalithArticleSummary ArticleSummary => (MegalithArticleSummary)Summary;

		public override bool IsLoaded => base.IsLoaded && Body != null;

		public string Body
		{
			get;
			private set;
		}

		public string Afterword
		{
			get;
			private set;
		}

		MegalithArticle(MegalithArticleSummary summary)
			: base(summary)
		{
		}

		public static async Task<MegalithArticle> LoadFile(MegalithArticleSummary summary, CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
            cancellationToken.ThrowIfCancellationRequested();

            var rt = new MegalithArticle(summary);
			var dat = await summary.FeedProvider.LocalStorage.ReadAllText(summary.ParsedUri.DataPath, MegalithFeedProvider.Encoding).ConfigureAwait(false);
			var com = await summary.FeedProvider.LocalStorage.ReadAllLines(summary.ParsedUri.CommentDataPath, MegalithFeedProvider.Encoding);
			var aft = await summary.FeedProvider.LocalStorage.ReadAllText(summary.ParsedUri.AfterwordDataPath, MegalithFeedProvider.Encoding);

			rt.Parse
			(
				dat,
				com,
				aft,
				progress
			);

			return rt;
		}

		public override async Task Refresh(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			using (var hc = FeedProvider.CreateClient())
			{
				string dat = null;
				string[] com = null;
				string aft = null;

				progress?.Report(ProgressInfo.Download(ArticleSummary));

				using (var resDat = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Get, ArticleSummary.ParsedUri.DataUri)
				{
					Headers =
					{
						IfModifiedSince = Summary.LastModified,
					},
				}, cancellationToken).ConfigureAwait(false))
					if (resDat.StatusCode == HttpStatusCode.NotModified)
						dat = await FeedProvider.LocalStorage.GetFile(ArticleSummary.ParsedUri.DataPath)
							.ContinueWith(x => x.Result.ReadAllText(MegalithFeedProvider.Encoding)).Unwrap();
					else if (resDat.StatusCode == HttpStatusCode.OK)
					{
						var data = await resDat.Content.ReadAsByteArrayAsync();

						await FeedProvider.LocalStorage.WriteAllBytes(ArticleSummary.ParsedUri.DataPath, data);
						dat = MegalithFeedProvider.GetString(data);
					}
					else if (resDat.StatusCode == HttpStatusCode.NotFound)
					{
						ArticleSummary.IsLost = true;
						await Summary.Save();
						resDat.EnsureSuccessStatusCode();
					}
					else
						resDat.EnsureSuccessStatusCode();

				using (var resCom = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Get, ArticleSummary.ParsedUri.CommentDataUri)
				{
					Headers =
					{
						IfModifiedSince = ArticleSummary.CommentLastModified,
					},
				}, cancellationToken))
					if (resCom.StatusCode == HttpStatusCode.NotModified)
						com = await FeedProvider.LocalStorage.ReadAllLines(ArticleSummary.ParsedUri.CommentDataPath, MegalithFeedProvider.Encoding);
					else if (resCom.StatusCode == HttpStatusCode.OK)
					{
						var data = await resCom.Content.ReadAsByteArrayAsync();

						await FeedProvider.LocalStorage.WriteAllBytes(ArticleSummary.ParsedUri.CommentDataPath, data);
						com = MegalithFeedProvider.GetString(data).TrimEnd('\r', '\n').Split('\n').Select(x => x.TrimEnd('\r')).ToArray();
						ArticleSummary.CommentLastModified = resCom.Content.Headers.LastModified;
					}

				using (var resAft = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Get, ArticleSummary.ParsedUri.AfterwordDataUri)
				{
					Headers =
					{
						IfModifiedSince = ArticleSummary.AfterwordLastModified,
					},
				}, cancellationToken))
					if (resAft.StatusCode == HttpStatusCode.NotModified)
						aft = await FeedProvider.LocalStorage.ReadAllText(ArticleSummary.ParsedUri.AfterwordDataPath, MegalithFeedProvider.Encoding);
					else if (resAft.StatusCode == HttpStatusCode.OK)
					{
						var data = await resAft.Content.ReadAsByteArrayAsync();

						await FeedProvider.LocalStorage.WriteAllBytes(ArticleSummary.ParsedUri.AfterwordDataPath, data);
						aft = MegalithFeedProvider.GetString(data);
						ArticleSummary.AfterwordLastModified = resAft.Content.Headers.LastModified;
					}

				Parse(dat, com, aft, progress);
				ArticleSummary.CachedCommentCount = ArticleSummary.CommentCount;
				await Summary.Save();
			}
		}

		void Parse(string dat, string[] com, string aft, IProgress<ProgressInfo> progress)
		{
			if (dat != null)
			{
				var idx = dat.IndexOf('\n');

				ArticleSummary.Parse(dat.Substring(0, idx).TrimEnd('\r').Split(new[] { "<>" }, StringSplitOptions.None));
				Body = dat.Substring(dat.IndexOf('\n', idx + 1) + 1);
			}

			if (com != null)
			{
				var count = 0;
				var items = com.AsParallel()
							   .AsOrdered()
							   .Where(x => !x.StartsWith("#EMPTY#<>"))
							   .Select((x, idx) => MegalithArticleComment.Parse(ArticleSummary.ParsedUri.AbsoluteUri, idx + 1, x))
							   .Do(_ => progress?.Report(ProgressInfo.Download(Summary, count++, com.Length)))
							   .Cast<ArticleComment>();

				Comments = items.ToList();
			}

			if (aft != null)
				Afterword = aft;
		}

		public override Task DeleteCache()
		{
			Body = null;
			Afterword = null;

			return base.DeleteCache();
		}
	}
}