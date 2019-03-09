using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Linearstar.Coah.Annotations;

namespace Linearstar.Coah.ViewModels
{
	class LocationViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		IList<LocationViewModel> items;

		public ILocationItem Location
		{
			get;
		}

		public IList<LocationViewModel> Items
		{
			get => items;
			set
			{
				items = value;
				OnPropertyChanged();
			}
		}

		public LocationViewModel(ILocationItem location) =>
			Location = location;

		public Task Load(IProgress<ProgressInfo> progress) =>
			Task.Run(async () =>
			{
				await Location.Refresh(CancellationToken.None, progress);

				if (Location.Items?.Count == 0 ||
					Location is Location && ((Location)Location).IsSingleFeedLocation)
					Items = null;
				else if (Location is Location)
					Items = Location.Items?.Select(x => new LocationViewModel(x)).Reverse().ToArray();
				else
					Items = Location.Items?.Select(x => new LocationViewModel(x)).ToArray();
			});

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
