using System;
using System.Runtime.Serialization;

namespace Linearstar.Coah.Megalith
{
	[DataContract]
	public class MegalithArticleUri : ParsedUri
	{
		[DataMember]
		public MegalithFeedUri Feed
		{
			get;
			private set;
		}

		public Uri RelativeUri
		{
			get;
			private set;
		}

		public string ArticleId
		{
			get;
			private set;
		}

		public Uri DataUri
		{
			get;
			private set;
		}

		public Uri CommentDataUri
		{
			get;
			set;
		}

		public Uri AfterwordDataUri
		{
			get;
			set;
		}

		public string DataPath
		{
			get;
			private set;
		}

		public string CommentDataPath
		{
			get;
			private set;
		}

		public string AfterwordDataPath
		{
			get;
			private set;
		}

		public MegalithArticleUri(Uri uri)
			: base(uri)
		{
		}

		public MegalithArticleUri(MegalithFeedSummary feedSummary, string key, int log)
			: this(new Uri(feedSummary.AbsoluteUri, $"?mode=read&key={key}&log={log}"))
		{
		}

		protected override void ParseUri(Uri uri)
		{
			Feed = new MegalithFeedUri(new Uri($"{uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.LastIndexOf('/'))}/?log=" + AbsoluteUri.Query("log")));
			RelativeUri = Feed.AbsoluteUri.MakeRelativeUri(uri);
			ArticleId = AbsoluteUri.Query("key");
			DataUri = new Uri(Feed.ArticleBaseUri, $"dat/{ArticleId}.dat");
			CommentDataUri = new Uri(Feed.ArticleBaseUri, $"com/{ArticleId}.res.dat");
			AfterwordDataUri = new Uri(Feed.ArticleBaseUri, $"aft/{ArticleId}.aft.dat");
			DataPath = $"{Feed.DataBasePath}/{ArticleId}.dat";
			CommentDataPath = $"{Feed.DataBasePath}/{ArticleId}.res.dat";
			AfterwordDataPath = $"{Feed.DataBasePath}/{ArticleId}.aft.dat";
			MetaDataPath = $"{Feed.DataBasePath}/{ArticleId}.meta";
		}
	}
}