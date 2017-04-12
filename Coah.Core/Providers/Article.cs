using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public abstract class Article
	{
		public FeedProvider Provider => Summary.Provider;

		public virtual ArticleSummary Summary
		{
			get;
		}

		public virtual bool IsLoaded => Comments != null;

		public virtual IList<ArticleComment> Comments
		{
			get;
			set;
		}

		public Article(ArticleSummary summary)
		{
			Summary = summary;
		}

		public abstract Task Refresh(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);

		public virtual async Task DeleteCache()
		{
			await Summary.DeleteCache();
			Comments = null;
		}
	}
}
