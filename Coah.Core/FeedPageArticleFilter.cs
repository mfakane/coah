using System;
using System.Collections.Generic;

namespace Linearstar.Coah
{
	public class FeedPageArticleFilter
	{
		readonly Func<FeedPage, IReadOnlyList<ArticleSummary>, IReadOnlyList<ArticleSummary>> filter;

		public string Name
		{
			get;
		}

		public FeedPageArticleFilter(string name, Func<FeedPage, IReadOnlyList<ArticleSummary>, IReadOnlyList<ArticleSummary>> filter)
		{
			Name = name;
			this.filter = filter;
		}

		public IReadOnlyList<ArticleSummary> Filter(FeedPage page, IReadOnlyList<ArticleSummary> articles) =>
			filter(page, articles);
	}
}
