using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Lang.Avalonia;

namespace Lang.Avalonia.Json.BrowserDemo;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        LanguageSelector.SelectionChanged += OnLanguageSelectionChanged;
        Loaded += (_, _) => UpdateDiagnostics();
    }

    private void OnLanguageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (LanguageSelector.SelectedItem is LocalizationLanguage language)
        {
            I18nManager.Instance.Culture = new CultureInfo(language.CultureName);
        }

        UpdateDiagnostics();
    }

    private void UpdateDiagnostics()
    {
        var plugin = App.Plugin;
        if (plugin == null)
        {
            return;
        }

        var languages = I18nManager.Instance.GetLanguages();
        var builder = new StringBuilder();
        builder.AppendLine($"Embedded mode  : {App.UseEmbeddedResources}");
        builder.AppendLine($"ResourceFolder : {plugin.ResourceFolder}");
        builder.AppendLine($"DirectoryExists: {Directory.Exists(plugin.ResourceFolder)}");
        builder.AppendLine($"LanguagesLoaded: {languages?.Count ?? 0}");
        builder.AppendLine($"CurrentCulture : {I18nManager.Instance.Culture?.Name}");
        builder.AppendLine("LoadDiagnostics:");
        foreach (var diagnostic in plugin.LoadDiagnostics)
        {
            builder.AppendLine($"  - {diagnostic}");
        }

        DiagnosticsText.Text = builder.ToString();

        if (LanguageSelector.ItemsSource == null && languages is { Count: > 0 })
        {
            LanguageSelector.ItemsSource = languages;
            LanguageSelector.SelectedItem = languages.FirstOrDefault(language =>
                string.Equals(language.CultureName, I18nManager.Instance.Culture?.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
