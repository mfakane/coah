using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public static class DirectoryExtensions
	{
		public static async Task<byte[]> ReadAllBytes(this IDirectory directory, string name)
		{
			var file = await directory.GetFile(name).ConfigureAwait(false);

			return file == null ? null : await file.ReadAllBytes();
		}

		public static async Task<string[]> ReadAllLines(this IDirectory directory, string name)
		{
			var file = await directory.GetFile(name).ConfigureAwait(false);

			return file == null ? null : await file.ReadAllLines();
		}

		public static async Task<string[]> ReadAllLines(this IDirectory directory, string name, Encoding encoding)
		{
			var file = await directory.GetFile(name).ConfigureAwait(false);

			return file == null ? null : await file.ReadAllLines(encoding);
		}

		public static async Task<string> ReadAllText(this IDirectory directory, string name)
		{
			var file = await directory.GetFile(name).ConfigureAwait(false);

			return file == null ? null : await file.ReadAllText();
		}

		public static async Task<string> ReadAllText(this IDirectory directory, string name, Encoding encoding)
		{
			var file = await directory.GetFile(name).ConfigureAwait(false);

			return file == null ? null : await file.ReadAllText(encoding);
		}

		public static async Task<IEnumerable<string>> ReadLines(this IDirectory directory, string name)
		{
			var file = await directory.GetFile(name).ConfigureAwait(false);

			return file == null ? null : await file.ReadLines();
		}

		public static async Task<IEnumerable<string>> ReadLines(this IDirectory directory, string name, Encoding encoding)
		{
			var file = await directory.GetFile(name).ConfigureAwait(false);

			return file == null ? null : await file.ReadLines(encoding);
		}

		public static async Task WriteAllBytes(this IDirectory directory, string name, byte[] bytes) =>
			await (await directory.CreateFile(name, FileCollisionOption.Replace)).WriteAllBytes(bytes);
		public static async Task AppendAllBytes(this IDirectory directory, string name, byte[] bytes) =>
			await (await directory.CreateFile(name, FileCollisionOption.Open)).WriteAllBytes(bytes);

		public static async Task WriteAllLines(this IDirectory directory, string name, IEnumerable<string> contents) =>
			await (await directory.CreateFile(name, FileCollisionOption.Replace)).WriteAllLines(contents);
		public static async Task WriteAllLines(this IDirectory directory, string name, IEnumerable<string> contents, Encoding encoding) =>
			await (await directory.CreateFile(name, FileCollisionOption.Replace)).WriteAllLines(contents, encoding);
		public static async Task AppendAllLines(this IDirectory directory, string name, IEnumerable<string> contents) =>
			await (await directory.CreateFile(name, FileCollisionOption.Open)).WriteAllLines(contents);
		public static async Task AppendAllLines(this IDirectory directory, string name, IEnumerable<string> contents, Encoding encoding) =>
			await (await directory.CreateFile(name, FileCollisionOption.Open)).WriteAllLines(contents, encoding);

		public static async Task WriteAllText(this IDirectory directory, string name, string content) =>
			await (await directory.CreateFile(name, FileCollisionOption.Replace)).WriteAllText(content);
		public static async Task WriteAllText(this IDirectory directory, string name, string content, Encoding encoding) =>
			await (await directory.CreateFile(name, FileCollisionOption.Replace)).WriteAllText(content, encoding);
		public static async Task AppendAllText(this IDirectory directory, string name, string content) =>
			await (await directory.CreateFile(name, FileCollisionOption.Open)).WriteAllText(content);
		public static async Task AppendAllText(this IDirectory directory, string name, string content, Encoding encoding) =>
			await (await directory.CreateFile(name, FileCollisionOption.Open)).WriteAllText(content, encoding);
	}
}
