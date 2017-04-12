using System;
using System.Composition;

namespace Linearstar.Coah
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
	public class FeedProviderAttribute : ExportAttribute
	{
		public int Priority
		{
			get;
			set;
		}

		public string DisplayName
		{
			get;
			set;
		}

		public FeedProviderAttribute()
			: base(typeof(FeedProvider))
		{
		}

		public FeedProviderAttribute(string displayName)
			: this()
		{
			DisplayName = displayName;
		}
	}
}
