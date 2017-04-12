using System;
using System.Collections;
using System.Linq;

namespace Linearstar.Coah.Views
{
	class WebScriptFormatProvider : IFormatProvider
	{
		static readonly WebScriptFormatProvider Instance = new WebScriptFormatProvider();
		readonly WebScriptCustomFormatter customFormatter = new WebScriptCustomFormatter();

		public static string Format(FormattableString formattable) =>
			formattable.ToString(Instance);

		public object GetFormat(Type formatType)
		{
			if (formatType == typeof(ICustomFormatter))
				return customFormatter;
			else
				return null;
		}

		class WebScriptCustomFormatter : ICustomFormatter
		{
			public string Format(string format, object arg, IFormatProvider formatProvider)
			{
				if (arg == null)
					return "null";
				else if (arg is bool)
					return ((bool)arg).ToString().ToLower();
				else if (arg is string)
					return "\"" + ((string)arg).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
				else if (arg is IEnumerable)
					return "[" + string.Join(", ", ((IEnumerable)arg).Cast<object>().Select(_ => Format(null, _, null))) + "]";
				else
					return arg.ToString();
			}
		}
	}

	class WebScriptIdentifier
	{
		public string Name { get; }

		public WebScriptIdentifier(string name) =>
			Name = name;

		public override string ToString() =>
			Name;
	}
}
