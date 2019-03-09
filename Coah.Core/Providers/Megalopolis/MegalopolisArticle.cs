using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalopolis
{
	public class MegalopolisArticle : Article
	{
		public MegalopolisFeedProvider FeedProvider => (MegalopolisFeedProvider)Provider;
		public MegalopolisArticleSummary ArticleSummary => (MegalopolisArticleSummary)Summary;

		ThreadResponse Thread
		{
			get;
			set;
		}

		public override bool IsLoaded => base.IsLoaded && Thread != null;
		public string[] Body => Thread.FormattedBody;
		public string Afterword => Thread.Afterword;
		public string Border => Thread.Border;
		public string Foreground => Thread.Foreground;
		public string Background => Thread.Background;
		public string BackgroundImage => Thread.BackgroundImage;
		public bool? IsVertical => Thread.WritingMode == WritingMode.Default ? default(bool?) : Thread.WritingMode == WritingMode.Horizontal;

		MegalopolisArticle(MegalopolisArticleSummary summary)
			: base(summary)
		{
		}

		public static async Task<MegalopolisArticle> LoadFile(MegalopolisArticleSummary summary, CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
            cancellationToken.ThrowIfCancellationRequested();

            var rt = new MegalopolisArticle(summary);
			var data = await summary.FeedProvider.LocalStorage.GetFile(summary.ParsedUri.DataPath);

			if (data != null)
				using (var fs = await data.Open(FileAccessMode.Read))
					rt.Parse((ThreadResponse)new DataContractJsonSerializer(typeof(ThreadResponse)).ReadObject(fs), progress);

			return rt;
		}

		public override async Task Refresh(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			using (var hc = FeedProvider.CreateClient())
			{
				progress?.Report(ProgressInfo.Download(ArticleSummary));

				using (var res = await hc.GetAsync(ArticleSummary.ParsedUri.DataUri, cancellationToken).ConfigureAwait(false))
					if (res.IsSuccessStatusCode)
					{
						var data = await res.Content.ReadAsByteArrayAsync();

						await FeedProvider.LocalStorage.WriteAllBytes(ArticleSummary.ParsedUri.DataPath, data);

						using (var ms = new MemoryStream(data))
							Parse((ThreadResponse)new DataContractJsonSerializer(typeof(ThreadResponse)).ReadObject(ms), progress);

						ArticleSummary.CachedCommentCount = ArticleSummary.CommentCount = Comments.Count;
						ArticleSummary.LastModified = res.Content.Headers.LastModified;
						await Summary.Save();
					}
					else if (res.StatusCode == HttpStatusCode.NotFound)
					{
						ArticleSummary.IsLost = true;
						await Summary.Save();
						res.EnsureSuccessStatusCode();
					}
					else
						res.EnsureSuccessStatusCode();
			}
		}

		void Parse(ThreadResponse thread, IProgress<ProgressInfo> progress)
		{
			var count = 0;
			var items = thread.Comments.AsParallel()
									   .AsOrdered()
									   .Select((x, idx) => MegalopolisArticleComment.Parse(ArticleSummary.ParsedUri.AbsoluteUri, idx + 1, x))
									   .Do(_ => progress?.Report(ProgressInfo.Download(Summary, count++, thread.Comments.Length)))
									   .Cast<ArticleComment>();

			Comments = items.ToList();
			Thread = thread;
		}

		public override Task DeleteCache()
		{
			Thread = null;

			return base.DeleteCache();
		}
	}
}