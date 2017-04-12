using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public static class FileExtension
	{
		public static async Task<byte[]> ReadAllBytes(this IFile file)
		{
			using (var fs = await file.Open(FileAccessMode.Read).ConfigureAwait(false))
			{
				var data = new byte[fs.Length];
				var length = fs.Read(data, 0, data.Length);

				return data;
			}
		}

		public static Task<string> ReadAllText(this IFile file) => ReadAllText(file, new UTF8Encoding(false));
		public static async Task<string> ReadAllText(this IFile file, Encoding encoding)
		{
			using (var sr = new StreamReader(await file.Open(FileAccessMode.Read).ConfigureAwait(false), encoding))
				return sr.ReadToEnd();
		}

		public static Task<string[]> ReadAllLines(this IFile file) => ReadAllLines(file, new UTF8Encoding(false));
		public static async Task<string[]> ReadAllLines(this IFile file, Encoding encoding)
		{
			using (var sr = new StreamReader(await file.Open(FileAccessMode.Read).ConfigureAwait(false), encoding))
				return ReadLinesCore(sr).ToArray();
		}

		public static Task<IEnumerable<string>> ReadLines(this IFile file) => ReadLines(file, new UTF8Encoding(false));
		public static async Task<IEnumerable<string>> ReadLines(this IFile file, Encoding encoding)
		{
			using (var sr = new StreamReader(await file.Open(FileAccessMode.Read).ConfigureAwait(false), encoding))
				return ReadLinesCore(sr);
		}

		static IEnumerable<string> ReadLinesCore(StreamReader sr)
		{
			string rt;

			while ((rt = sr.ReadLine()) != null)
				yield return rt;
		}

		public static async Task WriteAllBytes(this IFile file, byte[] bytes)
		{
			using (var fs = await file.Open(FileAccessMode.Write).ConfigureAwait(false))
				fs.Write(bytes, 0, bytes.Length);
		}

		public static Task WriteAllLines(this IFile file, IEnumerable<string> contents) => WriteAllLines(file, contents, new UTF8Encoding(false));
		public static async Task WriteAllLines(this IFile file, IEnumerable<string> contents, Encoding encoding)
		{
			using (var sw = new StreamWriter(await file.Open(FileAccessMode.Write).ConfigureAwait(false), encoding))
				foreach (var i in contents)
					sw.WriteLine(i);
		}

		public static Task WriteAllText(this IFile file, string content) => WriteAllText(file, content, new UTF8Encoding(false));
		public static async Task WriteAllText(this IFile file, string content, Encoding encoding)
		{
			using (var sw = new StreamWriter(await file.Open(FileAccessMode.Write).ConfigureAwait(false), encoding))
				sw.Write(content);
		}
	}
}
