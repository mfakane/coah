using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Linearstar.Coah.Models;

namespace Linearstar.Coah.ViewModels
{
	class ClientViewModel : Conductor<ViewerViewModel>.Collection.OneActive
	{
		readonly IWindowManager winManager = IoC.Get<IWindowManager>();
		readonly IDisposable handler;

		public Client Model { get; } = App.Current.Client;
		public LocationTreeViewModel LocationTree { get; }

		public ClientViewModel()
		{
			Items.CollectionChanged += (sender, e) =>
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						foreach (ViewerViewModel i in e.NewItems)
							winManager.ShowWindow(i);

						foreach (var i in e.NewItems.Cast<ViewerViewModel>().ToDictionary(x => x, x => Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.DataContext == x)))
							if (i.Value != null)
							{
								i.Value.Activated += (sender2, e2) => ActiveItem = i.Key;
								i.Value.Closed += (sender2, e2) =>
								{
									if (Items.Contains(i.Key))
										DeactivateItem(i.Key, true);
								};
							}

						ActiveItem = e.NewItems.Cast<ViewerViewModel>().Last();

						break;
					case NotifyCollectionChangedAction.Replace:
					case NotifyCollectionChangedAction.Remove:
						foreach (var i in e.OldItems.Cast<ViewerViewModel>().ToDictionary(x => x, x => Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.DataContext == x)))
							i.Value?.Close();

						if (e.Action == NotifyCollectionChangedAction.Replace)
							goto case NotifyCollectionChangedAction.Add;

						break;
				}
			};
			LocationTree = new LocationTreeViewModel(this);
			handler = new CompositeDisposable
			(
				Model.OnPropertyChanged(nameof(Model.CurrentViewer)).Subscribe(e => ActiveItem = Items.FirstOrDefault(x => x.Model == Model.CurrentViewer)),
				ObservableCollectionWrapper.Create(Model.Viewers, Items, (IViewer x) => new ViewerViewModel(x), x => x.Model)
			);
		}

		public async Task Load()
		{
			if (Model.IsLoaded)
				return;

			await Model.Load();
			await LocationTree.Load();
		}

		public void ShowSettings() =>
			IoC.Get<IWindowManager>().ShowDialog(new SettingsViewModel(Model.Settings));

		protected override void OnDeactivate(bool close)
		{
			if (close)
				handler.Dispose();

			base.OnDeactivate(close);
		}

		protected override void ChangeActiveItem(ViewerViewModel newItem, bool closePrevious)
		{
			var window = Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.DataContext == newItem);

			if (window != null && !window.IsActive)
				window.Activate();

			Model.CurrentViewer = newItem?.Model;

			base.ChangeActiveItem(newItem, closePrevious);
		}
	}
}
