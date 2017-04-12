using System.Runtime.Serialization;

namespace Linearstar.Coah.Megalopolis
{
	[DataContract]
	public class ErrorResponse
	{
		[DataMember(Name = "error")]
		public string Error { get; set; }
	}

	[DataContract]
	public class ErrorCollection
	{
		[DataMember(Name = "errors")]
		public string[] Errors { get; set; }
	}

	[DataContract]
	public class Entry
	{
		[DataMember(Name = "id")]
		public long Id { get; set; }
		[DataMember(Name = "subject")]
		public int Subject { get; set; }
		[DataMember(Name = "title")]
		public string Title { get; set; }
		[DataMember(Name = "name")]
		public string Name { get; set; }
		[DataMember(Name = "summary")]
		public string Summary { get; set; }
		[DataMember(Name = "link")]
		public string Link { get; set; }
		[DataMember(Name = "mail")]
		public string Mail { get; set; }
		[DataMember(Name = "dateTime")]
		public long DateTime { get; set; }
		[DataMember(Name = "lastUpdate")]
		public long LastUpdate { get; set; }
		[DataMember(Name = "pageCount")]
		public int? PageCount { get; set; }
		[DataMember(Name = "size")]
		public double? SizeInKB { get; set; }
		[DataMember(Name = "points")]
		public int? Points { get; set; }
		[DataMember(Name = "responseCount")]
		public int? ResponseCount { get; set; }
		[DataMember(Name = "commentCount")]
		public int? CommentCount { get; set; }
		[DataMember(Name = "evaluationCount")]
		public int? EvaluationCount { get; set; }
		[DataMember(Name = "readCount")]
		public int? ReadCount { get; set; }
		[DataMember(Name = "tags")]
		public string[] Tags { get; set; }
	}

	[DataContract]
	public class SubjectResponse
	{
		[DataMember(Name = "entries")]
		public Entry[] Entries { get; set; }
		[DataMember(Name = "subject")]
		public int Subject { get; set; }
		[DataMember(Name = "subjectCount")]
		public int SubjectCount { get; set; }
	}

	[DataContract]
	public class EntryList
	{
		[DataMember(Name = "entries")]
		public Entry[] Entries { get; set; }
		[DataMember(Name = "page")]
		public int Page { get; set; }
		[DataMember(Name = "pageCount")]
		public int PageCount { get; set; }
	}

	[DataContract]
	public class Comment
	{
		[DataMember(Name = "id")]
		public long Id { get; set; }
		[DataMember(Name = "name")]
		public string Name { get; set; }
		[DataMember(Name = "mail")]
		public string Mail { get; set; }
		[DataMember(Name = "body")]
		public string Body { get; set; }
		[DataMember(Name = "dateTime")]
		public long DateTime { get; set; }
		[DataMember(Name = "evaluation")]
		public int? Evaluation { get; set; }
	}

	[DataContract]
	public class ConfigurationResponse
	{
		[DataMember(Name = "system")]
		public ConfigurationSystem System { get; set; }
		[DataMember(Name = "configuration")]
		public ConfigurationBody Configuration { get; set; }

		[DataContract]
		public class ConfigurationSystem
		{
			[DataMember(Name = "megalopolis")]
			public int Megalopolis { get; set; }
			[DataMember(Name = "megalith")]
			public int Megalith { get; set; }
			[DataMember(Name = "php")]
			public string PHP { get; set; }
		}

		[DataContract]
		public class ConfigurationBody
		{
			[DataMember(Name = "title")]
			public string Title { get; set; }
			[DataMember(Name = "bbq")]
			public string BBQ { get; set; }
			[DataMember(Name = "pointEnabled")]
			public bool PointEnabled { get; set; }
			[DataMember(Name = "pointMap")]
			public int[] PointMap { get; set; }
			[DataMember(Name = "commentEnabled")]
			public bool CommentEnabled { get; set; }
			[DataMember(Name = "commentPointEnabled")]
			public bool CommentPointEnabled { get; set; }
			[DataMember(Name = "commentPointMap")]
			public int[] CommentPointMap { get; set; }
			[DataMember(Name = "adminOnly")]
			public bool AdminOnly { get; set; }
			[DataMember(Name = "defaultName")]
			public string DefaultName { get; set; }
			[DataMember(Name = "requireNameOnEntry")]
			public bool RequireNameOnEntry { get; set; }
			[DataMember(Name = "requireNameOnComment")]
			public bool RequireNameOnComment { get; set; }
			[DataMember(Name = "requirePasswordOnEntry")]
			public bool RequirePasswordOnEntry { get; set; }
			[DataMember(Name = "requirePasswordOnComment")]
			public bool RequirePasswordOnComment { get; set; }
			[DataMember(Name = "requirePostPassword")]
			public bool RequirePostPassword { get; set; }
			[DataMember(Name = "maxTags")]
			public int MaxTags { get; set; }
			[DataMember(Name = "foregroundEnabled")]
			public bool ForegroundEnabled { get; set; }
			[DataMember(Name = "backgroundEnabled")]
			public bool BackgroundEnabled { get; set; }
			[DataMember(Name = "backgroundImageEnabled")]
			public bool BackgroundImageEnabled { get; set; }
			[DataMember(Name = "borderEnabled")]
			public bool BorderEnabled { get; set; }
			[DataMember(Name = "subjectSplitting")]
			public int SubjectSplitting { get; set; }
			[DataMember(Name = "rateType")]
			public string RateType { get; set; }
			[DataMember(Name = "updatePeriod")]
			public int UpdatePeriod { get; set; }
			[DataMember(Name = "minBodySize")]
			public int MinBodySize { get; set; }
			[DataMember(Name = "maxBodySize")]
			public int MaxBodySize { get; set; }
			[DataMember(Name = "useSummary")]
			public bool UseSummary { get; set; }
			[DataMember(Name = "maxSummaryLines")]
			public int MaxSummaryLines { get; set; }
			[DataMember(Name = "maxSummarySize")]
			public int MaxSummarySize { get; set; }
		}
	}

	[DataContract]
	public class ThreadResponse
	{
		[DataMember(Name = "entry")]
		public Entry Entry { get; set; }
		[DataMember(Name = "tags")]
		public string[] Tags { get; set; }
		[DataMember(Name = "body")]
		public string Body { get; set; }
		[DataMember(Name = "formattedBody")]
		public string[] FormattedBody { get; set; }
		[DataMember(Name = "afterword")]
		public string Afterword { get; set; }
		[DataMember(Name = "formattedAfterword")]
		public string FormattedAfterword { get; set; }
		[DataMember(Name = "convertLineBreak")]
		public bool ConvertLineBreak { get; set; }
		[DataMember(Name = "foreground")]
		public string Foreground { get; set; }
		[DataMember(Name = "background")]
		public string Background { get; set; }
		[DataMember(Name = "backgroundImage")]
		public string BackgroundImage { get; set; }
		[DataMember(Name = "border")]
		public string Border { get; set; }
		[DataMember(Name = "writingMode")]
		public WritingMode WritingMode { get; set; }
		[DataMember(Name = "nonCommentEvaluation")]
		public int NonCommentEvaluation { get; set; }
		[DataMember(Name = "comments")]
		public Comment[] Comments { get; set; }
	}

	public enum WritingMode
	{
		Default,
		Horizontal,
		Vertical,
	}

	[DataContract]
	public class EvaluationResponse
	{
		[DataMember(Name = "id")]
		public long Id { get; set; }
		[DataMember(Name = "dateTime")]
		public long DateTime { get; set; }
		[DataMember(Name = "point")]
		public int Point { get; set; }
	}

	[DataContract]
	public class CommentResult : Comment
	{
		[DataMember(Name = "num")]
		public int Number { get; set; }
		[DataMember(Name = "formattedBody")]
		public string FormattedBody { get; set; }
		[DataMember(Name = "deleteAction")]
		public string DeleteAction { get; set; }
	}
}
