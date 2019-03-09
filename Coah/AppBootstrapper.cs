using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Linearstar.Coah.ViewModels;
using Linearstar.Coah.Views;

namespace Linearstar.Coah
{
	class AppBootstrapper : BootstrapperBase
	{
		public AppBootstrapper()
		{
			Initialize();
		}

		protected override void Configure()
		{
			var trigger = Parser.CreateTrigger;

			Parser.CreateTrigger = (target, triggerText) =>
			{
				if (triggerText == null)
					return ConventionManager.GetElementConvention(target.GetType()).CreateTrigger();

				var parameters = new Queue<string>(triggerText.Trim().Trim('[', ']').Split(default(char[]), StringSplitOptions.RemoveEmptyEntries));
				var triggerName = parameters.Dequeue();

				if (triggerName != "Key") return trigger(target, triggerText);

				var keys = parameters.Dequeue().Replace("Ctrl+", "Control+").Replace("Win+", "Windows+").Split('+');
				var key = Enum.TryParse<Key>(keys.Last(), true, out var k) ? k : Key.None;
				var modifiers = keys.Length > 1 ? keys.Take(keys.Length - 1).Select(x => Enum.TryParse<ModifierKeys>(x, true, out var mk) ? mk : ModifierKeys.None).Aggregate((x, y) => x | y) : ModifierKeys.None;

				return new KeyTrigger
				{
					Key = key,
					Modifiers = modifiers,
					ActiveOnFocus = parameters.Any() && parameters.Dequeue() == "ActiveOnFocus",
				};
			};
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			var vm = IoC.Get<ClientViewModel>();

			((IActivate)vm).Activate();
		}
	}
}
