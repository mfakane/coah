using System;
using System.Globalization;
using System.Windows.Data;

namespace Linearstar.Coah.Views
{
	class ValueTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value?.GetType();

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}
