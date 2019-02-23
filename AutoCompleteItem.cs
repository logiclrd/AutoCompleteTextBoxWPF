using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ComboBoxAutoComplete
{
	public class AutoCompleteItem : DependencyObject, IDisposable
	{
		public static DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(AutoCompleteItem), new PropertyMetadata(AutoCompleteItem_LabelChanged));
		public static DependencyProperty IsVisibleProperty = DependencyProperty.Register(nameof(IsVisible), typeof(bool), typeof(AutoCompleteItem));
		public static DependencyProperty TextBlockProperty = DependencyProperty.Register(nameof(TextBlock), typeof(TextBlock), typeof(AutoCompleteItem));

		AutoCompleteContext _context;
		DependencyPropertyDescriptor _currentSearchPropertyDescriptor;

		public AutoCompleteItem(AutoCompleteContext context)
		{
			_context = context;

			this.TextBlock = new TextBlock();

			_currentSearchPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(AutoCompleteContext.CurrentSearchProperty, typeof(AutoCompleteContext));
			_currentSearchPropertyDescriptor.AddValueChanged(_context, _context_CurrentSearchChanged);
		}

		public void Dispose()
		{
			if (_currentSearchPropertyDescriptor != null)
			{
				_currentSearchPropertyDescriptor.RemoveValueChanged(_context, _context_CurrentSearchChanged);
				_currentSearchPropertyDescriptor = null;
			}
		}

		public string Label
		{
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}

		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		public TextBlock TextBlock
		{
			get { return (TextBlock)GetValue(TextBlockProperty); }
			set { SetValue(TextBlockProperty, value); }
		}

		static void AutoCompleteItem_LabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is AutoCompleteItem item)
				item.Render();
		}

		private void _context_CurrentSearchChanged(object sender, EventArgs e)
		{
			Render();
		}

		void Render()
		{
			var textBlock = this.TextBlock;

			string label = this.Label;
			string currentSearch = _context.CurrentSearch;

			textBlock.Inlines.Clear();

			if (string.IsNullOrWhiteSpace(currentSearch))
				IsVisible = true;
			else
			{
				currentSearch = currentSearch.Trim();

				int offset = label.IndexOf(currentSearch, StringComparison.InvariantCultureIgnoreCase);

				if (offset < 0)
					IsVisible = false;
				else
				{
					IsVisible = true;

					while (offset >= 0)
					{
						textBlock.Inlines.Add(label.Substring(0, offset));
						textBlock.Inlines.Add(new Run(label.Substring(offset, currentSearch.Length)) { FontWeight = FontWeights.Bold });

						label = label.Substring(offset + currentSearch.Length);
						offset = label.IndexOf(currentSearch, StringComparison.InvariantCultureIgnoreCase);
					}
				}
			}

			textBlock.Inlines.Add(label);
		}
	}
}
