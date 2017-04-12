using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Linearstar.Coah.ViewModels
{
	class ArticleSummaryViewModel : NotifyBase
	{
		public IViewer Viewer { get; }
		public ArticleSummary Model { get; }
		public bool IsSelected { get; set; }
		public string Tags => HasTags ? string.Join(", ", Model.Tags) : null;
		public bool HasTags => Model.Tags?.Any() ?? false;

		public ArticleSummaryViewModel(IViewer viewer, ArticleSummary model)
		{
			Viewer = viewer;
			Model = model;
		}

		public void Show() =>
			Viewer.Client.ShowArticle(Model);

		public void OpenInBrowser() =>
			Process.Start(Model.AbsoluteUri.ToString());

		public void ShowSimiliarArticles() =>
			Viewer.Client.ShowFeed(Model.FeedSummary).GetSimilarArticles(Model.Title);

		public void DeleteCache() =>
			Task.Run(Model.DeleteCache);
	}
}
