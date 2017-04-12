using System;
using System.Collections.Generic;
using System.Linq;

namespace Linearstar.Coah
{
	static class EnumerableExtensions
	{
		public static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext) =>
			source.Select(_ =>
			{
				onNext(_);

				return _;
			});

		public static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource, int> onNext) =>
			source.Select((_, idx) =>
			{
				onNext(_, idx);

				return _;
			});

		public static ParallelQuery<TSource> Do<TSource>(this ParallelQuery<TSource> source, Action<TSource> onNext) =>
			source.Select(_ =>
			{
				onNext(_);

				return _;
			});

		public static ParallelQuery<TSource> Do<TSource>(this ParallelQuery<TSource> source, Action<TSource, int> onNext) =>
			source.Select((_, idx) =>
			{
				onNext(_, idx);

				return _;
			});
	}
}
