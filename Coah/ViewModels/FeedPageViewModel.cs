using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Linearstar.Coah.ViewModels
{
	class FeedPageViewModel : Screen, IPageViewModel
	{
		CompositeDisposable handler;
		IReadOnlyList<ArticleSummaryViewModel> articles;
		CancellationTokenSource cts;
		Func<IEnumerable<ArticleSummaryViewModel>, IEnumerable<ArticleSummaryViewModel>> applySort = x => x;

		public ViewerViewModel Viewer => (ViewerViewModel)Parent;
		IPage IPageViewModel.Model => Model;
		public FeedPage Model { get; }
		public ProgressState ProgressState => Model.ProgressState;

		public override string DisplayName
		{
			get => Model.FeedSummary.Name;
			set => throw new NotSupportedException();
		}

		public IReadOnlyList<ArticleSummaryViewModel> Articles
		{
			get => articles;
			set
			{
				articles = value;
				NotifyOfPropertyChange();
			}
		}

		public bool HasFilter => Model.Filters.Any();
		public string FilterString => Model.Filters.Count > 1 ? $"{Model.Filters.Count} 件のフィルタ" : Model.Filters.FirstOrDefault()?.Name;

		public FeedPageViewModel(FeedPage model) =>
			Model = model;

		public void SortArticles<TKey>(Func<ArticleSummaryViewModel, TKey> selector, bool isDescending)
		{
			applySort = x => (isDescending ? x?.OrderByDescending(selector) : x?.OrderBy(selector));
			Articles = applySort(Articles)?.ToArray();
		}

		protected override void OnActivate()
		{
			handler = new CompositeDisposable
			(
				Model.OnPropertyChanged(nameof(Model.Articles)).Subscribe(e =>
					Articles = applySort(Model.Articles?.Select(x => new ArticleSummaryViewModel(Model.Viewer, x)))?.ToArray()),
				Model.Filters.OnCollectionChanged().Subscribe(e =>
				{
					NotifyOfPropertyChange(nameof(HasFilter));
					NotifyOfPropertyChange(nameof(FilterString));
				})
			);
			base.OnActivate();
		}

		protected override void OnDeactivate(bool close)
		{
			if (close)
				handler.Dispose();

			base.OnDeactivate(close);
		}

		protected override void OnViewReady(object view)
		{
			if (!Model.Feed?.IsLoaded ?? true)
				Task.Run(() => Refresh());
		}

		public void ClearFilters() =>
			Model.Filters.Clear();

		public void ShowLocation() =>
			Process.Start(Model.FeedSummary.Location.Uri.ToString());

		public void Close() =>
			TryClose();

		public void OpenInBrowser() =>
			Process.Start(Model.FeedSummary.AbsoluteUri.ToString());

		public bool CanRefreshView() => !ProgressState.IsBusy;
		public void RefreshView() =>
			Task.Run(() => Refresh());

		public bool CanRefreshHard() => !ProgressState.IsBusy;
		public void RefreshHard() =>
			Task.Run(async () =>
			{
				await Model.Feed.DeleteCache().ConfigureAwait(false);
				await Refresh();
			});

		public bool CanRefreshCached() => !ProgressState.IsBusy;
		public void RefreshCached() =>
			Task.Run(() => Refresh(false));

		public bool CanStop() => ProgressState.IsBusy;
		public void Stop()
		{
			if (cts != null && !cts.IsCancellationRequested)
				cts.Cancel();
		}

		public void RefreshOrStop()
		{
			if (ProgressState.IsBusy)
				Stop();
			else
				RefreshView();
		}

		async Task Refresh(bool isOnline = true)
		{
			using (var cts = this.cts = new CancellationTokenSource())
			using (Disposable.Create(() => this.cts = null))
				await Model.Refresh(!isOnline, cts.Token);
		}

		public void ShowUnreadArticles()
		{
			foreach (var i in Articles)
				if (i.Model.NewCommentCount > 0)
					i.Show();
		}

		public void ShowSelectedItems()
		{
			foreach (var i in Articles.Where(x => x.IsSelected))
				i.Show();
		}

		public void OpenSelectedItemsInBrowser()
		{
			foreach (var i in Articles.Where(x => x.IsSelected))
				i.OpenInBrowser();
		}

		public void DeleteSelectedItemsCache()
		{
			foreach (var i in Articles.Where(x => x.IsSelected))
				i.DeleteCache();
		}
	}
}