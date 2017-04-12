using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Linearstar.Coah
{
	public interface ILocationItem
	{
		string Name
		{
			get;
			set;
		}

		IList<ILocationItem> Items
		{
			get;
		}

		Task Refresh(CancellationToken cts, IProgress<ProgressInfo> progress);
	}
}
