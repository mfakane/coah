using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalith
{
	[DataContract]
	public class MegalithArticleSummary : JsonArticleSummaryBase<MegalithFeedProvider, MegalithArticleUri>
	{
		protected override IDirectory LocalStorage => FeedProvider.LocalStorage;
		protected override Func<ArticleSummary, CancellationToken, IProgress<ProgressInfo>, Task<Article>> LoadArticleFromFile =>
			async (summary, cancellationToken, progress) => await MegalithArticle.LoadFile((MegalithArticleSummary)summary, cancellationToken, progress);

		[DataMember]
		public string Mail
		{
			get;
			private set;
		}

		[DataMember]
		public string Link
		{
			get;
			private set;
		}

		[DataMember]
		public int? EvaluationCount
		{
			get;
			private set;
		}

		[DataMember]
		public int? Points
		{
			get;
			private set;
		}

		[DataMember]
		public double? Rate
		{
			get;
			private set;
		}

		[DataMember]
		public string Background
		{
			get;
			private set;
		}

		[DataMember]
		public string Foreground
		{
			get;
			private set;
		}

		[DataMember]
		public double? SizeKBytes
		{
			get;
			private set;
		}

		[DataMember]
		public DateTimeOffset? CommentLastModified
		{
			get;
			set;
		}

		[DataMember]
		public DateTimeOffset? AfterwordLastModified
		{
			get;
			set;
		}

		MegalithArticleSummary(FeedSummary feedSummary, MegalithArticleUri uri)
			: base(feedSummary, uri)
		{
			DateTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(long.Parse(uri.ArticleId)).ToLocalTime();
		}

		public override async Task DeleteCache()
		{
			await FeedProvider.LocalStorage.DeleteFile(ParsedUri.DataPath).ConfigureAwait(false);
			await FeedProvider.LocalStorage.DeleteFile(ParsedUri.CommentDataPath);
			await FeedProvider.LocalStorage.DeleteFile(ParsedUri.AfterwordDataPath);
			Mail = null;
			Link = null;
			EvaluationCount = null;
			Points = null;
			Rate = null;
			Background = null;
			Foreground = null;
			SizeKBytes = null;
			CommentLastModified = null;
			AfterwordLastModified = null;
			await base.DeleteCache();
		}

		public static async Task<MegalithArticleSummary> LoadFile(MegalithFeedSummary feedSummary, MegalithArticleUri uri)
		{
			var rt = await LoadMeta<MegalithArticleSummary>(feedSummary, feedSummary.FeedProvider.LocalStorage, uri).ConfigureAwait(false);

			if (rt != null)
				return rt;
			else
			{
				var data = await feedSummary.FeedProvider.LocalStorage.ReadAllBytes(uri.DataPath);

				rt = new MegalithArticleSummary(feedSummary, uri);

				if (data != null)
				{
					var com = await feedSummary.FeedProvider.LocalStorage.GetFile(uri.CommentDataPath);
					var content = MegalithFeedProvider.GetString(data);
					var line = content.Substring(0, content.IndexOf('\n')).TrimEnd('\r');

					rt.Parse(line.Split(new[] { "<>" }, StringSplitOptions.None));
					rt.CachedCommentCount = (await com.ReadLines(MegalithFeedProvider.Encoding)).Count();
					rt.SizeKBytes = data.Length / 1024.0;

					await rt.Save();
				}

				return rt;
			}
		}

		public void Parse(string[] fields)
		{
			var q = new Queue<string>(fields.Select(WebUtility.HtmlDecode));

			Title = q.Dequeue();
			Author = q.Dequeue();
			Mail = q.Dequeue();
			Link = q.Dequeue();

			var ce = q.Dequeue().Split(new[] { '/' }, 2);

			CommentCount = int.Parse(ce.First());
			EvaluationCount = int.Parse(ce.Last());

			Points = int.Parse(q.Dequeue());
			Rate = double.Parse(q.Dequeue());
			LastModified = DateTimeOffset.Parse(q.Dequeue());
			q.Dequeue();
			Background = q.Dequeue();
			Foreground = q.Dequeue();

			if (q.Any())
				q.Dequeue();

			if (q.Any())
				Tags = q.Dequeue().Split(null);

			if (q.Any())
				SizeKBytes = double.Parse(q.Dequeue());
		}

		public static async Task<MegalithArticleSummary> Parse(MegalithFeed feed, string line, int? index = null)
		{
			var sidx = line.IndexOf("<>");
			var instance = (MegalithArticleSummary)await feed.GetArticleSummaryInstance(new MegalithArticleUri(feed.FeedSummary, line.Substring(0, sidx - 4), feed.FeedSummary.ParsedUri.FeedId)).ConfigureAwait(false);

			instance.Parse(line.Substring(sidx + 2).Split(new[] { "<>" }, StringSplitOptions.None));
			instance.Index = index;

			return instance;
		}
	}
}