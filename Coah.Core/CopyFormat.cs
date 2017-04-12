using System.Reflection;

namespace Linearstar.Coah
{
	public class CopyFormat
	{
		public string FormatString
		{
			get;
			set;
		}

		public string Format(CopyFormatResource res)
		{
			var str = FormatString;

			foreach (var i in res.GetType().GetRuntimeProperties())
				str = str.Replace("{" + i.Name + "}", (string)i.GetValue(res));

			return str;
		}
	}
}
