using System;
using System.Runtime.Serialization;

namespace Linearstar.Coah.Megalith
{
	[DataContract]
	public class MegalithLocationUri
	{
		[DataMember]
		public Uri AbsoluteUri
		{
			get;
			private set;
		}

		public Uri SubjectsUri
		{
			get;
			set;
		}

		public string SubjectsPath
		{
			get;
			private set;
		}

		public Uri SettingsUri
		{
			get;
			set;
		}

		public string SettingsPath
		{
			get;
			private set;
		}

		public string DataBasePath
		{
			get;
			private set;
		}

		public MegalithLocationUri(Uri uri)
		{
			if (!uri.IsAbsoluteUri)
				throw new ArgumentException($"{nameof(uri)} must be an absolute Uri.");

			ParseUri(AbsoluteUri = uri);
		}

		void ParseUri(Uri uri)
		{
			DataBasePath = uri.Host + "/" + uri.AbsolutePath.Trim('/').Replace('/', '_');
			SubjectsUri = new Uri(uri, "sub/subjects.txt");
			SubjectsPath = $"{DataBasePath}/subjects.txt";
			SettingsUri = new Uri(uri, "settings.ini");
			SettingsPath = $"{DataBasePath}/settings.ini";
		}

		[OnDeserialized]
		void OnDeserialized(StreamingContext ctx)
		{
			ParseUri(AbsoluteUri);
		}
	}
}