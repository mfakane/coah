using System;
using System.Runtime.Serialization;

namespace Linearstar.Coah.Megalopolis
{
	[DataContract]
	public class MegalopolisLocationUri
	{
		[DataMember]
		public Uri AbsoluteUri
		{
			get;
			private set;
		}

		public Uri ConfigurationUri
		{
			get;
			set;
		}

		public string ConfigurationPath
		{
			get;
			private set;
		}

		public string DataBasePath
		{
			get;
			private set;
		}

		public MegalopolisLocationUri(Uri uri)
		{
			if (!uri.IsAbsoluteUri)
				throw new ArgumentException($"{nameof(uri)} must be an absolute Uri.");

			ParseUri(AbsoluteUri = uri);
		}

		void ParseUri(Uri uri)
		{
			DataBasePath = uri.Host + "/" + uri.AbsolutePath.Trim('/').Replace('/', '_');
			ConfigurationUri = new Uri(uri, "config.json");
			ConfigurationPath = $"{DataBasePath}/config.json";
		}

		[OnDeserialized]
		void OnDeserialized(StreamingContext ctx)
		{
			ParseUri(AbsoluteUri);
		}
	}
}
