using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Linearstar.Coah.Models
{
	class Viewer : NotifyBase, IViewer
	{
		public ObservableCollection<IPage> Pages { get; } = new ObservableCollection<IPage>();

		public IPage CurrentPage
		{
			get => GetValue<IPage>();
			set => SetValue(value);
		}

		public IClient Client
		{
			get;
		}

		public Viewer(IClient client)
		{
			Client = client;
			Pages.CollectionChanged += (sender, e) =>
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						foreach (IPage i in e.NewItems)
							i.Viewer = this;

						break;
					case NotifyCollectionChangedAction.Remove:
						foreach (IPage i in e.OldItems)
							i.Viewer = null;

						break;
				}
			};
		}
	}
}
