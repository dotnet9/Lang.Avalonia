using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lang.Avalonia.Xml.Demo.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        Languages = I18nManager.Instance.Resources.Select(kvp => kvp.Value).ToList();
        SelectLanguage = Languages.FirstOrDefault(l => l.CultureName == I18nManager.Instance.Culture.Name);

        AllCultures = new ObservableCollection<CultureInfo>(I18nManager.Instance.GetAvailableCultures());

        // 初始化演示数据
        _userCount = 1;
        _articleTitle = "Hello World: Internationalization Demo";
        GenerateSlug();

        Task.Run(async () =>
        {
            while (true)
            {
                CurrentTime = DateTime.Now;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        });
    }

    public List<LocalizationLanguage> Languages { get; set; }
    public LocalizationLanguage? _selectLanguage;

    public LocalizationLanguage? SelectLanguage
    {
        get => _selectLanguage;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectLanguage, value);
            I18nManager.Instance.Culture = new CultureInfo(value.CultureName);
            // 语言变更时更新Slug
            GenerateSlug();
        }
    }

    private DateTime _currentTime;

    public DateTime CurrentTime
    {
        get => _currentTime;
        set => this.RaiseAndSetIfChanged(ref _currentTime, value);
    }

    // 复数形式演示
    private int _userCount;
    public int UserCount
    {
        get => _userCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _userCount, value);
        }
    }

    // 标题转Slug演示
    private string _articleTitle;
    public string ArticleTitle
    {
        get => _articleTitle;
        set
        {
            this.RaiseAndSetIfChanged(ref _articleTitle, value);
            GenerateSlug();
        }
    }

    private string _generatedSlug;
    public string GeneratedSlug
    {
        get => _generatedSlug;
        private set => this.RaiseAndSetIfChanged(ref _generatedSlug, value);
    }

    public void GenerateSlug()
    {
        if (string.IsNullOrWhiteSpace(ArticleTitle))
        {
            GeneratedSlug = string.Empty;
            return;
        }

        // 移除特殊字符，转换为小写，空格替换为连字符
        var slug = new StringBuilder();
        foreach (char c in ArticleTitle.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c))
            {
                slug.Append(c);
            }
            else if (c == ' ')
            {
                slug.Append('-');
            }
        }

        // 移除连续的连字符
        while (slug.ToString().Contains("--"))
        {
            slug.Replace("--", "-");
        }

        // 移除首尾的连字符
        GeneratedSlug = slug.ToString().Trim('-');
    }

    // 格式化日期
    public string GetFormattedDate(string format)
    {
        return CurrentTime.ToString(format, I18nManager.Instance.Culture);
    }

    // 格式化数字
    public string FormatNumber(double number, string format)
    {
        return number.ToString(format, I18nManager.Instance.Culture);
    }

    public ObservableCollection<CultureInfo> AllCultures { get; }
}