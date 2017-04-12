using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	[DataContract]
	public abstract class ArticleSummary : NotifyBase
	{
		public FeedProvider Provider => FeedSummary.Provider;

		[DataMember]
		public virtual int? Index
		{
			get => GetValue<int?>();
			set => SetValue(value);
		}

		[DataMember]
		public virtual string Title
		{
			get => GetValue<string>();
			set => SetValue(value);
		}

		[DataMember]
		public virtual Uri Uri
		{
			get => GetValue<Uri>();
			set => SetValue(value);
		}

		public virtual Uri AbsoluteUri => Uri;

		[DataMember]
		public virtual string Author
		{
			get => GetValue<string>();
			set => SetValue(value);
		}

		[DataMember]
		public virtual DateTimeOffset DateTime
		{
			get => GetValue<DateTimeOffset>();
			set => SetValue(value);
		}

		[DataMember]
		public virtual DateTimeOffset? LastModified
		{
			get => GetValue<DateTimeOffset?>();
			set => SetValue(value);
		}

		[DataMember]
		public virtual DateTimeOffset? LastAccess
		{
			get => GetValue<DateTimeOffset?>();
			set => SetValue(value);
		}

		[DataMember]
		public virtual int CommentCount
		{
			get => GetValue<int>();
			set
			{
				SetValue(value);
				OnPropertyChanged(nameof(NewCommentCount));
				OnPropertyChanged(nameof(HasReadAll));
				OnPropertyChanged(nameof(HasNewComment));
			}
		}

		[DataMember]
		public virtual int? CachedCommentCount
		{
			get => GetValue<int?>();
			set
			{
				SetValue(value);
				OnPropertyChanged(nameof(IsCached));
			}
		}

		[DataMember]
		public virtual int? ReadCommentCount
		{
			get => GetValue<int?>();
			set
			{
				SetValue(value);
				OnPropertyChanged(nameof(NewCommentCount));
				OnPropertyChanged(nameof(HasReadAll));
				OnPropertyChanged(nameof(HasNewComment));
			}
		}

		public int? NewCommentCount => ReadCommentCount == 0 ? null : CommentCount - ReadCommentCount;
		public bool HasReadAll => NewCommentCount == 0;
		public bool HasNewComment => NewCommentCount > 0;
		public bool IsCached => CachedCommentCount > 0;

		[DataMember]
		public virtual string[] Tags
		{
			get => GetValue<string[]>();
			set => SetValue(value);
		}

		public FeedSummary FeedSummary
		{
			get;
			set;
		}

		[DataMember]
		public bool IsNew
		{
			get => GetValue<bool>();
			set => SetValue(value);
		}

		[DataMember]
		public virtual bool IsLost
		{
			get => GetValue<bool>();
			set => SetValue(value);
		}

		public ArticleSummary(FeedSummary feedSummary, Uri uri)
		{
			FeedSummary = feedSummary;
			Uri = uri;
		}

		public void SetReadAll()
		{
			LastAccess = DateTimeOffset.Now;
			ReadCommentCount = CachedCommentCount;
		}

		public abstract Task Save();
		public abstract ValueTask<Article> GetArticle(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);

		public virtual Task DeleteCache()
		{
			LastModified = null;
			CachedCommentCount = null;
			ReadCommentCount = null;
			IsLost = false;

			return Task.Delay(0);
		}
	}
}
