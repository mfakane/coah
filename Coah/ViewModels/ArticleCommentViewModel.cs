using System.Diagnostics;
using System.Windows;

namespace Linearstar.Coah.ViewModels
{
	public class ArticleCommentViewModel
	{
		public ArticleComment Comment
		{
			get;
		}

		public ArticleCommentViewModel(ArticleComment comment)
		{
			Comment = comment;
		}

		public void OpenInBrowser() =>
			Process.Start(Comment.Uri.ToString());

		public void CopyIdentifier() =>
			Clipboard.SetText(Comment.Sender.Identifier);
	}
}