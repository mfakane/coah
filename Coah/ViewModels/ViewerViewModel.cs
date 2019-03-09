using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Linearstar.Coah.ViewModels
{
	class ViewerViewModel : Conductor<IPageViewModel>.Collection.OneActive
	{
		readonly IDisposable handler;
		ProgressInfo currentProgress;

		public IViewer Model { get; }
		public ClientViewModel Client => (ClientViewModel)Parent;

		public ProgressInfo CurrentProgress
		{
			get => currentProgress;
			private set
			{
				currentProgress = value;
				NotifyOfPropertyChange();
			}
		}

		public override string DisplayName
		{
			get =>
				string.Join(" ", new[]
				{
					Items.Count > 1 ? $"({Items.Count})" : null,
					ActiveItem?.DisplayName ?? "Coah",
				}.Where(x => !string.IsNullOrEmpty(x)));
			set
			{
			}
		}

		public string DisplayTitle => DisplayName + (DisplayName == "Coah" ? null : " - Coah");

		public ViewerViewModel(IViewer model)
		{
			Model = model;
			Items.CollectionChanged += (sender, e) =>
			{
				NotifyOfPropertyChange(nameof(DisplayName));
				NotifyOfPropertyChange(nameof(DisplayTitle));

				if (e.NewItems != null)
					foreach (IPageViewModel i in e.NewItems)
						i.ProgressState.PropertyChanged += ProgressState_PropertyChanged;

				if (e.OldItems != null)
					foreach (IPageViewModel i in e.OldItems)
						i.ProgressState.PropertyChanged -= ProgressState_PropertyChanged;

				CurrentProgress = Items.Where(x => x.ProgressState.CurrentProgress != null)
												 .OrderBy(x => e.NewItems == null || !e.NewItems.Contains(x))
												 .ThenBy(x => x.ProgressState.CurrentProgress.HasCompleted)
												 .Select(x => x.ProgressState.CurrentProgress)
												 .FirstOrDefault();
			};
			PropertyChanged += (sender, e) =>
			{
				switch (e.PropertyName)
				{
					case nameof(ActiveItem):
						Model.CurrentPage = ActiveItem?.Model;
						NotifyOfPropertyChange(nameof(DisplayName));
						NotifyOfPropertyChange(nameof(DisplayTitle));

						break;
				}
			};
			handler = new CompositeDisposable
			(
				Model.OnPropertyChanged(nameof(Model.CurrentPage)).Subscribe(e => ActiveItem = Items.FirstOrDefault(x => x.Model == Model.CurrentPage)),
				ObservableCollectionWrapper.Create(Model.Pages, Items, (IPage p) =>
				{
					switch (p)
					{
						case FeedPage feedPage: return new FeedPageViewModel(feedPage);
						case ArticlePage articlePage: return new ArticlePageViewModel(articlePage);
						default: return (IPageViewModel)null;
					}
				}, x => x.Model)
			);
		}

		void ProgressState_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var state = (ProgressState)sender;

			if (CurrentProgress?.HasCompleted ?? true)
				CurrentProgress = state.CurrentProgress;
			else
				CurrentProgress = Items.Select(x => x.ProgressState.CurrentProgress).FirstOrDefault(x => x != null && (x == state.CurrentProgress || !x.HasCompleted));
		}

		public void Activate() =>
			Client.ActivateItem(this);

		protected override async void OnViewReady(object view) =>
			await Load();

		public Task Load() =>
			Client.Load();
	}
}
