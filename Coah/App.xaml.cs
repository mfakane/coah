using CefSharp;
using Linearstar.Coah.Models;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Linearstar.Coah
{
	partial class App : Application
	{
		static readonly string dllPath;

		public static new App Current => (App)Application.Current;
		internal Client Client { get; } = new ClientHost().Client;

		static App()
		{
			dllPath = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
			{
				var path = Path.Combine(dllPath, e.Name.Substring(0, e.Name.IndexOf(',')) + ".dll");

				return File.Exists(path) ? Assembly.LoadFrom(path) : null;
			};
		}

		void Application_Startup(object sender, StartupEventArgs e)
		{
			var settings = new CefSettings
			{
				CefCommandLineArgs =
				{
					{ "renderer-process-limit", "1" },
					{ "disable-plugins-discovery", "1" },
				},
				//BrowserSubprocessPath = Path.Combine(dllPath, "CefSharp.BrowserSubprocess.exe"),
				LogSeverity = LogSeverity.Disable,
			};

			CefSharpSettings.WcfEnabled = false;
			Cef.Initialize(settings);
			Client.Settings.Load();
		}

		void Application_Exit(object sender, ExitEventArgs e)
		{
			Client.Settings.Save();
			Cef.Shutdown();
		}
	}
}
