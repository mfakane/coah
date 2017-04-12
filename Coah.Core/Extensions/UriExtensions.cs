using System;
using System.Net;
using System.Text;

namespace Linearstar.Coah
{
	static class UriExtensions
	{
		public static string Query(this Uri uri, string key, Encoding encoding = null)
		{
			if (string.IsNullOrEmpty(uri.Query) || uri.Query[0] != '?')
				return null;

			if (encoding == null)
				key = Uri.EscapeDataString(key);
			else
			{
				var bytes = encoding.GetBytes(key);

				bytes = WebUtility.UrlEncodeToBytes(bytes, 0, bytes.Length);
				key = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			}

			var begin = (uri.Query.StartsWith("?" + key + "=") ? 0 : uri.Query.IndexOf("&" + key + "=")) + key.Length + 2;
			var end = uri.Query.IndexOf('&', begin);
			var rt = end == -1 ? uri.Query.Substring(begin) : uri.Query.Substring(begin, end - begin);

			if (encoding == null)
				return Uri.UnescapeDataString(rt);
			else
			{
				var bytes = Encoding.UTF8.GetBytes(rt);

				bytes = WebUtility.UrlDecodeToBytes(bytes, 0, bytes.Length);

				return encoding.GetString(bytes, 0, bytes.Length);
			}
		}
	}
}
