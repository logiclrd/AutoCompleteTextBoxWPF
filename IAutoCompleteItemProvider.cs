using System.Collections.Generic;

namespace ComboBoxAutoComplete
{
	public interface IAutoCompleteItemProvider
	{
		IEnumerable<string> GetItems();
	}
}
