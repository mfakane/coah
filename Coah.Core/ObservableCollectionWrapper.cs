using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Linearstar.Coah
{
	public class ObservableCollectionWrapper<TInputCollection, TInput, TOutputCollection, TOutput> : IReadOnlyList<TOutput>, INotifyCollectionChanged, IDisposable
		where TInputCollection : IList<TInput>, INotifyCollectionChanged
		where TOutputCollection : IList<TOutput>, INotifyCollectionChanged
	{
		readonly Func<TInput, TOutput> converter;
		readonly Func<TOutput, TInput> reverseSelector;
		bool isUpdating;

		public TInputCollection InputCollection { get; }
		public TOutputCollection OutputCollection { get; }

		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add
			{
				OutputCollection.CollectionChanged += value;
			}
			remove
			{
				OutputCollection.CollectionChanged -= value;
			}
		}

		public TOutput this[int index] => OutputCollection[index];
		public int Count => OutputCollection.Count;

		public ObservableCollectionWrapper(TInputCollection inputCollection, TOutputCollection outputCollection, Func<TInput, TOutput> converter)
		{
			InputCollection = inputCollection;
			OutputCollection = outputCollection;
			this.converter = converter;

			foreach (var i in InputCollection)
				OutputCollection.Add(converter(i));

			inputCollection.CollectionChanged += InputCollection_CollectionChanged;
		}

		public ObservableCollectionWrapper(TInputCollection inputCollection, TOutputCollection outputCollection, Func<TInput, TOutput> converter, Func<TOutput, TInput> reverseSelector)
			: this(inputCollection, outputCollection, converter)
		{
			this.reverseSelector = reverseSelector;
			outputCollection.CollectionChanged += OutputCollection_CollectionChanged;
		}

		void InputCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (isUpdating)
				return;

			isUpdating = true;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (TInput i in e.NewItems)
						OutputCollection.Add(converter(i));

					break;
				case NotifyCollectionChangedAction.Move:
					var item = OutputCollection[e.OldStartingIndex];

					OutputCollection.RemoveAt(e.OldStartingIndex);
					OutputCollection.Insert(e.NewStartingIndex - (e.NewStartingIndex > e.OldStartingIndex ? 1 : 0), item);

					break;
				case NotifyCollectionChangedAction.Remove:
					for (var i = e.OldItems.Count - 1; i >= 0; i--)
						OutputCollection.RemoveAt(e.OldStartingIndex + i);

					break;
				case NotifyCollectionChangedAction.Replace:
					for (int i = 0; i < e.OldItems.Count; i++)
						OutputCollection[i + e.OldStartingIndex] = converter((TInput)e.NewItems[i]);

					break;
				case NotifyCollectionChangedAction.Reset:
					OutputCollection.Clear();

					foreach (var i in InputCollection)
						OutputCollection.Add(converter(i));

					break;
			}

			isUpdating = false;
		}

		void OutputCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (isUpdating)
				return;

			isUpdating = true;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (TOutput i in e.NewItems)
						InputCollection.Add(reverseSelector(i));

					break;
				case NotifyCollectionChangedAction.Move:
					var item = InputCollection[e.OldStartingIndex];

					InputCollection.RemoveAt(e.OldStartingIndex);
					InputCollection.Insert(e.NewStartingIndex - (e.NewStartingIndex > e.OldStartingIndex ? 1 : 0), item);

					break;
				case NotifyCollectionChangedAction.Remove:
					for (var i = e.OldItems.Count - 1; i >= 0; i--)
						InputCollection.RemoveAt(e.OldStartingIndex + i);

					break;
				case NotifyCollectionChangedAction.Replace:
					for (int i = 0; i < e.OldItems.Count; i++)
						InputCollection[i + e.OldStartingIndex] = reverseSelector((TOutput)e.NewItems[i]);

					break;
				case NotifyCollectionChangedAction.Reset:
					InputCollection.Clear();

					foreach (var i in OutputCollection)
						InputCollection.Add(reverseSelector(i));

					break;
			}

			isUpdating = false;
		}

		public IEnumerator<TOutput> GetEnumerator() => OutputCollection.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Dispose()
		{
			InputCollection.CollectionChanged -= InputCollection_CollectionChanged;
			OutputCollection.CollectionChanged -= OutputCollection_CollectionChanged;
		}
	}

	public static class ObservableCollectionWrapper
	{
		public static ObservableCollectionWrapper<TCollection, TInput, ObservableCollection<TOutput>, TOutput> Create<TCollection, TInput, TOutput>(TCollection collection, Func<TInput, TOutput> converter)
			where TCollection : IList<TInput>, INotifyCollectionChanged =>
			new ObservableCollectionWrapper<TCollection, TInput, ObservableCollection<TOutput>, TOutput>(collection, new ObservableCollection<TOutput>(), converter);

		public static ObservableCollectionWrapper<TInputCollection, TInput, TOutputCollection, TOutput> Create<TInputCollection, TInput, TOutputCollection, TOutput>(TInputCollection inputCollection, TOutputCollection outputCollection, Func<TInput, TOutput> converter, Func<TOutput, TInput> reverseSelector)
			where TInputCollection : IList<TInput>, INotifyCollectionChanged
			where TOutputCollection : IList<TOutput>, INotifyCollectionChanged =>
			new ObservableCollectionWrapper<TInputCollection, TInput, TOutputCollection, TOutput>(inputCollection, outputCollection, converter, reverseSelector);
	}
}
