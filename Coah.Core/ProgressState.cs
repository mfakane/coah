using System;

namespace Linearstar.Coah
{
	public class ProgressState : NotifyBase, IProgress<ProgressInfo>
	{
		public ProgressInfo CurrentProgress
		{
			get => GetValue<ProgressInfo>();
			private set
			{
				SetValue(value);
				OnPropertyChanged(nameof(IsBusy));

				if (value != null && value.Exception != null)
					CurrentError = value;
			}
		}

		public bool IsBusy => CurrentProgress != null && !CurrentProgress.HasCompleted;

		public ProgressInfo CurrentError
		{
			get => GetValue<ProgressInfo>();
			private set
			{
				SetValue(value);
				OnPropertyChanged(nameof(HasError));
			}
		}

		public bool HasError => CurrentError != null;

		public void Report(ProgressInfo value) =>
			CurrentProgress = value;
	}
}
