using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Linearstar.Coah
{
	public interface IViewer : INotifyPropertyChanged
	{
		ObservableCollection<IPage> Pages { get; }
		IPage CurrentPage { get; set; }
		IClient Client { get; }
	}
}