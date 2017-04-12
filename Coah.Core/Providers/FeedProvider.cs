using System;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public abstract class FeedProvider
	{
		FeedProviderAttribute metadata;

		public FeedProviderAttribute Metadata =>
			metadata ?? (metadata = GetType().GetTypeInfo().GetCustomAttribute<FeedProviderAttribute>());

		[Import]
		public IClient Client
		{
			get;
			set;
		}

		public abstract Task<Location> GetLocation(Uri uri, bool isForced);
		public abstract Task<FeedSummary> GetFeedSummary(Uri uri, bool isForced);
		public abstract Task<ArticleSummary> GetArticleSummary(Uri uri, bool isForced);

		public virtual IReadOnlyCollection<DisplayRange> GetArticleDisplayRange(Uri uri) =>
			new[] { DisplayRange.All };
	}
}
