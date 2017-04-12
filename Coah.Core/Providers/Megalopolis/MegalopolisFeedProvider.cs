using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalopolis
{
	[FeedProvider("Megalopolis", Priority = -1)]
	public class MegalopolisFeedProvider : FeedProvider
	{
		readonly WeakInstanceStore<Uri, MegalopolisLocation> instances = new WeakInstanceStore<Uri, MegalopolisLocation>();

		public IDirectory LocalStorage { get; private set; }

		async Task EnsureLocalStorage() =>
			LocalStorage = await Client.SpecialDirectories.Cache.CreateDirectory(Metadata.DisplayName);

		public override async Task<ArticleSummary> GetArticleSummary(Uri uri, bool isForced)
		{
			await EnsureLocalStorage().ConfigureAwait(false);

			var parsedUri = new MegalopolisArticleUri(uri);
			var feedSummary = await GetFeedSummary(parsedUri.Feed.AbsoluteUri, true);
			var feed = await feedSummary.GetFeed(CancellationToken.None, null);

			return await feed.GetArticleSummaryInstance(parsedUri.AbsoluteUri);
		}

		public override async Task<FeedSummary> GetFeedSummary(Uri uri, bool isForced)
		{
			await EnsureLocalStorage().ConfigureAwait(false);

			return await (await GetLocation(uri, true)).GetFeedSummaryInstance(uri);
		}

		public override async Task<Location> GetLocation(Uri uri, bool isForced)
		{
			await EnsureLocalStorage().ConfigureAwait(false);

			return instances.GetOrAdd(uri, _ => new MegalopolisLocation(this, uri));
		}

		public HttpClient CreateClient()
		{
			var handler = new HttpClientHandler
			{
				AllowAutoRedirect = false,
				AutomaticDecompression = DecompressionMethods.GZip,
			};

			if (!Client.Settings.UseSystemReadProxy)
			{
				handler.UseProxy = Client.Settings.ReadProxy != null;
				handler.Proxy = Client.Settings.ReadProxy;
			}

			return new HttpClient(handler)
			{
				DefaultRequestHeaders =
				{
					AcceptEncoding =
					{
						new StringWithQualityHeaderValue("gzip"),
					},
				},
			};
		}
	}
}
