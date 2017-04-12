using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Linearstar.Coah.JBBS
{
	[DataContract]
	public class JBBSArticleComment : ArticleComment
	{
		static readonly Regex removeDayOfWeek = new Regex(@"\(.+\)");
		static readonly Regex htmlPattern = new Regex(@"<dt><a name=""(?<index>[0-9]+)"">[0-9]+</a> ：(?<name>.+)：(?<datetime>.+)<dd>(?<body>.*)");
		static readonly Regex htmlNamePattern = new Regex(@"<b>(?<name>.+)</b>");
		static readonly Regex htmlMailPattern = new Regex(@"<a href=""mailto:(?<mail>.+?)""");

		[DataMember]
		public string Title
		{
			get;
			set;
		}

		public override string ToString() =>
			$"{Index}<>{WebUtility.HtmlEncode(Sender.Name)}<>{WebUtility.HtmlEncode(Sender.Mail)}<>{DateTime:yyyy/MM/dd (ddd) HH:mm:ss}<>{Body}<>{WebUtility.HtmlEncode(Title)}<>{Sender.Identifier}";

		public static JBBSArticleComment Parse(Uri articleUri, string line)
		{
			var sl = line.Split(new[] { "<>" }, StringSplitOptions.None).Concat(Enumerable.Repeat("壊れています", 7)).Take(7).ToArray();

			return new JBBSArticleComment
			{
				Uri = new Uri(articleUri, sl[0]),
				Index = int.Parse(sl[0]),
				Sender = new Sender(WebUtility.HtmlDecode(sl[1]), WebUtility.HtmlDecode(sl[2]), sl[6]),
				Body = sl[4],
				DateTime = ParseDateTime(sl[3]),
				Title = WebUtility.HtmlDecode(sl[5]),
			};
		}

		public static JBBSArticleComment ParseHtml(Uri articleUri, string line)
		{
			var m = htmlPattern.Match(line);
			var name = htmlNamePattern.Match(m.Groups["name"].Value);
			var mail = htmlMailPattern.Match(m.Groups["name"].Value);
			var dateTimeAndId = m.Groups["datetime"].Value.Split(new[] { " ID:" }, 2, StringSplitOptions.None);

			return new JBBSArticleComment
			{
				Uri = new Uri(articleUri, m.Groups["index"].Value),
				Index = int.Parse(m.Groups["index"].Value),
				Sender = new Sender(WebUtility.HtmlDecode(name.Groups["name"].Value), mail.Success ? WebUtility.HtmlDecode(mail.Groups["mail"].Value) : "", dateTimeAndId[1]),
				Body = m.Groups["body"].Value.Replace("href=\"#", "href=\"" + articleUri.AbsolutePath),
				DateTime = ParseDateTime(dateTimeAndId[0]),
			};
		}

		static DateTimeOffset ParseDateTime(string dateTimeString)
		{
			DateTimeOffset.TryParseExact(removeDayOfWeek.Replace(dateTimeString, ""), new[]
			{
				"yyyy/MM/dd HH:mm:ss.ff",
				"yyyy/MM/dd HH:mm:ss",
				"yyyy/MM/dd HH:mm",
				"yy/MM/dd HH:mm:ss",
				"yy/MM/dd HH:mm",
			}, new CultureInfo("ja-JP"), DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out var rt);

			return rt;
		}
	}
}