using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.JBBS
{
	[FeedProvider("JBBS")]
	public class JBBSFeedProvider : FeedProvider
	{
		public static readonly Encoding Encoding = Encoding.GetEncoding("EUC-JP");
		static readonly Regex feedRegex = new Regex(@"^http://(jbbs\.livedoor\.ne\.jp|jbbs\.shitaraba\.net)/[a-z]+/[0-9]+/$");
		static readonly Regex articleRegex = new Regex(@"^http://(jbbs\.livedoor\.ne\.jp|jbbs\.shitaraba\.net)/bbs/read\.cgi/[a-z]+/[0-9]+/[0-9]+/[l0-9\-]*$");

		readonly WeakInstanceStore<Uri, JBBSLocation> instances = new WeakInstanceStore<Uri, JBBSLocation>();

		public IDirectory LocalStorage { get; private set; }

		async Task EnsureLocalStorage() =>
			LocalStorage = await Client.SpecialDirectories.Cache.CreateDirectory(Metadata.DisplayName);

		public override async Task<Location> GetLocation(Uri uri, bool isForced)
		{
			await EnsureLocalStorage().ConfigureAwait(false);

			if (!isForced && !feedRegex.IsMatch(uri.AbsoluteUri))
				return null;

			return instances.GetOrAdd(uri, _ => new JBBSLocation(this, uri));
		}

		public override async Task<FeedSummary> GetFeedSummary(Uri uri, bool isForced)
		{
			await EnsureLocalStorage().ConfigureAwait(false);

			if (!isForced && !feedRegex.IsMatch(uri.AbsoluteUri))
				return null;

			return await (await GetLocation(uri, true)).GetFeedSummaryInstance(uri);
		}

		public override async Task<ArticleSummary> GetArticleSummary(Uri uri, bool isForced)
		{
			await EnsureLocalStorage().ConfigureAwait(false);

			if (!isForced && !articleRegex.IsMatch(uri.AbsoluteUri))
				return null;

			var parsedUri = new JBBSArticleUri(uri);
			var feedSummary = await GetFeedSummary(parsedUri.Feed.AbsoluteUri, true);
			var feed = await feedSummary.GetFeed(CancellationToken.None, null);

			return await feed.GetArticleSummaryInstance(parsedUri.AbsoluteUri);
		}

		public override IReadOnlyCollection<DisplayRange> GetArticleDisplayRange(Uri uri)
		{
			var seg = uri.Segments.Last();

			if (seg.Cast<char>().All(_ => char.IsDigit(_) || _ == 'l'))
				return new[] { new DisplayRange(int.Parse(seg.Replace("l", ""))) };
			else if (seg.Cast<char>().All(_ => char.IsDigit(_) || _ == '-'))
			{
				var sl = seg.Split('-');

				return new[] { new DisplayRange(string.IsNullOrEmpty(sl[0]) ? (int?)null : int.Parse(sl[0]), string.IsNullOrEmpty(sl[1]) ? (int?)null : int.Parse(sl[1])) };
			}
			else
				return new[] { DisplayRange.All };
		}

		public static string GetString(byte[] data) =>
			Encoding.GetString(data, 0, data.Length);

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
