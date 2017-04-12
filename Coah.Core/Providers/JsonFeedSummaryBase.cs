using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	[DataContract]
	public abstract class JsonFeedSummaryBase<TProvider, TParsedUri> : FeedSummary
		where TProvider : FeedProvider
		where TParsedUri : ParsedUri
	{
		WeakReference<Feed> instance;

		public TProvider FeedProvider => (TProvider)Provider;

		[DataMember]
		public TParsedUri ParsedUri
		{
			get;
			private set;
		}

		protected abstract IDirectory LocalStorage
		{
			get;
		}

		protected abstract Func<FeedSummary, CancellationToken, IProgress<ProgressInfo>, Task<Feed>> LoadFeedFromFile
		{
			get;
		}

		protected JsonFeedSummaryBase(Location location, TParsedUri uri)
			: base(location, uri.AbsoluteUri)
		{
			ParsedUri = uri;
		}

		public override async Task Save()
		{
			var meta = await LocalStorage.CreateFile(ParsedUri.MetaDataPath, FileCollisionOption.Replace);

			using (var fs = await meta.Open(FileAccessMode.Write))
				new DataContractJsonSerializer(GetType()).WriteObject(fs, this);
		}

		public override async Task DeleteCache()
		{
			await LocalStorage.DeleteFile(ParsedUri.MetaDataPath);
			await base.DeleteCache();
		}

		public override async ValueTask<Feed> GetFeed(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			if (instance == null)
				instance = new WeakReference<Feed>(null);

			if (instance.TryGetTarget(out var target))
				return target;

			instance.SetTarget(target = await LoadFeedFromFile(this, cancellationToken, progress));

			return target;
		}

		protected static async Task<T> LoadMeta<T>(Location location, IDirectory localStorage, TParsedUri uri, string name)
			where T : JsonFeedSummaryBase<TProvider, TParsedUri>
		{
			var meta = await localStorage.GetFile(uri.MetaDataPath);

			if (meta != null)
				using (var fs = await meta.Open(FileAccessMode.Read))
				{
					var rt = (T)new DataContractJsonSerializer(typeof(T)).ReadObject(fs);

					rt.Name = name;
					rt.ParsedUri = uri;
					rt.Location = location;

					return rt;
				}
			else
				return null;
		}
	}
}
