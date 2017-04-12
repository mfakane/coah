using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Linearstar.Coah.Megalopolis
{
	[DataContract]
	public class MegalopolisFeedUri : ParsedUri
	{
		[DataMember]
		public MegalopolisLocationUri Location
		{
			get;
			private set;
		}

		public Uri RelativeUri
		{
			get;
			private set;
		}

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

		public string DataBasePath
		{
			get;
			private set;
		}

		public string DataPath
		{
			get;
			private set;
		}

		public MegalopolisFeedUri(Uri uri)
			: base(uri)
		{
		}

		public MegalopolisFeedUri(MegalopolisLocation location, int subject)
			: this(new Uri(location.ParsedUri.AbsoluteUri, subject.ToString()))
		{
		}

		protected override void ParseUri(Uri uri)
		{
			Location = new MegalopolisLocationUri(new Uri(uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.LastIndexOf('/'))));
			RelativeUri = Location.AbsoluteUri.MakeRelativeUri(uri);
			FeedId = uri.Segments.Last();
			DataUri = new Uri(uri.AbsoluteUri + ".json");
			DataBasePath = $"{Location.DataBasePath}/{FeedId}";
			DataPath = $"{DataBasePath}.json";
			MetaDataPath = $"{DataBasePath}.meta";
		}
	}
}