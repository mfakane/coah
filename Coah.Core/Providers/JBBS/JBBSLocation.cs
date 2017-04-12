using System;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.JBBS
{
	public class JBBSLocation : Location
	{
		readonly WeakInstanceStore<Uri, JBBSFeedSummary> instances = new WeakInstanceStore<Uri, JBBSFeedSummary>();

		public JBBSFeedProvider FeedProvider => (JBBSFeedProvider)Provider;

		public JBBSLocation(FeedProvider provider, Uri uri)
			: base(provider, uri)
		{
		}

		public override async Task<FeedSummary> GetFeedSummaryInstance(Uri uri) =>
			await instances.GetOrAddAsync(uri, _ => JBBSFeedSummary.LoadFile(this, new JBBSFeedUri(uri), Name)).ConfigureAwait(false);

		protected override async Task RefreshCore(CancellationToken cancellationToken, IProgress<ProgressInfo> progress) =>
			Items = new[] { await GetFeedSummaryInstance(Uri) };
	}
}