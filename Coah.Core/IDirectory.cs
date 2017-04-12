using System.Collections.Generic;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public interface IDirectory
	{
		string Name
		{
			get;
		}

		string FullName
		{
			get;
		}

		Task<IFile> GetFile(string name);
		Task<IReadOnlyList<IFile>> GetFiles(string searchPattern = null);
		Task<IFile> CreateFile(string name, FileCollisionOption collisionOption);
		Task<IDirectory> GetDirectory(string name);
		Task<IReadOnlyList<IDirectory>> GetDirectories(string searchPattern = null);
		Task<IDirectory> CreateDirectory(string name, bool failIfExists = false);
		Task Delete();
		Task<bool> DeleteFile(string name);
	}
}
