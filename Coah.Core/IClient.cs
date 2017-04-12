using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition.Hosting;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public interface IClient
	{
		CompositionHost Host { get; }
		IReadOnlyCollection<FeedProvider> Providers { get; }
		IClientSettings Settings { get; }
		ISpecialDirectories SpecialDirectories { get; }
		IList<ILocationItem> Locations { get; }
		IViewer CurrentViewer { get; set; }
		ObservableCollection<IViewer> Viewers { get; }

		Task<Location> GetLocation(Uri uri);
		Task<Location> GetLocation<TFeedProvider>(Uri uri) where TFeedProvider : FeedProvider;
		Task<FeedSummary> GetFeedSummary(Uri uri);
		Task<ArticleSummary> GetArticleSummary(Uri uri);

		FeedPage ShowFeed(FeedSummary feedSummary);
		ArticlePage ShowArticle(ArticleSummary articleSummary);
	}
}
