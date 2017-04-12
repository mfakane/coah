using System;
using System.Net;
using System.Runtime.Serialization;

namespace Linearstar.Coah.Megalith
{
	[DataContract]
	public class MegalithArticleComment : ArticleComment
	{
		[DataMember]
		public int? Evaluation
		{
			get;
			set;
		}

		public static MegalithArticleComment Parse(Uri articleUri, int index, string line)
		{
			var sl = line.Split(new[] { "<>" }, StringSplitOptions.None);

			return new MegalithArticleComment
			{
				Uri = new Uri(articleUri, "#comment" + index),
				Index = index,
				Sender = new Sender(WebUtility.HtmlDecode(sl[1]), WebUtility.HtmlDecode(sl[2])),
				Body = sl[0],
				DateTime = DateTimeOffset.Parse(sl[3]),
				Evaluation = int.Parse(WebUtility.HtmlDecode(sl[4])),
			};
		}
	}
}