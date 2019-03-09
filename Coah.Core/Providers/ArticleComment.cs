using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Linearstar.Coah
{
	[DataContract]
	public class ArticleComment
	{
		public static readonly Regex UrlPattern = new Regex(@"h?(?<ttp>ttp(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?)");
		public static readonly Regex LinkTagPattern = new Regex(@"</?a(\s.+?)?>", RegexOptions.IgnoreCase);
		public static readonly Regex AnchorPattern = new Regex(@"((&gt;)+|＞+)(?<num>[0-9]+(-[0-9]+(,[0-9]+(-[0-9]+)?)*)?)");

		string body;
		string bodyHtml;
		HashSet<int> references;

		[DataMember]
		public virtual Uri Uri
		{
			get;
			set;
		}

		[DataMember]
		public virtual int Index
		{
			get;
			set;
		}

		[DataMember]
		public virtual Sender Sender
		{
			get;
			set;
		}

		[DataMember]
		public virtual DateTimeOffset DateTime
		{
			get;
			set;
		}

		[DataMember]
		public virtual string Body
		{
			get => body;
			set
			{
				body = value;
				bodyHtml = null;
				references = null;
			}
		}

		public virtual HashSet<int> GetReferences() =>
			references ?? (references = new HashSet<int>(AnchorPattern.Matches(Body).Cast<Match>().SelectMany(m => m.Groups["num"].Value.Split(',')).SelectMany(x =>
			{
				if (x.Contains("-"))
				{
					var sl = x.Split('-');

					return Enumerable.Range(int.Parse(sl[0]), int.Parse(sl[1]) - int.Parse(sl[0]) + 1);
				}
				else
					return new[] { int.Parse(x) };
			})));

		public virtual string GetBodyHtml()
		{
			if (bodyHtml != null)
				return bodyHtml;

			bodyHtml = Body;
			bodyHtml = LinkTagPattern.Replace(bodyHtml, "");
			bodyHtml = UrlPattern.Replace(bodyHtml, "<a href=\"h${ttp}\">$0</a>");
			bodyHtml = AnchorPattern.Replace(bodyHtml, "<a href=\"#res-${num}\">$0</a>");

			return bodyHtml;
		}
	}
}
