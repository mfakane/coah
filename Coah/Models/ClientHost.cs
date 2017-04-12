using System.Composition;
using System.Composition.Hosting;
using System.Reflection;

namespace Linearstar.Coah.Models
{
	class ClientHost
	{
		public CompositionHost Container
		{
			get;
		}

		public Client Client
		{
			get;
			private set;
		}

		public ClientHost()
		{
			Container = new ContainerConfiguration()
				.WithAssembly(Assembly.GetExecutingAssembly())
				.WithAssembly(typeof(IClient).Assembly)
				.CreateContainer();
			Container.SatisfyImports(this);
			Client = (Client)Container.GetExport<IClient>();
			Client.Host = Container;
		}
	}
}
