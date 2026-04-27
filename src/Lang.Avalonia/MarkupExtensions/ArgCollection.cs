using Avalonia.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lang.Avalonia.MarkupExtensions;

/// <summary>
/// I18n 绑定参数集合。常量参数直接保存，Binding 参数会接入 MultiBinding。
/// </summary>
public class ArgCollection(I18nBinding owner) : Collection<object>
{
    internal List<(bool IsBinding, int Index)> Indexes { get; } = [];

    /// <inheritdoc />
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
