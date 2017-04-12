using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	[DataContract]
	public abstract class JsonArticleSummaryBase<TProvider, TParsedUri> : ArticleSummary
		where TProvider : FeedProvider
		where TParsedUri : ParsedUri
	{
		WeakReference<Article> instance;

		public TProvider FeedProvider => (TProvider)Provider;
		public override Uri AbsoluteUri => ParsedUri.AbsoluteUri;

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

		protected abstract Func<ArticleSummary, CancellationToken, IProgress<ProgressInfo>, Task<Article>> LoadArticleFromFile
		{
			get;
		}

		protected JsonArticleSummaryBase(FeedSummary feedSummary, TParsedUri uri)
			: base(feedSummary, uri.AbsoluteUri)
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

		public override async ValueTask<Article> GetArticle(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			if (instance == null)
				instance = new WeakReference<Article>(null);

			if (instance.TryGetTarget(out var target))
				return target;

			instance.SetTarget(target = await LoadArticleFromFile(this, cancellationToken, progress));

			return target;
		}

		protected static async Task<T> LoadMeta<T>(FeedSummary feedSummary, IDirectory localStorage, TParsedUri uri)
			where T : JsonArticleSummaryBase<TProvider, TParsedUri>
		{
			var meta = await localStorage.GetFile(uri.MetaDataPath);

			if (meta != null)
				using (var fs = await meta.Open(FileAccessMode.Read))
				{
					var rt = (T)new DataContractJsonSerializer(typeof(T)).ReadObject(fs);

					rt.ParsedUri = uri;
					rt.FeedSummary = feedSummary;

					return rt;
				}
			else
				return null;
		}
	}
}
