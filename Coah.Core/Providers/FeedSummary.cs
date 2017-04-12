using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	[DataContract]
	public abstract class FeedSummary : NotifyBase, ILocationItem
	{
		public FeedProvider Provider => Location.Provider;

		public virtual Location Location
		{
			get;
			set;
		}

		[DataMember]
		public virtual Uri Uri
		{
			get => GetValue<Uri>();
			set => SetValue(value);
		}

		public virtual Uri AbsoluteUri => Uri;

		[DataMember]
		public virtual string Name
		{
			get => GetValue<string>();
			set => SetValue(value);
		}

		[DataMember]
		public virtual DateTimeOffset? LastModified
		{
			get => GetValue<DateTimeOffset?>();
			set => SetValue(value);
		}

		[DataMember]
		public virtual int ArticleCount
		{
			get => GetValue<int>();
			set => SetValue(value);
		}

		[DataMember]
		public virtual int CachedArticleCount
		{
			get => GetValue<int>();
			set => SetValue(value);
		}

		public virtual bool CanGetCachedArticles => true;

		IList<ILocationItem> ILocationItem.Items => null;
		Task ILocationItem.Refresh(CancellationToken cts, IProgress<ProgressInfo> progress) => Task.Delay(0);

		public FeedSummary(Location location, Uri uri)
		{
			Location = location;
			Uri = uri;
		}

		public abstract Task<int> GetCachedArticleCount();
		public abstract Task Save();
		public abstract ValueTask<Feed> GetFeed(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);

		public virtual Task DeleteCache()
		{
			LastModified = null;
			ArticleCount = 0;

			return Task.Delay(0);
		}
	}
}
