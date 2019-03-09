using System;
using System.Collections.Generic;
using System.Linq;

namespace Linearstar.Coah
{
	static class EnumerableExtensions
	{
		public static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext) =>
			source.Select(x =>
			{
				onNext(x);

				return x;
			});

		public static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource, int> onNext) =>
			source.Select((x, idx) =>
			{
				onNext(x, idx);

				return x;
			});

		public static ParallelQuery<TSource> Do<TSource>(this ParallelQuery<TSource> source, Action<TSource> onNext) =>
			source.Select(x =>
			{
				onNext(x);

				return x;
			});

		public static ParallelQuery<TSource> Do<TSource>(this ParallelQuery<TSource> source, Action<TSource, int> onNext) =>
			source.Select((x, idx) =>
			{
				onNext(x, idx);

				return x;
			});
	}
}
