using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Linearstar.Coah.Megalith;

namespace Linearstar.Coah.Models
{
	[Export(typeof(IClient)), Shared]
	class Client : NotifyBase, IClient
	{
		public bool IsLoaded { get; private set; }

		public CompositionHost Host { get; set; }

		[Import]
		public ISpecialDirectories SpecialDirectories { get; set; }
		[Import]
		public IClientSettings Settings { get; set; }

		public IReadOnlyCollection<FeedProvider> Providers { get; private set; }
		public ObservableCollection<IViewer> Viewers { get; } = new ObservableCollection<IViewer>();
		public IList<ILocationItem> Locations { get; } = new ObservableCollection<ILocationItem>();

		public IViewer CurrentViewer
		{
			get => GetValue<IViewer>();
			set => SetValue(value);
		}

		[ImportingConstructor]
		public Client([ImportMany] IEnumerable<FeedProvider> providers)
		{
			Viewers.Add(new Viewer(this));
			Providers = providers.OrderByDescending(_ => _.Metadata.Priority).ToArray();
		}

		public async Task Load()
		{
			if (IsLoaded)
				return;

			IsLoaded = true;

			Locations.Add(CreateLocation(await GetLocation(new Uri("http://coolier.dip.jp/sosowa/ssw_l/")), "創", "東方創想話"));
			Locations.Add(CreateLocation(await GetLocation(new Uri("http://jbbs.shitaraba.net/internet/22214/")), "寄", "東方寄書板"));
			Locations.Add(CreateLocation(await GetLocation<MegalithFeedProvider>(new Uri("http://coolier.dip.jp/sosowa/usouso/")), "嘘", "偽型・東方嘘々話"));
		}

		Location CreateLocation(Location location, string shortName, string name)
		{
			location.ShortName = shortName;
			location.Name = name;

			return location;
		}

		public Task<Location> GetLocation(Uri uri) =>
			Providers.Select(_ => _.GetLocation(uri, false).ToObservable()).Concat().FirstOrDefaultAsync(_ => _ != null).ToTask();

		public Task<Location> GetLocation<TFeedProvider>(Uri uri)
			where TFeedProvider : FeedProvider =>
			Providers.OfType<TFeedProvider>().Select(_ => _.GetLocation(uri, false).ToObservable()).Concat().FirstOrDefaultAsync(_ => _ != null).ToTask();

		public Task<FeedSummary> GetFeedSummary(Uri uri) =>
			Providers.Select(_ => _.GetFeedSummary(uri, false).ToObservable()).Concat().FirstOrDefaultAsync(_ => _ != null).ToTask();

		public Task<ArticleSummary> GetArticleSummary(Uri uri) =>
			Providers.Select(_ => _.GetArticleSummary(uri, false).ToObservable()).Concat().FirstOrDefaultAsync(_ => _ != null).ToTask();

		public ArticlePage ShowArticle(ArticleSummary articleSummary)
		{
			foreach (var i in Viewers)
			{
				var match = i.Pages.OfType<ArticlePage>().FirstOrDefault(_ => _.ArticleSummary == articleSummary);

				if (match != null)
				{
					CurrentViewer = i;
					i.CurrentPage = match;

					return match;
				}
			}

			var viewer = CurrentViewer;
			var rt = new ArticlePage(articleSummary);

			viewer.Pages.Add(rt);
			viewer.CurrentPage = rt;

			return rt;
		}

		public FeedPage ShowFeed(FeedSummary feedSummary)
		{
			foreach (var i in Viewers)
			{
				var match = i.Pages.OfType<FeedPage>().FirstOrDefault(_ => _.FeedSummary == feedSummary);

				if (match != null)
				{
					CurrentViewer = i;
					i.CurrentPage = match;

					return match;
				}
			}

			var viewer = CurrentViewer;
			var rt = new FeedPage(feedSummary);

			viewer.Pages.Add(rt);
			viewer.CurrentPage = rt;

			return rt;
		}
	}
}
