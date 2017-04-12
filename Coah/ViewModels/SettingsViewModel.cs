using Caliburn.Micro;

namespace Linearstar.Coah.ViewModels
{
	class SettingsViewModel : Screen
	{
		readonly IClientSettings settings;

		public SettingsViewModel(IClientSettings settings)
		{
			this.settings = settings;
		}
	}
}
