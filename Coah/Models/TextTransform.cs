using System;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace Linearstar.Coah
{
	static class TextTransform
	{
		static IRazorEngineService textService;

		public static string ToText(Article article)
		{
			if (textService == null)
				textService = RazorEngineService.Create(new TemplateServiceConfiguration() { EncodedStringFactory = new RawStringFactory() });

			throw new NotImplementedException();
		}
	}
}
