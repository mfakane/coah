using System;
using System.Runtime.Serialization;

namespace Linearstar.Coah.JBBS
{
	[DataContract]
	public class JBBSArticleUri : ParsedUri
	{
		[DataMember]
		public JBBSFeedUri Feed
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

		public JBBSArticleUri(Uri uri)
			: base(uri)
		{
		}

		public JBBSArticleUri(JBBSFeedSummary feedSummary, string id)
			: this(new Uri(feedSummary.ParsedUri.ArticleBaseUri, id + "/"))
		{
		}

		protected override void ParseUri(Uri uri)
		{
			Feed = new JBBSFeedUri(new Uri($"{uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)}/{uri.Segments[uri.Segments.Length - 3]}{uri.Segments[uri.Segments.Length - 2]}"));
			RelativeUri = Feed.ArticleBaseUri.MakeRelativeUri(uri);
			ArticleId = RelativeUri.ToString().TrimEnd('/');
			DataUri = new Uri(Feed.ArticleDataBaseUri, RelativeUri);
			ArchivedUri = new Uri(Feed.ArticleArchivedBaseUri, RelativeUri);
			DataPath = $"{Feed.DataBasePath}/{ArticleId}.dat";
			MetaDataPath = $"{Feed.DataBasePath}/{ArticleId}.meta";
		}
	}
}
