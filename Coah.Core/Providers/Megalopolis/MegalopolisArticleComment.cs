using System;
using System.Runtime.Serialization;

namespace Linearstar.Coah.Megalopolis
{
	[DataContract]
	public class MegalopolisArticleComment : ArticleComment
	{
		[DataMember]
		public int? Evaluation
		{
			get;
			set;
		}

		public static MegalopolisArticleComment Parse(Uri articleUri, int index, Comment line)
		{
			return new MegalopolisArticleComment
			{
				Uri = articleUri,
				Index = index,
				Sender = new Sender(line.Name, line.Mail, null),
				Body = line.Body,
				DateTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(line.DateTime).ToLocalTime(),
				Evaluation = line.Evaluation,
			};
		}
	}
}