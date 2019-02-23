using System.Collections.Generic;

namespace AutoCompleteTextBoxWPF
{
	public interface IAutoCompleteItemProvider
	{
		IEnumerable<string> GetItems();
	}
}
