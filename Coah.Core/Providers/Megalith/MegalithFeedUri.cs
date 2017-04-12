using System;
using System.Runtime.Serialization;

namespace Linearstar.Coah.Megalith
{
	[DataContract]
	public class MegalithFeedUri : ParsedUri
	{
		[DataMember]
		public MegalithLocationUri Location
		{
			get;
			private set;
		}

		public int FeedId
		{
			get;
			private set;
		}

		public Uri DataUri
		{
			get;
			private set;
		}

		public Uri DataUriIfNewest
		{
			get;
			private set;
		}

		public string DataPath
		{
			get;
			private set;
		}

		public Uri ArticleBaseUri
		{
			get;
			private set;
		}

		public string DataBasePath
		{
			get;
			private set;
		}

		public MegalithFeedUri(Uri uri)
			: base(uri)
		{
		}

		public MegalithFeedUri(MegalithLocation location, int log)
			: this(new Uri(location.ParsedUri.AbsoluteUri, "?log=" + log))
		{
		}

		protected override void ParseUri(Uri uri)
		{
			Location = new MegalithLocationUri(new Uri(uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.LastIndexOf('/'))));
			FeedId = int.Parse(uri.Query("log"));
			DataBasePath = Location.DataBasePath + "/" + FeedId;
			DataUri = new Uri(uri, $"sub/subject{FeedId}.txt");
			DataUriIfNewest = new Uri(uri, $"sub/subject.txt");
			DataPath = $"{DataBasePath}/subject.txt";
			MetaDataPath = $"{DataBasePath}/subject.meta";
			ArticleBaseUri = new Uri(uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.LastIndexOf('/')) + "/");
		}
	}
}