using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

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
				return Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
			}
		}
	}
}
