using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace AutoCompleteTextBoxWPF
{
	/// <summary>
	/// Interaction logic for AutoCompleteTextBox.xaml
	/// </summary>
	public partial class AutoCompleteTextBox : UserControl
	{
		public AutoCompleteTextBox()
		{
			InitializeComponent();

			_context = new AutoCompleteContext();

			spRoot.DataContext = this;

			lstAutoCompleteItems.ItemsSource = _items;

			DependencyPropertyDescriptor.FromProperty(TextBoxBase.IsSelectionActiveProperty, typeof(TextBoxBase)).AddValueChanged(
				txtInput,
				(sender, e) =>
				{
					SetValue(IsSelectionActivePropertyKey, txtInput.IsSelectionActive);
				});

			txtInput.SelectionChanged +=
				(sender, e) =>
				{
					RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
				};

			txtInput.TextChanged +=
				(sender, e) =>
				{
					RaiseEvent(new TextChangedEventArgs(TextChangedEvent, e.UndoAction, e.Changes));
				};

			this.SetBinding(
				SelectionBrushProperty,
				new Binding()
				{
					Source = txtInput,
					Path = new PropertyPath(TextBox.SelectionBrushProperty),
				});
			
			this.SetBinding(
				SelectionOpacityProperty,
				new Binding()
				{
					Source = txtInput,
					Path = new PropertyPath(TextBox.SelectionOpacityProperty),
				});
		}

		IAutoCompleteItemProvider _itemProvider;

		public IAutoCompleteItemProvider ItemProvider
		{
			get { return _itemProvider; }
			set
			{
				_itemProvider = value;
				ReloadItems();
			}
		}

		static readonly DependencyProperty TextEditor_IsReadOnlyProperty = (DependencyProperty)typeof(TextBox).Assembly.GetTypes().Single(t => t.Name == "TextEditor").GetField("IsReadOnlyProperty", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

		public static readonly DependencyProperty AutoWordSelectionProperty = DependencyProperty.Register(nameof(AutoWordSelection), typeof(bool), typeof(AutoCompleteTextBox), new PropertyMetadata(false));
		public static readonly DependencyProperty CaretBrushProperty = DependencyProperty.Register(nameof(CaretBrush), typeof(Brush), typeof(AutoCompleteTextBox));
		public static readonly DependencyProperty CharacterCasingProperty = DependencyProperty.Register(nameof(CharacterCasing), typeof(CharacterCasing), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(CharacterCasing.Normal));
		public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden));
		public static readonly DependencyProperty IsInactiveSelectionHighlightEnabledProperty = DependencyProperty.Register(nameof(IsInactiveSelectionHighlightEnabled), typeof(bool), typeof(AutoCompleteTextBox));
		public static readonly DependencyProperty IsReadOnlyProperty = TextEditor_IsReadOnlyProperty.AddOwner(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
		public static readonly DependencyProperty IsReadOnlyCaretVisibleProperty = DependencyProperty.Register(nameof(IsReadOnlyCaretVisible), typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
		public static readonly DependencyProperty IsUndoEnabledProperty = DependencyProperty.Register(nameof(IsUndoEnabled), typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(true));
		public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(0));
		public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register(nameof(MaxLines), typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(Int32.MaxValue));
		public static readonly DependencyProperty MinLinesProperty = DependencyProperty.Register(nameof(MinLines), typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(1));
		public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(GetDefaultSelectionBrush()));
		public static readonly DependencyProperty SelectionOpacityProperty = DependencyProperty.Register(nameof(SelectionOpacity), typeof(double), typeof(AutoCompleteContext), new FrameworkPropertyMetadata(SelectionOpacityDefaultValue));
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));
		public static readonly DependencyProperty TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(typeof(AutoCompleteTextBox));
		public static readonly DependencyProperty TextDecorationsProperty = Inline.TextDecorationsProperty.AddOwner(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(new TextDecorationCollection()));
		public static readonly DependencyProperty TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(TextWrapping.NoWrap));
		public static readonly DependencyProperty UndoLimitProperty = DependencyProperty.Register(nameof(UndoLimit), typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(-1));
		public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden));

		private static Brush GetDefaultSelectionBrush()
		{
			Brush selectionBrush = new SolidColorBrush(SystemColors.HighlightColor);
			selectionBrush.Freeze();
			return selectionBrush;
		}

		internal const double AdornerSelectionOpacityDefaultValue = 0.4;
		internal const double NonAdornerSelectionOpacityDefaultValue = 1;
		private static bool FrameworkAppContextSwitches_UseAdornerForTextboxSelectionRendering = (bool)typeof(TextBox).Assembly.GetTypes().Single(type => type.Name == "FrameworkAppContextSwitches").GetProperty("UseAdornerForTextboxSelectionRendering", BindingFlags.Static | BindingFlags.Public).GetValue(null);

		private static double SelectionOpacityDefaultValue = (FrameworkAppContextSwitches_UseAdornerForTextboxSelectionRendering) ? AdornerSelectionOpacityDefaultValue : NonAdornerSelectionOpacityDefaultValue;

		public bool AutoWordSelection
		{
			get { return (bool)GetValue(AutoWordSelectionProperty); }
			set { SetValue(AutoWordSelectionProperty, value); }
		}

		public Brush CaretBrush
		{
			get { return (Brush)GetValue(CaretBrushProperty); }
			set { SetValue(CaretBrushProperty, value); }
		}

		public CharacterCasing CharacterCasing
		{
			get { return (CharacterCasing)GetValue(CharacterCasingProperty); }
			set { SetValue(CharacterCasingProperty, value); }
		}

		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
			set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
		}

		public bool IsInactiveSelectionHighlightEnabled
		{
			get { return (bool)GetValue(IsInactiveSelectionHighlightEnabledProperty); }
			set { SetValue(IsInactiveSelectionHighlightEnabledProperty, value); }
		}

		public bool IsReadOnly
		{
			get { return (bool)GetValue(TextEditor_IsReadOnlyProperty); }
			set { SetValue(TextEditor_IsReadOnlyProperty, value); }
		}

		public bool IsReadOnlyCaretVisible
		{
			get { return (bool)GetValue(IsReadOnlyCaretVisibleProperty); }
			set { SetValue(IsReadOnlyCaretVisibleProperty, value); }
		}

		public bool IsUndoEnabled
		{
			get { return (bool)GetValue(IsUndoEnabledProperty); }
			set { SetValue(IsUndoEnabledProperty, value); }
		}

		[DefaultValue((int)0)]
		[Localizability(LocalizationCategory.None, Modifiability = Modifiability.Unmodifiable)] // cannot be modified by localizer
		public int MaxLength
		{
			get { return (int)GetValue(MaxLengthProperty); }
			set { SetValue(MaxLengthProperty, value); }
		}

		[DefaultValue(Int32.MaxValue)]
		public int MaxLines
		{
			get { return (int)GetValue(MaxLinesProperty); }
			set { SetValue(MaxLinesProperty, value); }
		}

		[DefaultValue(1)]
		public int MinLines
		{
			get { return (int)GetValue(MinLinesProperty); }
			set { SetValue(MinLinesProperty, value); }
		}

		public Brush SelectionBrush
		{
			get { return (Brush)GetValue(SelectionBrushProperty); }
			set { SetValue(SelectionBrushProperty, value); }
		}

		public double SelectionOpacity
		{
			get { return (double)GetValue(SelectionOpacityProperty); }
			set { SetValue(SelectionOpacityProperty, value); }
		}

		[DefaultValue("")]
		[Localizability(LocalizationCategory.Text)]
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public TextAlignment TextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentProperty); }
			set { SetValue(TextAlignmentProperty, value); }
		}

		public TextDecorationCollection TextDecorations
		{
			get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
			set { SetValue(TextDecorationsProperty, value); }
		}

		public TextWrapping TextWrapping
		{
			get { return (TextWrapping)GetValue(TextWrappingProperty); }
			set { SetValue(TextWrappingProperty, value); }
		}

		public int UndoLimit
		{
			get { return (int)GetValue(UndoLimitProperty); }
			set { SetValue(UndoLimitProperty, value); }
		}

		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
			set { SetValue(VerticalScrollBarVisibilityProperty, value); }
		}

		public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AutoCompleteTextBox));
		public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(nameof(TextChanged), RoutingStrategy.Bubble, typeof(TextChangedEventHandler), typeof(AutoCompleteTextBox));

		public event RoutedEventHandler SelectionChanged
		{
			add { AddHandler(SelectionChangedEvent, value); }
			remove { RemoveHandler(SelectionChangedEvent, value); }
		}

		public event TextChangedEventHandler TextChanged
		{
			add { AddHandler(TextChangedEvent, value); }
			remove { RemoveHandler(TextChangedEvent, value); }
		}

		internal static readonly DependencyPropertyKey IsSelectionActivePropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(IsSelectionActive), typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
		public static readonly DependencyProperty IsSelectionActiveProperty = IsSelectionActivePropertyKey.DependencyProperty;

		public bool IsSelectionActive
		{
			get { return (bool)GetValue(IsSelectionActiveProperty); }
		}

		public void AppendText(string textData) => txtInput.AppendText(textData);
		public bool CanUndo => txtInput.CanUndo;
		public bool CanRedo => txtInput.CanRedo;

		public int CaretIndex
		{
			get => txtInput.CaretIndex;
			set => txtInput.CaretIndex = value;
		}

		public void Clear() => txtInput.Clear();
		public void Copy() => txtInput.Copy();
		public void Cut() => txtInput.Cut();
		public IDisposable DeclareChangeBlock() => txtInput.DeclareChangeBlock();
		public void EndChange() => txtInput.EndChange();
		public double ExtentWidth => txtInput.ExtentWidth;
		public double ExtentHeight => txtInput.ExtentHeight;
		public void GetCharacterIndexFromLineIndex(int lineIndex) => txtInput.GetCharacterIndexFromLineIndex(lineIndex);
		public void GetCharacterIndexFromPoint(Point point, bool snapToText) => txtInput.GetCharacterIndexFromPoint(point, snapToText);
		public void GetFirstVisibleLineIndex() => txtInput.GetFirstVisibleLineIndex();
		public void GetLastVisibleLineIndex() => txtInput.GetLastVisibleLineIndex();
		public void GetLineIndexFromCharacterIndex(int charIndex) => txtInput.GetLineIndexFromCharacterIndex(charIndex);
		public void GetLineLength(int lineIndex) => txtInput.GetLineLength(lineIndex);
		public void GetLineText(int lineIndex) => txtInput.GetLineText(lineIndex);
		public void GetNextSpellingErrorCharacterIndex(int charIndex, LogicalDirection direction) => txtInput.GetNextSpellingErrorCharacterIndex(charIndex, direction);
		public void GetRectFromCharacterIndex(int charIndex) => txtInput.GetRectFromCharacterIndex(charIndex);
		public void GetSpellingError(int charIndex) => txtInput.GetSpellingError(charIndex);
		public void GetSpellingErrorLength(int charIndex) => txtInput.GetSpellingErrorLength(charIndex);
		public void GetSpellingErrorStart(int charIndex) => txtInput.GetSpellingErrorStart(charIndex);
		public double HorizontalOffset => txtInput.HorizontalOffset;
		public int LineCount => txtInput.LineCount;
		public void LineDown() => txtInput.LineDown();
		public void LineLeft() => txtInput.LineLeft();
		public void LineRight() => txtInput.LineRight();
		public void LineUp() => txtInput.LineUp();
		public void LockCurrentUndoUnit() => txtInput.LockCurrentUndoUnit();
		public void PageDown() => txtInput.PageDown();
		public void PageLeft() => txtInput.PageLeft();
		public void PageRight() => txtInput.PageRight();
		public void PageUp() => txtInput.PageUp();
		public void Paste() => txtInput.Paste();
		public void Redo() => txtInput.Redo();
		public void ScrollToEnd() => txtInput.ScrollToEnd();
		public void ScrollToHome() => txtInput.ScrollToHome();
		public void ScrollToHorizontalOffset(double offset) => txtInput.ScrollToHorizontalOffset(offset);
		public void ScrollToLine(int lineIndex) => txtInput.ScrollToLine(lineIndex);
		public void ScrollToVerticalOffset(double offset) => txtInput.ScrollToVerticalOffset(offset);
		public void Select(int start, int length) => txtInput.Select(start, length);
		public void SelectAll() => txtInput.SelectAll();

		public string SelectedText
		{
			get => txtInput.SelectedText;
			set => txtInput.SelectedText = value;
		}

		[DefaultValue((int)0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionLength
		{
			get => txtInput.SelectionLength;
			set => txtInput.SelectionLength = value;
		}

		[DefaultValue((int)0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionStart
		{
			get => txtInput.SelectionStart;
			set => txtInput.SelectionStart = value;
		}

		public SpellCheck SpellCheck => txtInput.SpellCheck;
		public Typography Typography => txtInput.Typography;
		public void Undo() => txtInput.Undo();
		public double VerticalOffset => txtInput.VerticalOffset;
		public double ViewportWidth => txtInput.ViewportWidth;
		public double ViewportHeight => txtInput.ViewportHeight;

		AutoCompleteContext _context;
		ObservableCollection<AutoCompleteItem> _items = new ObservableCollection<AutoCompleteItem>();

		void ReloadItems()
		{
			int index = 0;

			foreach (var item in _itemProvider.GetItems())
			{
				if (index >= _items.Count)
					_items.Add(new AutoCompleteItem(_context) { Label = item });
				else if (_items[index].Label != item)
					_items[index].Label = item;

				index++;
			}
		}

		void ResetOpen()
		{
			if (!pAutoCompletePopup.IsOpen)
			{
				pAutoCompletePopup.IsOpen = true;

				ReloadItems();

				for (int i = 0; i < _items.Count; i++)
					if (_items[i].IsVisible)
					{
						lstAutoCompleteItems.SelectedIndex = i;
						break;
					}
			}
		}

		void ResetClose()
		{
			pAutoCompletePopup.IsOpen = false;
			lstAutoCompleteItems.SelectedIndex = -1;
		}

		private void txtInput_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var clickPosition = e.GetPosition(txtInput);

			var hitTestResult = VisualTreeHelper.HitTest(txtInput, clickPosition);

			var clickRecipient = hitTestResult?.VisualHit;

			while (clickRecipient != null)
			{
				if (ReferenceEquals(clickRecipient, txtInput))
				{
					ResetOpen();
					break;
				}

				clickRecipient = VisualTreeHelper.GetParent(clickRecipient);
			}
		}

		private void txtInput_GotKeyboardFocus(object sender, RoutedEventArgs e)
		{
			ResetOpen();
		}

		private void txtInput_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			ResetClose();
		}

		private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
		{
			_context.CurrentSearch = txtInput.Text;

			if (_items.Any(item => item.IsVisible))
				lstAutoCompleteItems.Visibility = Visibility.Visible;
			else
				lstAutoCompleteItems.Visibility = Visibility.Collapsed;
		}

		private void txtInput_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				ResetClose();
			else if (e.Key == Key.Enter)
			{
				if (lstAutoCompleteItems.SelectedIndex >= 0)
				{
					e.Handled = true;
					txtInput.Text = _items[lstAutoCompleteItems.SelectedIndex].Label;
					txtInput.SelectionStart = txtInput.Text.Length;
				}

				ResetClose();
			}
			else if ((e.Key == Key.Up) || (e.Key == Key.Down)
						|| (e.Key == Key.PageUp) || (e.Key == Key.PageDown)
						|| (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && ((e.Key == Key.Home) || (e.Key == Key.End))))
			{
				e.Handled = true;

				int currentIndex = lstAutoCompleteItems.SelectedIndex;
				int newIndex = currentIndex;

				switch (e.Key)
				{
					case Key.Up:
					{
						newIndex = currentIndex - 1;

						while ((newIndex > 0) && !_items[newIndex].IsVisible)
							newIndex--;

						if ((newIndex < 0) || !_items[newIndex].IsVisible)
							newIndex = currentIndex;

						break;
					}
					case Key.Down:
					{
						newIndex = currentIndex + 1;

						while ((newIndex < _items.Count) && !_items[newIndex].IsVisible)
							newIndex++;

						if (newIndex >= _items.Count)
							newIndex = currentIndex;

						break;
					}
					case Key.PageUp:
					{
						newIndex = currentIndex;

						double scrollChange = 0.0;
						double scrollChangeLimit = lstAutoCompleteItems.ActualHeight;

						while (newIndex > 0)
						{
							var container = (ListBoxItem)lstAutoCompleteItems.ItemContainerGenerator.ContainerFromItem(_items[newIndex - 1]);

							scrollChange += container.ActualHeight;

							if (scrollChange >= scrollChangeLimit)
								break;

							newIndex--;
						}

						if (newIndex + 1 < _items.Count)
							newIndex++;

						if (newIndex < 0)
							newIndex = 0;

						while ((newIndex < _items.Count) && !_items[newIndex].IsVisible)
							newIndex++;

						if (newIndex > currentIndex)
							newIndex = currentIndex;

						break;
					}
					case Key.PageDown:
					{
						newIndex = currentIndex;

						if (newIndex < 0)
							newIndex = 0;

						double scrollChange = 0.0;
						double scrollChangeLimit = lstAutoCompleteItems.ActualHeight;

						while (newIndex + 1 < _items.Count)
						{
							var container = (ListBoxItem)lstAutoCompleteItems.ItemContainerGenerator.ContainerFromItem(_items[newIndex]);

							scrollChange += container.ActualHeight;

							if (scrollChange >= scrollChangeLimit)
								break;

							newIndex++;
						}

						if (newIndex > 0)
							newIndex--;

						while ((newIndex > 0) && !_items[newIndex].IsVisible)
							newIndex--;

						if (newIndex < currentIndex)
							newIndex = currentIndex;

						break;
					}
					case Key.Home:
					{
						for (int i = 0; i < _items.Count; i++)
							if (_items[i].IsVisible)
							{
								newIndex = i;
								break;
							}

						break;
					}
					case Key.End:
					{
						for (int i = _items.Count - 1; i >= 0; i--)
							if (_items[i].IsVisible)
							{
								newIndex = i;
								break;
							}

						break;
					}
				}

				if (newIndex < 0)
					newIndex = 0;
				if (newIndex >= _items.Count)
					newIndex = _items.Count - 1;

				if (newIndex != currentIndex)
				{
					lstAutoCompleteItems.SelectedIndex = newIndex;
					lstAutoCompleteItems.ScrollIntoView(_items[newIndex]);
				}
			}
		}

		private void lstAutoCompleteItems_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var clickPosition = e.GetPosition(lstAutoCompleteItems);

			var hitTestResult = VisualTreeHelper.HitTest(lstAutoCompleteItems, clickPosition);

			var clickRecipient = hitTestResult?.VisualHit;

			while (clickRecipient != null)
			{
				if (clickRecipient is ListBoxItem itemContainer)
				{
					if (lstAutoCompleteItems.ItemContainerGenerator.ItemFromContainer(itemContainer) is AutoCompleteItem clickedItem)
					{
						txtInput.Text = clickedItem.Label;
						ResetClose();
					}

					break;
				}

				clickRecipient = VisualTreeHelper.GetParent(clickRecipient);
			}
		}
	}
}
