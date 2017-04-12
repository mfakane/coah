using System;
using RazorEngine.Templating;

namespace Linearstar.Coah
{
	public class SkinResource
	{
		public string CommentMarkerId { get; } = "__" + Guid.NewGuid().ToString() + "__";
		public string CommentMarker => "<!--" + CommentMarkerId + "-->";
		public Article Article { get; }

		public Uri BaseUri
		{
			get;
			set;
		}

		public Func<ArticleComment, TemplateWriter> CommentHelper
		{
			get;
			set;
		}

		public SkinResource(Article article) =>
			Article = article;
	}
}
