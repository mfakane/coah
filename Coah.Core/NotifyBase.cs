using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Linearstar.Coah
{
	[DataContract]
	public class NotifyBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		Dictionary<string, object> container;

		protected void OnPropertyChanged([CallerMemberName] string name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		protected void OnPropertyChanged<T>(string propertyName, T oldValue, T newValue)
		{
			if ((!oldValue?.Equals(newValue) ?? false) ||
				(!newValue?.Equals(oldValue) ?? false))
				OnPropertyChanged(propertyName);
		}

		protected T GetValue<T>([CallerMemberName] string name = null) =>
			container?.ContainsKey(name) ?? false
				? (T)container[name]
				: default;

		protected void SetValue<T>(string name, T value)
		{
			if (container == null)
				container = new Dictionary<string, object>();

			OnPropertyChanged(name, GetValue<T>(name), container[name] = value);
		}

		protected void SetValue<T>(T value, [CallerMemberName] string name = null) =>
			SetValue(name, value);
	}
}
