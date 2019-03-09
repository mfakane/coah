using System;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace Linearstar.Coah
{
	public class SkinTemplate<TArticleSummary, TArticle, TArticleComment> : TemplateBase<SkinResource>
		where TArticleSummary : ArticleSummary
		where TArticle : Article
		where TArticleComment : ArticleComment
	{
		public TArticle Article => (TArticle)Model.Article;
		public TArticleSummary Summary => (TArticleSummary)Article.Summary;
		public Uri BaseUri => Model.BaseUri;

		public IEncodedString RenderBody(string body) =>
			new RawString(body);

		public IEncodedString Comments(Func<TArticleComment, TemplateWriter> commentHelper)
		{
			Model.CommentHelper = x => commentHelper((TArticleComment)x);

			return new RawString(Model.CommentMarker);
		}

		public IEncodedString CommentId(TArticleComment comment) =>
			new HtmlEncodedString("res-" + comment.Index);

		public IEncodedString CommentIndex(TArticleComment comment) =>
			new RawString($"<a href=\"#menu-{comment.Index}\">{comment.Index}</a><a href=\"#ref-{comment.Index}\" class=\"reference-counter\"></a>");

		public IEncodedString CommentSenderIdentifier(TArticleComment comment) =>
			new RawString($"<a href=\"#menu-id-{comment.Index}\">{comment.Sender.Identifier}</a> <a href=\"#id-{comment.Sender.Identifier}\" class=\"sender-identifier-counter\"></a>");

		public override string ResolveUrl(string path)
		{
			if (path.StartsWith("~"))
				return BaseUri + path.Substring(1);
			else if (Uri.IsWellFormedUriString(path, UriKind.Relative))
				return new Uri(BaseUri, new Uri(path, UriKind.Relative)).ToString();
			else
				return path;
		}
	}
}
