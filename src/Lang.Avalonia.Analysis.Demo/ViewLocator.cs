using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Lang.Avalonia.Analysis.Demo.ViewModels;
using Lang.Avalonia.Analysis.Demo.Views;

namespace Lang.Avalonia.Analysis.Demo;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        if (param is MainWindowViewModel)
        {
            return new MainWindow();
        }

        return new TextBlock { Text = "Not Found: " + param.GetType().FullName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
