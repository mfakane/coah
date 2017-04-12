using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Linearstar.Coah
{
	static class StringUtil
	{
		const string IsHiragana = nameof(IsHiragana);
		const string IsKatakana = nameof(IsKatakana);
		const string IsHalfwidthKatakana = nameof(IsHalfwidthKatakana);
		const string IsCJKUnifiedIdeographs = nameof(IsCJKUnifiedIdeographs);
		static readonly Regex charClassRegex = new Regex($@"(?<{IsHiragana}>\p{{IsHiragana}})|(?<{IsKatakana}>\p{{IsKatakana}})|(?<{IsHalfwidthKatakana}>[\uFF61-\uFF9F])|(?<{IsCJKUnifiedIdeographs}>\p{{IsCJKUnifiedIdeographs}})");

		public static IEnumerable<string> GetWords(string str)
		{
			var sb = new StringBuilder();
			var prev = CharClass.None;

			for (var i = 0; i < str.Length; i++)
			{
				var cc = GetCharClass(str[i]);

				if (cc != prev && i > 0)
				{
					yield return sb.ToString();

					sb.Clear();
				}

				sb.Append(str[i]);
				prev = cc;
			}

			if (prev != CharClass.None)
				yield return sb.ToString();
		}

		public static HashSet<string> IntersectWords(string str, IEnumerable<string> with)
		{
			var rt = new HashSet<string>(GetWords(str));

			rt.IntersectWith(with);

			return rt;
		}

		static CharClass GetCharClass(char c)
		{
			if (char.IsDigit(c))
				return CharClass.Digit;
			else if (char.IsWhiteSpace(c))
				return CharClass.WhiteSpace;
			else if (char.IsPunctuation(c) || char.IsSymbol(c))
				return CharClass.Symbol;
			else
			{
				var m = charClassRegex.Match(c.ToString());

				if (m.Groups[IsHiragana].Success)
					return CharClass.Hiragana;
				else if (m.Groups[IsKatakana].Success)
					return CharClass.Katakana;
				else if (m.Groups[IsHalfwidthKatakana].Success)
					return CharClass.HalfwidthKatakana;
				else if (m.Groups[IsCJKUnifiedIdeographs].Success)
					return CharClass.CJKUnifiedIdeographs;
				else if (char.IsLetter(c))
					return CharClass.Letter;
				else
					return CharClass.Other;
			}
		}

		enum CharClass
		{
			None,
			Letter,
			Digit,
			WhiteSpace,
			Symbol,
			Hiragana,
			Katakana,
			HalfwidthKatakana,
			CJKUnifiedIdeographs,
			Other,
		}
	}
}
