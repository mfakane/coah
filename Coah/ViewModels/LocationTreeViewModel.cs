using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Linearstar.Coah.ViewModels
{
	class LocationTreeViewModel : ViewAware
	{
		public ObservableCollection<LocationViewModel> Items { get; } = new ObservableCollection<LocationViewModel>();
		public ClientViewModel Client { get; }
		public ProgressState ProgressState { get; } = new ProgressState();

		public LocationTreeViewModel(ClientViewModel client)
		{
			Client = client;
		}

		public Task Load()
		{
			foreach (var i in Client.Model.Locations)
				Items.Add(new LocationViewModel(i));

			return Task.WhenAll(Items.Select(x => x.Load(ProgressState)));
		}

		public void ShowLocation(LocationViewModel item)
		{
			if (item.Location is Location && ((Location)item.Location).IsSingleFeedLocation)
				Client.Model.ShowFeed((FeedSummary)item.Location.Items.First());
			else if (item.Location is FeedSummary)
				Client.Model.ShowFeed((FeedSummary)item.Location);
		}
	}
}
