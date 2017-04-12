namespace Linearstar.Coah
{
	public struct DisplayRange
	{
		public static DisplayRange All = new DisplayRange();
		public static DisplayRange Top50 = new DisplayRange(null, 50);
		public static DisplayRange Latest50 = new DisplayRange { Latest = 50, };

		public bool IsSingle => !Latest.HasValue && Begin.HasValue && End.HasValue && Begin == End;

		public int? Begin
		{
			get;
			set;
		}

		public int? End
		{
			get;
			set;
		}

		public int? Latest
		{
			get;
			set;
		}

		public DisplayRange(int? begin, int? end)
			: this()
		{
			Begin = begin;
			End = end;
		}

		public DisplayRange(int? num)
			: this(num, num)
		{
		}

		public override string ToString()
		{
			if (Latest.HasValue)
				return $"最新 {Latest}";

			if (IsSingle)
				return Begin.ToString();

			if (Begin.HasValue || End.HasValue)
				return $"{(Begin ?? 1)}-{End}";

			return "すべて";
		}

		public override bool Equals(object obj) =>
			obj is DisplayRange &&
			((DisplayRange)obj).Begin == Begin &&
			((DisplayRange)obj).End == End &&
			((DisplayRange)obj).Latest == Latest;

		public override int GetHashCode() =>
			typeof(DisplayRange).GetHashCode() ^ Begin.GetHashCode() ^ End.GetHashCode() ^ Latest.GetHashCode();

		public static bool operator ==(DisplayRange a, DisplayRange b) =>
			a.Begin == b.Begin &&
			a.End == b.End &&
			a.Latest == b.Latest;

		public static bool operator !=(DisplayRange a, DisplayRange b) =>
			a.Begin != b.Begin ||
			a.End != b.End ||
			a.Latest != b.Latest;
	}
}
