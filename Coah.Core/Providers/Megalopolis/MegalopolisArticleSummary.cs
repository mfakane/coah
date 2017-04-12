using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalopolis
{
	[DataContract]
	public class MegalopolisArticleSummary : JsonArticleSummaryBase<MegalopolisFeedProvider, MegalopolisArticleUri>
	{
		protected override IDirectory LocalStorage => FeedProvider.LocalStorage;
		protected override Func<ArticleSummary, CancellationToken, IProgress<ProgressInfo>, Task<Article>> LoadArticleFromFile =>
			async (summary, cancellationToken, progress) => await MegalopolisArticle.LoadFile((MegalopolisArticleSummary)summary, cancellationToken, progress);

		[DataMember]
		Entry Entry
		{
			get;
			set;
		}

		public override string Title => Entry.Title;
		public override string Author => Entry.Name;
		public string Link => Entry.Link;
		public string Mail => Entry.Mail;
		public override DateTimeOffset DateTime => new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(Entry.DateTime);
		public override DateTimeOffset? LastModified => new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(Entry.LastUpdate);
		public int? PageCount => Entry.PageCount;
		public double? SizeKBytes => Entry.SizeInKB;
		public int? Points => Entry.Points;
		public int? ResponseCount => Entry.ResponseCount;
		public override int CommentCount => Entry.CommentCount ?? 0;
		public int? EvaluationCount => Entry.EvaluationCount;
		public int? ReadCount => Entry.ReadCount;
		public override string[] Tags => Entry.Tags;

		MegalopolisArticleSummary(FeedSummary feedSummary, MegalopolisArticleUri uri)
			: base(feedSummary, uri)
		{
		}

		public override async Task DeleteCache()
		{
			await FeedProvider.LocalStorage.DeleteFile(ParsedUri.DataPath).ConfigureAwait(false);
			await base.DeleteCache();
		}

		public static async Task<MegalopolisArticleSummary> LoadFile(MegalopolisFeedSummary feedSummary, MegalopolisArticleUri uri)
		{
			var rt = await LoadMeta<MegalopolisArticleSummary>(feedSummary, feedSummary.FeedProvider.LocalStorage, uri).ConfigureAwait(false);

			if (rt != null)
				return rt;
			else
			{
				var data = await feedSummary.FeedProvider.LocalStorage.GetFile(uri.DataPath);

				rt = new MegalopolisArticleSummary(feedSummary, uri);

				if (data != null)
					using (var fs = await data.Open(FileAccessMode.Read))
					{
						var thread = (ThreadResponse)new DataContractJsonSerializer(typeof(ThreadResponse)).ReadObject(fs);

						rt.Entry = thread.Entry;
						rt.CachedCommentCount = thread.Comments.Length;

						await rt.Save();
					}

				return rt;
			}
		}

		public static async Task<MegalopolisArticleSummary> Parse(MegalopolisFeed feed, Entry line, int? index = null)
		{
			var instance = (MegalopolisArticleSummary)await feed.GetArticleSummaryInstance(new MegalopolisArticleUri(feed.FeedSummary, line.Id.ToString()));

			instance.Entry = line;
			instance.Index = index;

			return instance;
		}
	}
}
