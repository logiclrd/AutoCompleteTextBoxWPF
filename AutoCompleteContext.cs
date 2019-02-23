using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoCompleteTextBoxWPF
{
	public class AutoCompleteContext : DependencyObject
	{
		public static DependencyProperty CurrentSearchProperty = DependencyProperty.Register(nameof(CurrentSearch), typeof(string), typeof(AutoCompleteContext));

		public string CurrentSearch
		{
			get { return (string)GetValue(CurrentSearchProperty); }
			set { SetValue(CurrentSearchProperty, value); }
		}
	}
}
