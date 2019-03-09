using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Linearstar.Coah.Views
{
	[ContentProperty("Styles")]
	public class GenericStyleSelector : StyleSelector
	{
		static GenericStyleSelector current;
		readonly Dictionary<Style, Type> dataTypes = new Dictionary<Style, Type>();

		public static Type GetDataType(Style obj) =>
			current.dataTypes.ContainsKey(obj) ? current.dataTypes[obj] : null;

		public static void SetDataType(Style obj, Type value) =>
			current.dataTypes[obj] = value;

		public List<Style> Styles { get; set; }

		public GenericStyleSelector()
		{
			current = this;
			Styles = new List<Style>();
		}

		public override Style SelectStyle(object item, DependencyObject container)
		{
			if (item != null)
			{
				var type = item.GetType();
				var rt = Styles.Where(x => dataTypes.ContainsKey(x) && dataTypes[x].IsAssignableFrom(type))
							   .OrderByDescending(x => GetLevel(dataTypes[x]))
							   .FirstOrDefault();

				if (rt != null)
					return rt;
			}

			return base.SelectStyle(item, container);
		}

		int GetLevel(Type type)
		{
			var rt = 0;

			while (type.BaseType != null
				&& type.BaseType != typeof(object))
			{
				type = type.BaseType;
				rt++;
			}

			return rt;
		}
	}
}
