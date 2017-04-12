using System;
using System.IO;
using System.Linq;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Linearstar.Coah
{
	public class Skin
	{
		readonly IRazorEngineService razor = RazorEngineService.Create(new TemplateServiceConfiguration
		{
			DisableTempFileLocking = true,
		});

		public string Name { get; }
		public string FullName { get; }

		public Skin(string path)
		{
			Name = Path.GetFileName(FullName = path);
		}

		public string GetHtml(SkinResource resource)
		{
			var typeName = resource.Article.GetType().Namespace.Split('.').Last();
			var files = Directory.GetFiles(FullName, $"*{typeName}.*cshtml");
			var file = files.FirstOrDefault() ?? Path.Combine(FullName, "Index.cshtml");

			resource.BaseUri = new Uri(FullName);

			return razor.RunCompile(File.ReadAllText(file), file, model: resource);
		}
	}
}
