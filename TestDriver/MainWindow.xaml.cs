using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using AutoCompleteTextBoxWPF;

namespace TestDriver
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			txtTest.ItemProvider = new AutoCompleteProvider();
		}

		class AutoCompleteProvider : IAutoCompleteItemProvider
		{
			public IEnumerable<string> GetItems()
			{
				return Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.System));
			}
		}

		private void cmdSubmit_Click(object sender, RoutedEventArgs e)
		{
			var animation = new DoubleAnimation() { From = 0.5, To = 1.0, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };

			cmdSubmit.BeginAnimation(Button.OpacityProperty, animation);
		}
	}
}
