using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.Megalopolis
{
	public class MegalopolisLocation : Location
	{
		readonly WeakInstanceStore<Uri, MegalopolisFeedSummary> instances = new WeakInstanceStore<Uri, MegalopolisFeedSummary>();

		public MegalopolisFeedProvider FeedProvider => (MegalopolisFeedProvider)Provider;
		public override bool IsSingleFeedLocation => false;

		public MegalopolisLocationUri ParsedUri
		{
			get;
			set;
		}

		public ConfigurationResponse Configuration
		{
			get;
			set;
		}

		public MegalopolisLocation(FeedProvider provider, Uri uri)
			: base(provider, uri) =>
			ParsedUri = new MegalopolisLocationUri(uri);

		public static async Task<MegalopolisLocation> LoadFile(MegalopolisFeedProvider provider, Uri uri, CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			var rt = new MegalopolisLocation(provider, uri);
			var data = await provider.LocalStorage.GetFile(rt.ParsedUri.ConfigurationPath);

			if (data != null)
				using (var fs = await data.Open(FileAccessMode.Read))
					rt.Configuration = (ConfigurationResponse)new DataContractJsonSerializer(typeof(ConfigurationResponse)).ReadObject(fs);

			return rt;
		}

		public override Task<FeedSummary> GetFeedSummaryInstance(Uri uri) =>
			GetFeedSummaryInstance(new MegalopolisFeedUri(uri));

		async Task<FeedSummary> GetFeedSummaryInstance(MegalopolisFeedUri pu) =>
			await instances.GetOrAddAsync(pu.AbsoluteUri, _ => MegalopolisFeedSummary.LoadFile(this, pu, "作品集 " + pu.AbsoluteUri.Segments.Last().Trim('/'))).ConfigureAwait(false);

		protected override async Task RefreshCore(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			using (var hc = FeedProvider.CreateClient())
			{
				using (var res = await hc.GetAsync(ParsedUri.ConfigurationUri, cancellationToken).ConfigureAwait(false))
					if (res.IsSuccessStatusCode)
					{
						var data = await res.Content.ReadAsByteArrayAsync();

						using (var ms = new MemoryStream(data))
						{
							await FeedProvider.LocalStorage.WriteAllBytes(ParsedUri.ConfigurationPath, data);
							Configuration = (ConfigurationResponse)new DataContractJsonSerializer(typeof(ConfigurationResponse)).ReadObject(ms);
						}
					}
					else
						res.EnsureSuccessStatusCode();

				using (var res = await hc.GetAsync(new Uri(ParsedUri.AbsoluteUri, "0.json"), cancellationToken))
					if (res.IsSuccessStatusCode)
						using (var ns = await res.Content.ReadAsStreamAsync())
						{
							var subj = (SubjectResponse)new DataContractJsonSerializer(typeof(SubjectResponse)).ReadObject(ns);

							Items = await Enumerable.Range(0, subj.SubjectCount).Select(_ => GetFeedSummaryInstance(new MegalopolisFeedUri(this, _ + 1))).WhenAll();
						}
					else
						res.EnsureSuccessStatusCode();
			}
		}
	}
}
