using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalith
{
	[DataContract]
	public class MegalithFeedSummary : JsonFeedSummaryBase<MegalithFeedProvider, MegalithFeedUri>
	{
		protected override IDirectory LocalStorage => FeedProvider.LocalStorage;
		protected override Func<FeedSummary, CancellationToken, IProgress<ProgressInfo>, Task<Feed>> LoadFeedFromFile =>
			async (summary, cancellationToken, progress) => await MegalithFeed.LoadFile((MegalithFeedSummary)summary, cancellationToken, progress);

		MegalithFeedSummary(Location location, MegalithFeedUri uri)
			: base(location, uri)
		{
		}

		public override async Task<int> GetCachedArticleCount()
		{
			var directory = await FeedProvider.LocalStorage.GetDirectory(ParsedUri.DataPath);

			if (directory == null) return 0;

			var files = await directory.GetFiles("*.dat");

			return files.Count;
		}

		public override async Task DeleteCache()
		{
			await FeedProvider.LocalStorage.DeleteFile(ParsedUri.DataPath).ConfigureAwait(false);
			await base.DeleteCache();
		}

		public static async Task<MegalithFeedSummary> LoadFile(MegalithLocation location, MegalithFeedUri uri, string name)
		{
			var rt = await LoadMeta<MegalithFeedSummary>(location, location.FeedProvider.LocalStorage, uri, name);

			if (rt != null)
				return rt;
			else
			{
				rt = new MegalithFeedSummary(location, uri)
				{
					Name = name,
				};
				var data = await location.FeedProvider.LocalStorage.ReadLines(uri.DataPath, MegalithFeedProvider.Encoding);

				if (data != null)
				{
					rt.ArticleCount = data.Count();
					await rt.Save();
				}

				return rt;
			}
		}
	}
}
