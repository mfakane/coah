using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah.JBBS
{
	public class JBBSArticle : Article
	{
		public JBBSFeedProvider FeedProvider => (JBBSFeedProvider)Provider;
		public JBBSArticleSummary ArticleSummary => (JBBSArticleSummary)Summary;

		JBBSArticle(JBBSArticleSummary summary)
			: base(summary)
		{
		}

		public static async Task<JBBSArticle> LoadFile(JBBSArticleSummary summary, CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			var rt = new JBBSArticle(summary);
			var data = await summary.FeedProvider.LocalStorage.ReadAllText(summary.ParsedUri.DataPath, JBBSFeedProvider.Encoding).ConfigureAwait(false);

			if (data != null)
				rt.Parse(data, progress);

			return rt;
		}

		public override async Task Refresh(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
		{
			using (var hc = FeedProvider.CreateClient())
			{
				progress?.Report(ProgressInfo.Download(ArticleSummary));

				using (var res = await hc.GetAsync(new Uri(ArticleSummary.ParsedUri.DataUri, Comments?.Any() ?? false ? (Comments.Count + 1) + "-" : ""), cancellationToken).ConfigureAwait(false))
					if (res.IsSuccessStatusCode)
					{
						if (res.Headers.TryGetValues("ERROR", out var errors))
						{
							var error = errors.First();

							switch (error)
							{
								case "STORAGE IN":
									using (var res2 = await hc.GetAsync(ArticleSummary.ParsedUri.ArchivedUri, cancellationToken))
									{
										var content = JBBSFeedProvider.GetString(await res2.Content.ReadAsByteArrayAsync());
										var dlIdx = content.IndexOf("<dl>" + 4, StringComparison.Ordinal);

										Comments = null;
										Parse(content.Substring(dlIdx, content.LastIndexOf("</dl>", StringComparison.Ordinal) - dlIdx), progress, JBBSArticleComment.ParseHtml);
										((JBBSArticleComment)Comments[0]).Title = Regex.Match(content, "<title>(.+?)</title>").Groups[1].Value;

										await FeedProvider.LocalStorage.WriteAllLines(ArticleSummary.ParsedUri.DataPath, Comments.Select(_ => _.ToString()), JBBSFeedProvider.Encoding);
										ArticleSummary.LastModified = res2.Content.Headers.LastModified;
									}

									break;
								case "BBS NOT FOUND":
								case "KEY NOT FOUND":
								case "THREAD NOT FOUND":
									break;
								default:
									throw new InvalidOperationException($"Server has returned an unknown error: {error}");
							}

							Summary.IsLost = true;
						}
						else if (res.Content.Headers.ContentLength > 0)
						{
							var data = await res.Content.ReadAsByteArrayAsync();

							if (Comments == null)
								await FeedProvider.LocalStorage.WriteAllBytes(ArticleSummary.ParsedUri.DataPath, data);
							else
								await FeedProvider.LocalStorage.AppendAllBytes(ArticleSummary.ParsedUri.DataPath, data);

							Parse(JBBSFeedProvider.GetString(data), progress);

							ArticleSummary.Title = ((JBBSArticleComment)Comments.First()).Title;
							ArticleSummary.LastModified = res.Content.Headers.LastModified;
						}

						ArticleSummary.CachedCommentCount = ArticleSummary.CommentCount = Comments.Count;
						await Summary.Save();
					}
					else
						res.EnsureSuccessStatusCode();
			}
		}

		void Parse(string content, IProgress<ProgressInfo> progress, Func<Uri, string, ArticleComment> commentParser = null)
		{
			var count = 0;
			var lines = content.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			var items = lines.AsParallel()
							 .AsOrdered()
							 .Select(_ => (commentParser ?? JBBSArticleComment.Parse)(ArticleSummary.ParsedUri.AbsoluteUri, _))
							 .Do(_ => progress?.Report(ProgressInfo.Download(Summary, count++, lines.Length)));

			if (Comments == null)
				Comments = items.ToList();
			else
				foreach (var i in items)
					Comments.Add(i);
		}
	}
}