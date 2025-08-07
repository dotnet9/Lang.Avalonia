using Avalonia.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lang.Avalonia.MarkupExtensions;

public class ArgCollection(I18nBinding owner) : Collection<object>
{
    internal List<(bool IsBinding, int Index)> Indexes { get; } = [];

    protected override void InsertItem(int index, object item)
    {
        if (item is BindingBase binding)
        {
            Indexes.Add((true, owner.Bindings.Count));
            owner.Bindings.Add(binding);
        }
        else
        {
            Indexes.Add((false, Count));
            base.InsertItem(index, item);
        }
    }
}
