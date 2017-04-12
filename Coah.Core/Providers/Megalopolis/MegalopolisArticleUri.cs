using System;
using System.Runtime.Serialization;

namespace Linearstar.Coah.Megalopolis
{
	[DataContract]
	public class MegalopolisArticleUri : ParsedUri
	{
		[DataMember]
		public MegalopolisFeedUri Feed
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

		public Uri ArchivedUri
		{
			get;
			set;
		}

		public string DataPath
		{
			get;
			private set;
		}

		public MegalopolisArticleUri(Uri uri)
			: base(uri)
		{
		}

		public MegalopolisArticleUri(MegalopolisFeedSummary feedSummary, string id)
			: this(new Uri(feedSummary.AbsoluteUri + "/" + id))
		{
		}

		protected override void ParseUri(Uri uri)
		{
			Feed = new MegalopolisFeedUri(new Uri(uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.LastIndexOf('/'))));
			RelativeUri = new Uri(Feed.AbsoluteUri.AbsoluteUri + "/").MakeRelativeUri(uri);
			ArticleId = RelativeUri.ToString().Trim('/');
			DataUri = new Uri(uri.AbsoluteUri + ".json");
			DataPath = $"{Feed.DataBasePath}/{ArticleId}.json";
			MetaDataPath = $"{Feed.DataBasePath}/{ArticleId}.meta";
		}
	}
}