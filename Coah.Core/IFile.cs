using System.IO;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public interface IFile
	{
		string Name
		{
			get;
		}

		string FullName
		{
			get;
		}

		Task<Stream> Open(FileAccessMode mode);
		Task Delete();
		Task<bool> Move(string newPath, bool overwriteExisting);
		Task<bool> Rename(string newName, bool overwriteExisting);
	}
}