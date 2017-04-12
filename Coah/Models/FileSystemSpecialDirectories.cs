using System;
using System.Composition;
using System.IO;

namespace Linearstar.Coah.Models
{
	[Export(typeof(ISpecialDirectories)), Shared]
	class FileSystemSpecialDirectories : ISpecialDirectories
	{
		static readonly string startupPath = AppDomain.CurrentDomain.BaseDirectory;

		public IDirectory Cache { get; } = new FileSystemDirectory(Path.Combine(startupPath, "Cache"), true);
	}
}
