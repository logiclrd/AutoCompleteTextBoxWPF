using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

using AutoCompleteTextBoxWPF;

using DataGen.NumberToWords;

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
			Log("Submit: " + txtTest.Text);

			var animation = new DoubleAnimation() { From = 0.5, To = 1.0, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };

			cmdSubmit.BeginAnimation(Button.OpacityProperty, animation);
		}

		private void txtTest_TextChanged(object sender, TextChangedEventArgs e)
		{
			Log("TextChanged event: { " + txtTest.Text + " / " + e.UndoAction + " / " + JsonSerializer.Serialize(e.Changes) + "}");
		}

		int _appendIndex;

		private void cmdAppend_Click(object sender, RoutedEventArgs e)
		{
			string appendText = (++_appendIndex).ToWords(CultureInfo.CurrentCulture);

			Log("Appending text: " + appendText);
			txtTest.Text += appendText;
		}

		void Log(string line)
		{
			if (txtSubmittedInput != null)
				txtSubmittedInput.Text = line + "\n" + txtSubmittedInput.Text;
		}
	}
}
