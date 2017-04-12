using Caliburn.Micro;

namespace Linearstar.Coah.ViewModels
{
	interface IPageViewModel : IHaveDisplayName
	{
		IPage Model
		{
			get;
		}

		ProgressState ProgressState
		{
			get;
		}
	}
}
