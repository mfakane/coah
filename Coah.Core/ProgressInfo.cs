using System;
using System.Net.Http;

namespace Linearstar.Coah
{
	public class ProgressInfo
	{
		public string Message
		{
			get;
		}

		public bool IsIndeterminate
		{
			get;
		}

		public int Value
		{
			get;
		}

		public int Maximum
		{
			get;
		}

		public bool HasCompleted
		{
			get;
			set;
		}

		public bool HasProgressValue => IsIndeterminate || Value != 0 && Maximum != 0;

		public Exception Exception
		{
			get;
			set;
		}

		public bool IsHttpRequestException => Exception is HttpRequestException;

		public ProgressInfo(string message)
		{
			Message = message;
			HasCompleted = true;
		}

		public ProgressInfo(string message, Exception exception)
			: this(message)
		{
			Exception = exception;
		}

		public ProgressInfo(string message, int value, int max)
		{
			Message = message;
			Value = value;
			Maximum = max;
		}

		public ProgressInfo(string message, bool isIndeterminate)
		{
			Message = message;
			IsIndeterminate = isIndeterminate;
		}

		public static ProgressInfo Canceled(Location summary) => new ProgressInfo(summary.Name + " の読み込みを中止しました");
		public static ProgressInfo Canceled(FeedSummary summary) => new ProgressInfo(summary.Name + " の読み込みを中止しました");
		public static ProgressInfo Canceled(ArticleSummary summary) => new ProgressInfo(summary.Title + " の読み込みを中止しました");

		public static ProgressInfo Error(Location summary, Exception ex) => new ProgressInfo(summary.Name + " の読み込みでエラーが発生しました", ex);
		public static ProgressInfo Error(FeedSummary summary, Exception ex) => new ProgressInfo(summary.Name + " の読み込みでエラーが発生しました", ex);
		public static ProgressInfo Error(ArticleSummary summary, Exception ex) => new ProgressInfo(summary.Title + " の読み込みでエラーが発生しました", ex);

		public static ProgressInfo Download(Location summary) => new ProgressInfo(summary.Name + " を読み込んでいます...", true);
		public static ProgressInfo Download(FeedSummary summary) => new ProgressInfo(summary.Name + " を読み込んでいます...", true);
		public static ProgressInfo Download(FeedSummary summary, int value, int max) => new ProgressInfo(summary.Name + " を読み込んでいます...", value, max);
		public static ProgressInfo Download(ArticleSummary summary) => new ProgressInfo(summary.Title + " を読み込んでいます...", true);
		public static ProgressInfo Download(ArticleSummary summary, int value, int max) => new ProgressInfo(summary.Title + " を読み込んでいます...", value, max);

		public static ProgressInfo Done(Location summary) => new ProgressInfo(summary.Name + " を読み込みました");
		public static ProgressInfo Done(FeedSummary summary) => new ProgressInfo(summary.Name + " を読み込みました");
		public static ProgressInfo Done(ArticleSummary summary, int newCount) => new ProgressInfo($"{summary.Title} を読み込みました (新着 {newCount} 件)");
	}
}
