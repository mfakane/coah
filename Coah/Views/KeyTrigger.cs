using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Linearstar.Coah.Views
{
	class KeyTrigger : TriggerBase<FrameworkElement>
	{
		FrameworkElement container;
		UIElement root;

		public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(Key), typeof(KeyTrigger));
		public static readonly DependencyProperty ModifiersProperty = DependencyProperty.Register(nameof(Modifiers), typeof(ModifierKeys), typeof(KeyTrigger));

		public Key Key
		{
			get => (Key)GetValue(KeyProperty);
			set => SetValue(KeyProperty, value);
		}

		public ModifierKeys Modifiers
		{
			get => (ModifierKeys)GetValue(ModifiersProperty);
			set => SetValue(ModifiersProperty, value);
		}

		public bool ActiveOnFocus
		{
			get;
			set;
		} = true;

		protected override void OnAttached()
		{
			if (ActiveOnFocus)
				AssociatedObject.KeyDown += AssociatedObject_KeyDown;
			else
			{
				container = AssociatedObject.FindLogicalAncestor<FrameworkElement>().LastOrDefault();

				if (container != null)
					if (container.IsLoaded)
						Container_Loaded(container, new RoutedEventArgs());
					else
						container.Loaded += Container_Loaded;
			}

			if (AssociatedObject is MenuItem)
				((MenuItem)AssociatedObject).InputGestureText =
					(Modifiers == ModifierKeys.None ? null : Modifiers.ToString().Replace(", ", "+").Replace("Control", "Ctrl").Replace("Windows", "Win") + "+") + Key.ToString();

			base.OnAttached();
		}

		protected override void OnDetaching()
		{
			if (root != null)
			{
				root.KeyDown -= AssociatedObject_KeyDown;
				root = null;
			}

			if (container != null)
			{
				container.Loaded -= Container_Loaded;
				container = null;
			}

			AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
			base.OnDetaching();
		}

		void Container_Loaded(object sender, RoutedEventArgs e)
		{
			root = container.FindAncestor<UIElement>().LastOrDefault() ?? container;

			if (root != null)
			{
				container.Loaded -= Container_Loaded;
				root.KeyDown += AssociatedObject_KeyDown;
			}
		}

		void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
		{
			if (container?.IsVisible ?? true)
				if (e.Key == Key && e.KeyboardDevice.Modifiers == Modifiers)
				{
					if (container != null && AssociatedObject.DataContext == null)
						AssociatedObject.DataContext = container.DataContext;

					InvokeActions(e);
					e.Handled = true;
				}
		}
	}
}
