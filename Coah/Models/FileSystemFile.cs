using System.IO;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	class FileSystemFile : IFile
	{
		public string Name
		{
			get;
			private set;
		}

		public string FullName
		{
			get;
			private set;
		}

		public FileSystemFile(string path) =>
			Name = Path.GetFileName(FullName = path);

		public Task Delete()
		{
			File.Delete(FullName);

			return Task.CompletedTask;
		}

		public Task<bool> Move(string newPath, bool overwriteExisting)
		{
			if (!overwriteExisting && File.Exists(newPath))
				return Task.FromResult(false);

			File.Move(FullName, newPath);
			Name = Path.GetFileName(FullName = newPath);

			return Task.FromResult(true);
		}

		public Task<Stream> Open(FileAccessMode mode) =>
			Task.FromResult<Stream>(File.Open(FullName, mode == FileAccessMode.Read ? FileMode.Open : FileMode.Append, mode == FileAccessMode.Read ? FileAccess.Read : FileAccess.Write, FileShare.Read));

		public Task<bool> Rename(string newName, bool overwriteExisting)
		{
			var newPath = Path.Combine(Path.GetDirectoryName(FullName), newName);

			if (!overwriteExisting && File.Exists(newPath))
				return Task.FromResult(false);

			File.Move(FullName, newPath);
			Name = Path.GetFileName(FullName = newPath);

			return Task.FromResult(true);
		}
	}
}
