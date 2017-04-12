using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Linearstar.Coah.Views
{
	class DropDownMenuButton : ToggleButton
	{
		public static readonly DependencyProperty DropDownMenuProperty = DependencyProperty.Register(nameof(DropDownMenu), typeof(ContextMenu), typeof(DropDownMenuButton), new PropertyMetadata((sender, e) =>
		{
			if (e.OldValue != null)
			{
				var c = (ContextMenu)e.OldValue;

				c.PlacementTarget = null;
				c.ClearValue(ContextMenu.IsOpenProperty);
			}

			if (e.NewValue != null)
			{
				var c = (ContextMenu)e.NewValue;

				c.PlacementTarget = (UIElement)sender;
				c.Placement = PlacementMode.Bottom;
				c.SetBinding(ContextMenu.IsOpenProperty, new Binding("IsChecked") { Source = sender });
			}
		}));

		public ContextMenu DropDownMenu
		{
			get => (ContextMenu)GetValue(DropDownMenuProperty);
			set => SetValue(DropDownMenuProperty, value);
		}
	}
}
