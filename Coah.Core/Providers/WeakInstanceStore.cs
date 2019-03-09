using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public class WeakInstanceStore<TKey, TValue>
			where TValue : class
	{
		readonly ConcurrentDictionary<TKey, WeakReference<TValue>> instances = new ConcurrentDictionary<TKey, WeakReference<TValue>>();

		public int Count => instances.Count;

		public void Clean()
		{
			foreach (var i in instances.ToArray())
				if (!i.Value.TryGetTarget(out _))
					instances.TryRemove(i.Key, out _);
		}

		public bool TryGetValue(TKey key, out TValue target)
		{
			if (!instances.TryGetValue(key, out var value))
			{
				target = default;

				return false;
			}

			if (value.TryGetTarget(out target))
				return true;

			instances.TryRemove(key, out _);

			return false;
		}

		public async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> valueFactory) =>
			TryGetValue(key, out var value)
				? value
				: AddOrUpdate(key, await valueFactory(key).ConfigureAwait(false));

		public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
		{
			var target = default(TValue);
			var value = instances.GetOrAdd(key, k => new WeakReference<TValue>(target = valueFactory(k)));

			if (target == default(TValue))
				value.TryGetTarget(out target);

			return target;
		}

		public TValue AddOrUpdate(TKey key, TValue target)
		{
			instances.AddOrUpdate(key, k => new WeakReference<TValue>(target), (k, v) =>
			{
				v.SetTarget(target);

				return v;
			});

			return target;
		}
	}
}
