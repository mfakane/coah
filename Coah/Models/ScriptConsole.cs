using System.Collections.Generic;

namespace Linearstar.Coah.Models
{
	class ScriptConsole : NotifyBase
	{
		public IList<ConsoleMessage> Messages
		{
			get;
			private set;
		}
	}

	enum ConsoleMessageSectionKind
	{
		Default,
	}

	class ConsoleMessageSection
	{
		public string Text
		{
			get;
			set;
		}

		public ConsoleMessageSectionKind Kind
		{
			get;
			set;
		}
	}

	class ConsoleMessage
	{
		public IList<ConsoleMessageSection> Sections
		{
			get;
			set;
		}
	}

	class PromptMessage
	{

	}
}
