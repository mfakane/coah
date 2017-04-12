using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Linearstar.Coah.ViewModels
{
	class ArticlePageViewModel : Screen, IPageViewModel
	{
		CompositeDisposable handler;
		IReadOnlyList<IReadOnlyCollection<ArticleCommentViewModel>> comments;
		CancellationTokenSource cts;

		public ViewerViewModel Viewer => (ViewerViewModel)Parent;
		IPage IPageViewModel.Model => Model;
		public ArticlePage Model { get; }
		public ArticleSummaryViewModel ArticleSummary { get; }
		public ProgressState ProgressState => Model.ProgressState;

		public override string DisplayName
		{
			get => Model.ArticleSummary.Title;
			set => throw new NotSupportedException();
		}

		public IReadOnlyList<IReadOnlyCollection<ArticleCommentViewModel>> Comments
		{
			get => comments;
			private set
			{
				comments = value;
				NotifyOfPropertyChange();
			}
		}

		public string DisplayRangeString => string.Join(", ", Model.DisplayRanges);

		public string SelectedText
		{
			get;
			set;
		}

		public ArticlePageViewModel(ArticlePage model)
		{
			Model = model;
			ArticleSummary = new ArticleSummaryViewModel(model.Viewer, model.ArticleSummary);
		}

		protected override void OnActivate()
		{
			handler = new CompositeDisposable
			(
				Model.OnPropertyChanged(nameof(Model.DisplayRanges)).Subscribe(_ => NotifyOfPropertyChange(nameof(DisplayRangeString))),
				Model.OnPropertyChanged(nameof(Model.Comments)).Subscribe(e =>
				{
					if (Model.Comments == null)
						Comments = null;
					else
					{
						var wrapper = ObservableCollectionWrapper.Create(Model.Comments, (IReadOnlyCollection<ArticleComment> items) => items.Select(_ => new ArticleCommentViewModel(_)).ToArray());

						Comments = wrapper;
						handler.Add(wrapper);
					}
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
			if (!Model.Article?.IsLoaded ?? true)
				Task.Run(RefreshCore);
		}

		public void CopySelectedText() =>
			Clipboard.SetText(SelectedText);

		public void ShowFeed() =>
			Viewer.Client.Model.ShowFeed(Model.ArticleSummary.FeedSummary);

		public void Close() =>
			TryClose();

		public void DeleteCacheAndClose()
		{
			Task.Run(() => Model.DeleteCache().ConfigureAwait(false));
			Close();
		}

		public bool CanRefreshView() => !ProgressState.IsBusy;
		public void RefreshView() =>
			Task.Run(RefreshCore);

		public bool CanRefreshHard() => !ProgressState.IsBusy;
		public void RefreshHard() =>
			Task.Run(async () =>
			{
				await Model.DeleteCache().ConfigureAwait(false);
				await RefreshCore();
			});

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

		public void SetDisplayRangeTop50() =>
			Model.DisplayRanges = new[] { DisplayRange.Top50 };

		public void SetDisplayRangeLatest50() =>
			Model.DisplayRanges = new[] { DisplayRange.Latest50 };

		public void SetDisplayRangeAll() =>
			Model.DisplayRanges = new[] { DisplayRange.All };

		async Task RefreshCore()
		{
			using (var cts = this.cts = new CancellationTokenSource())
			using (Disposable.Create(() => this.cts = null))
				await Model.Refresh(cts.Token);
		}
	}
}
