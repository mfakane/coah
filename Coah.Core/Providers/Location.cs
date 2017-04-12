using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public abstract class Location : ILocationItem
	{
		public FeedProvider Provider
		{
			get;
			set;
		}

		public virtual Uri Uri
		{
			get;
			set;
		}

		public virtual string Name
		{
			get;
			set;
		}

		public virtual string ShortName
		{
			get;
			set;
		}

		public virtual IList<FeedSummary> Items
		{
			get;
			set;
		}

		IList<ILocationItem> ILocationItem.Items => (IList<ILocationItem>)Items;

		public bool HasShortName => !string.IsNullOrEmpty(ShortName);
		public virtual bool IsSingleFeedLocation => true;

		public Location(FeedProvider provider, Uri uri)
		{
			Provider = provider;
			Uri = uri;
		}

		public async Task Refresh(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			try
			{
				progress.Report(ProgressInfo.Download(this));
				await RefreshCore(cancellationToken, progress).ConfigureAwait(false);
				progress.Report(ProgressInfo.Done(this));
			}
			catch (OperationCanceledException)
			{
				progress.Report(ProgressInfo.Canceled(this));
			}
			catch (Exception ex)
			{
				progress.Report(ProgressInfo.Error(this, ex));
			}
		}

		protected abstract Task RefreshCore(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);
		public abstract Task<FeedSummary> GetFeedSummaryInstance(Uri uri);
	}
}
