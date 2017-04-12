using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalopolis
{
	[DataContract]
	public class MegalopolisFeedSummary : JsonFeedSummaryBase<MegalopolisFeedProvider, MegalopolisFeedUri>
	{
		protected override IDirectory LocalStorage => FeedProvider.LocalStorage;
		protected override Func<FeedSummary, CancellationToken, IProgress<ProgressInfo>, Task<Feed>> LoadFeedFromFile =>
			async (summary, cancellationToken, progress) => await MegalopolisFeed.LoadFile((MegalopolisFeedSummary)summary, cancellationToken, progress).ConfigureAwait(false);

		MegalopolisFeedSummary(Location location, MegalopolisFeedUri uri)
			: base(location, uri)
		{
		}

		public override async Task<int> GetCachedArticleCount()
		{
			var directory = await FeedProvider.LocalStorage.GetDirectory(ParsedUri.DataPath);

			if (directory == null) return 0;

			var files = await directory.GetFiles("*.json");

			return files.Count;
		}

		public override async Task DeleteCache()
		{
			await FeedProvider.LocalStorage.DeleteFile(ParsedUri.DataPath).ConfigureAwait(false);
			await base.DeleteCache();
		}

		public static async Task<MegalopolisFeedSummary> LoadFile(MegalopolisLocation location, MegalopolisFeedUri uri, string name)
		{
			var rt = await LoadMeta<MegalopolisFeedSummary>(location, location.FeedProvider.LocalStorage, uri, name).ConfigureAwait(false);

			if (rt != null)
				return rt;
			else
			{
				var data = await location.FeedProvider.LocalStorage.GetFile(uri.DataPath);

				rt = new MegalopolisFeedSummary(location, uri)
				{
					Name = name,
				};

				if (data != null)
				{
					var feed = await rt.GetFeed(CancellationToken.None, null);

					rt.ArticleCount = feed.Articles?.Count ?? 0;
					await rt.Save();
				}

				return rt;
			}
		}
	}
}