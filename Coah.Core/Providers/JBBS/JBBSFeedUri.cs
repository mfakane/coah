using System;
using System.Runtime.Serialization;

namespace Linearstar.Coah.JBBS
{
	[DataContract]
	public class JBBSFeedUri : ParsedUri
	{
		public string FeedId
		{
			get;
			private set;
		}

		public Uri DataUri
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

		public Uri ArticleDataBaseUri
		{
			get;
			private set;
		}

		public Uri ArticleArchivedBaseUri
		{
			get;
			private set;
		}

		public string DataBasePath
		{
			get;
			private set;
		}

		public JBBSFeedUri(Uri uri)
			: base(uri)
		{
		}

		protected override void ParseUri(Uri uri)
		{
			FeedId = uri.AbsolutePath.Trim('/');
			DataBasePath = FeedId;
			DataUri = new Uri(uri, "subject.txt");
			DataPath = $"{DataBasePath}/subject.txt";
			MetaDataPath = $"{DataBasePath}/subject.meta";
			ArticleBaseUri = new Uri(uri, $"/bbs/read.cgi/{FeedId}/");
			ArticleDataBaseUri = new Uri(uri, $"/bbs/rawmode.cgi/{FeedId}/");
			ArticleArchivedBaseUri = new Uri(uri, $"/bbs/read_archive.cgi/{FeedId}/");
		}
	}
}
