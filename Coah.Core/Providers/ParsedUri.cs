using System;
using System.Runtime.Serialization;

namespace Linearstar.Coah
{
	[DataContract]
	public abstract class ParsedUri : IEquatable<ParsedUri>
	{
		[DataMember]
		public Uri AbsoluteUri
		{
			get;
			private set;
		}

		public string MetaDataPath
		{
			get;
			protected set;
		}

		public ParsedUri(Uri uri)
		{
			if (!uri.IsAbsoluteUri)
				throw new ArgumentException($"{nameof(uri)} must be an absolute Uri.");

			ParseUri(AbsoluteUri = uri);
		}

		protected abstract void ParseUri(Uri uri);

		[OnDeserialized]
		void OnDeserialized(StreamingContext ctx) =>
			ParseUri(AbsoluteUri);

		public override int GetHashCode() =>
			GetType().GetHashCode() ^ AbsoluteUri.GetHashCode();

		public bool Equals(ParsedUri other) =>
			other?.GetType() == GetType() && other?.AbsoluteUri == AbsoluteUri;
	}
}
