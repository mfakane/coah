using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalith
{
	public class MegalithLocation : Location
	{
		readonly WeakInstanceStore<Uri, MegalithFeedSummary> instances = new WeakInstanceStore<Uri, MegalithFeedSummary>();

		public MegalithFeedProvider FeedProvider => (MegalithFeedProvider)Provider;
		public override bool IsSingleFeedLocation => false;

		public MegalithLocationUri ParsedUri
		{
			get;
			set;
		}

		public MegalithSettings Settings
		{
			get;
			set;
		}

		public MegalithLocation(FeedProvider provider, Uri uri)
			: base(provider, uri) =>
			ParsedUri = new MegalithLocationUri(uri);

		public static async Task<MegalithLocation> LoadFile(MegalithFeedProvider provider, Uri uri, CancellationToken cancellationToken, IProgress<ProgressInfo> _)
		{
            cancellationToken.ThrowIfCancellationRequested();

			var rt = new MegalithLocation(provider, uri);
			var settings = await provider.LocalStorage.ReadAllText(rt.ParsedUri.SettingsPath, MegalithFeedProvider.Encoding);

			if (settings != null)
				rt.Settings = MegalithSettings.Parse(settings);

			var items = await provider.LocalStorage.ReadAllText(rt.ParsedUri.SubjectsPath, MegalithFeedProvider.Encoding);

			if (items != null)
				rt.Items = await Enumerable.Range(0, items.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length)
					.Select(x => rt.GetFeedSummaryInstance(new Uri(rt.ParsedUri.AbsoluteUri + "?log=" + (x + 1)))).WhenAll();

			return rt;
		}

		public override Task<FeedSummary> GetFeedSummaryInstance(Uri uri) =>
			GetFeedSummaryInstance(new MegalithFeedUri(uri));

		async Task<FeedSummary> GetFeedSummaryInstance(MegalithFeedUri pu) =>
			await instances.GetOrAddAsync(pu.AbsoluteUri, _ => MegalithFeedSummary.LoadFile(this, pu, "作品集 " + pu.AbsoluteUri.Query("log"))).ConfigureAwait(false);

		protected override async Task RefreshCore(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			using (var hc = FeedProvider.CreateClient())
			{
				using (var res = await hc.GetAsync(ParsedUri.SettingsUri, cancellationToken))
					if (res.IsSuccessStatusCode)
					{
						var data = await res.Content.ReadAsByteArrayAsync();

						await FeedProvider.LocalStorage.WriteAllBytes(ParsedUri.SettingsPath, data);
						Settings = MegalithSettings.Parse(MegalithFeedProvider.GetString(data));
					}
					else
						res.EnsureSuccessStatusCode();

				using (var res = await hc.GetAsync(ParsedUri.SubjectsUri, cancellationToken))
					if (res.IsSuccessStatusCode)
					{
						var lines = MegalithFeedProvider.GetString(await res.Content.ReadAsByteArrayAsync()).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

						await FeedProvider.LocalStorage.WriteAllLines(ParsedUri.SubjectsPath, lines, MegalithFeedProvider.Encoding);
						Items = await Enumerable.Range(0, lines.Length).Select(x => GetFeedSummaryInstance(new MegalithFeedUri(this, x + 1))).WhenAll();
					}
					else
						res.EnsureSuccessStatusCode();
			}
		}
	}
}