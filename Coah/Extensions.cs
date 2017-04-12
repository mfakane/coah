using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace Linearstar.Coah
{
	static class Extensions
	{
		public static IObservable<PropertyChangedEventArgs> OnPropertyChanged(this INotifyPropertyChanged obj, string propertyName) =>
			Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => (sender, e) => h(e), h => obj.PropertyChanged += h, h => obj.PropertyChanged -= h)
					  .Where(e => e.PropertyName == propertyName)
					  .StartWith(new PropertyChangedEventArgs(propertyName));

		public static IObservable<NotifyCollectionChangedEventArgs> OnCollectionChanged(this INotifyCollectionChanged obj) =>
			Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(h => (sender, e) => h(e), h => obj.CollectionChanged += h, h => obj.CollectionChanged -= h);

		public static IEnumerable<T> FindAncestor<T>(this DependencyObject current)
			where T : DependencyObject
		{
			while ((current = VisualTreeHelper.GetParent(current)) != null)
				if (current is T rt)
					yield return rt;
		}

		public static IEnumerable<T> FindLogicalAncestor<T>(this DependencyObject current)
			where T : DependencyObject
		{
			while ((current = LogicalTreeHelper.GetParent(current)) != null)
				if (current is T rt)
					yield return rt;
		}
	}
}
