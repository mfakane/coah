using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Linearstar.Coah.Views
{
	[TemplatePart(Name = ToggleButtonElementName, Type = typeof(ToggleButton))]
	class SplitMenuButton : Button
	{
		public static readonly DependencyProperty DropDownMenuProperty = DependencyProperty.Register(nameof(DropDownMenu), typeof(ContextMenu), typeof(SplitMenuButton), new PropertyMetadata((sender, e) =>
		{
			var d = (SplitMenuButton)sender;

			d.UpdateMenu((ContextMenu)e.OldValue, (ContextMenu)e.NewValue);
		}));
		const string ToggleButtonElementName = "PART_ToggleButton";
		ToggleButton toggleButton;

		public ContextMenu DropDownMenu
		{
			get => (ContextMenu)GetValue(DropDownMenuProperty);
			set => SetValue(DropDownMenuProperty, value);
		}

		public SplitMenuButton()
		{
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			toggleButton = (ToggleButton)GetTemplateChild(ToggleButtonElementName);
			toggleButton.Click += (sender, e) => e.Handled = true;
			UpdateMenu(null, DropDownMenu);
		}

		void UpdateMenu(ContextMenu oldValue, ContextMenu newValue)
		{
			if (toggleButton != null)
			{
				if (oldValue != null)
				{
					oldValue.PlacementTarget = null;
					oldValue.ClearValue(ContextMenu.IsOpenProperty);
				}

				if (newValue != null)
				{
					newValue.PlacementTarget = this;
					newValue.Placement = PlacementMode.Bottom;
					newValue.SetBinding(ContextMenu.IsOpenProperty, new Binding("IsChecked") { Source = toggleButton });
				}
			}
		}
	}
}
