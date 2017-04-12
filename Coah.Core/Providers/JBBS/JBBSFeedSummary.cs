using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.JBBS
{
	[DataContract]
	public class JBBSFeedSummary : JsonFeedSummaryBase<JBBSFeedProvider, JBBSFeedUri>
	{
		protected override IDirectory LocalStorage => FeedProvider.LocalStorage;
		protected override Func<FeedSummary, CancellationToken, IProgress<ProgressInfo>, Task<Feed>> LoadFeedFromFile =>
			async (summary, cancellationToken, progress) => await JBBSFeed.LoadFile((JBBSFeedSummary)summary, cancellationToken, progress).ConfigureAwait(false);

		JBBSFeedSummary(Location location, JBBSFeedUri uri)
			: base(location, uri)
		{
		}

		public override async Task<int> GetCachedArticleCount()
		{
			var directory = await FeedProvider.LocalStorage.GetDirectory(ParsedUri.DataPath).ConfigureAwait(false);

			if (directory == null) return 0;

			var files = await directory.GetFiles("*.dat");

			return files.Count;
		}

		public override async Task DeleteCache()
		{
			await FeedProvider.LocalStorage.DeleteFile(ParsedUri.DataPath).ConfigureAwait(false);
			await base.DeleteCache();
		}

		public static async Task<JBBSFeedSummary> LoadFile(JBBSLocation location, JBBSFeedUri uri, string name)
		{
			var rt = await LoadMeta<JBBSFeedSummary>(location, location.FeedProvider.LocalStorage, uri, name).ConfigureAwait(false);

			if (rt != null)
				return rt;
			else
			{
				var data = await location.FeedProvider.LocalStorage.ReadLines(uri.DataPath, JBBSFeedProvider.Encoding);

				rt = new JBBSFeedSummary(location, uri)
				{
					Name = name,
				};

				if (data != null)
				{
					rt.ArticleCount = data.Distinct().Count();
					await rt.Save();
				}

				return rt;
			}
		}
	}
}