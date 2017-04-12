using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	class FileSystemDirectory : IDirectory
	{
		public string FullName
		{
			get;
		}

		public string Name
		{
			get;
		}

		public FileSystemDirectory(string path, bool createIfNotExists = false)
		{
			Name = Path.GetFileName(FullName = path);

			if (createIfNotExists && !Directory.Exists(path))
				Directory.CreateDirectory(path);
		}

		public Task<IDirectory> CreateDirectory(string name, bool failIfExists = false)
		{
			var fullPath = Path.Combine(FullName, name);
			var dirInfo = new DirectoryInfo(Path.GetDirectoryName(fullPath));

			if (!dirInfo.Exists)
				dirInfo.Create();

			if (!Directory.Exists(fullPath))
				Directory.CreateDirectory(fullPath);
			else if (failIfExists)
				throw new IOException($"Directory {fullPath} already exists.");

			return Task.FromResult<IDirectory>(new FileSystemDirectory(fullPath));
		}

		public Task<IFile> CreateFile(string name, FileCollisionOption collisionOption)
		{
			var fullPath = Path.Combine(FullName, name);
			var dirInfo = new DirectoryInfo(Path.GetDirectoryName(fullPath));

			if (!dirInfo.Exists)
				dirInfo.Create();

			if (collisionOption == FileCollisionOption.Replace || !File.Exists(fullPath))
				File.Create(fullPath).Close();
			else if (collisionOption == FileCollisionOption.FailIfExists)
				throw new IOException($"File {fullPath} already exists.");

			return Task.FromResult<IFile>(new FileSystemFile(fullPath));
		}

		public Task Delete()
		{
			Directory.Delete(FullName, true);

			return Task.CompletedTask;
		}

		public Task<bool> DeleteFile(string name)
		{
			var fullPath = Path.Combine(FullName, name);

			if (!File.Exists(fullPath))
				return Task.FromResult(false);

			File.Delete(fullPath);

			return Task.FromResult(true);
		}

		public Task<IDirectory> GetDirectory(string name)
		{
			var fullPath = Path.Combine(FullName, name);

			return Task.FromResult<IDirectory>(Directory.Exists(fullPath) ? new FileSystemDirectory(fullPath) : null);
		}

		public Task<IReadOnlyList<IDirectory>> GetDirectories(string searchPattern) =>
			Task.FromResult<IReadOnlyList<IDirectory>>(Directory.EnumerateDirectories(FullName, searchPattern ?? "*").Select(_ => new FileSystemDirectory(_)).ToArray());

		public Task<IFile> GetFile(string name)
		{
			var fullPath = Path.Combine(FullName, name);

			return Task.FromResult<IFile>(File.Exists(fullPath) ? new FileSystemFile(fullPath) : null);
		}

		public Task<IReadOnlyList<IFile>> GetFiles(string searchPattern) =>
			Task.FromResult<IReadOnlyList<IFile>>(Directory.EnumerateFiles(FullName, searchPattern ?? "*").Select(_ => new FileSystemFile(_)).ToArray());
	}
}
