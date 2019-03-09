using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Linearstar.Coah
{
	[Export(typeof(IClientSettings))]
	class ClientSettings : IClientSettings
	{
		string SettingsPath { get; } = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".dat");

		public string StartupPath { get; } = AppDomain.CurrentDomain.BaseDirectory;
		public IEnumerable<Skin> Skins => Directory.EnumerateDirectories(Path.Combine(StartupPath, "Resources", "Skins")).Select(x => new Skin(x));
		public Skin CurrentSkin => Skins.First(x => x.Name == "Default");

		public bool UseSystemReadProxy { get; } = false;
		public bool UseSystemWriteProxy { get; } = false;
		public IWebProxy ReadProxy { get; } = null;
		public IWebProxy WriteProxy { get; } = null;

		public void Load()
		{

		}

		public void Save()
		{
		}
	}
}
