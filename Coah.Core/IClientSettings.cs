using System.Net;

namespace Linearstar.Coah
{
	public interface IClientSettings
	{
		bool UseSystemReadProxy
		{
			get;
		}

		bool UseSystemWriteProxy
		{
			get;
		}

		IWebProxy ReadProxy
		{
			get;
		}

		IWebProxy WriteProxy
		{
			get;
		}

		void Load();
		void Save();
	}
}
