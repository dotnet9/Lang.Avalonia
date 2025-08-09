﻿using System.Collections.Generic;
using System.Globalization;

namespace Lang.Avalonia;

public interface ILangPlugin
{
    void Load(CultureInfo cultureInfo);
    public CultureInfo Culture { get; set; }
    List<LocalizationLanguage>? GetLanguages();
    string? GetResource(string key, string? cultureName = null);
}