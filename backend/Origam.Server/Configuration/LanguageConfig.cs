#region license

/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Origam.Extensions;

namespace Origam.Server.Configuration;

public class LanguageConfig
{
    public RequestCulture DefaultCulture { get; }
    public CultureInfo[] AllowedCultures { get; }
    public CultureItem[] CultureItems { get; }

    public LanguageConfig(IConfiguration configuration)
    {
        IConfigurationSection languageSection = configuration.GetSectionOrThrow("LanguageConfig");

        string defaultCulture = languageSection.GetStringOrThrow("Default");
        if (string.IsNullOrWhiteSpace(defaultCulture))
        {
            throw new Exception("Default culture in \"LanguageConfig\" cannot be empty");
        }
        DefaultCulture = new RequestCulture(defaultCulture);

        CultureItems = languageSection
            .GetSectionOrThrow("Allowed")
            .GetChildren()
            .Select(CultureItem.Create)
            .ToArray();
        var defaultCultureExists = CultureItems.Any(item => item.CultureName == defaultCulture);
        if (!defaultCultureExists)
        {
            throw new Exception(
                $"The default culture \"{defaultCulture}\" is not among the allowed cultures in the \"LanguageConfig\" section."
            );
        }
        AllowedCultures = CultureItems.Select(item => new CultureInfo(item.CultureName)).ToArray();
    }
}

public class CultureItem
{
    public static CultureItem Create(IConfigurationSection section)
    {
        return new CultureItem
        {
            CultureName = section.GetStringOrThrow("Culture"),
            Caption = section.GetStringOrThrow("Caption"),
            ResetPasswordMailSubject = section["ResetPasswordMailSubject"],
            ResetPasswordMailBodyFileName = section["ResetPasswordMailBodyFileName"],
            DateCompleterConfig = DateCompleterConfig.Create(section),
            DefaultDateFormats = DefaultDateFormats.Create(section),
        };
    }

    public string CultureName { get; set; }
    public string Caption { get; set; }
    public string ResetPasswordMailSubject { get; set; }
    public string ResetPasswordMailBodyFileName { get; set; }
    public DateCompleterConfig DateCompleterConfig { get; set; }
    public DefaultDateFormats DefaultDateFormats { get; set; }
}

public class DefaultDateFormats
{
    public string Short { get; set; }
    public string Long { get; set; }
    public string Time { get; set; }

    public static DefaultDateFormats Create(IConfigurationSection parentSection)
    {
        var section = parentSection.GetSection("DefaultDateFormats");
        return new DefaultDateFormats
        {
            Short = section?["Short"] ?? "dd.MM.yyyy",
            Long = section?["Long"] ?? "dd.MM.yyyy HH:mm:ss",
            Time = section?["Time"] ?? "HH:mm:ss",
        };
    }
}

public class DateCompleterConfig
{
    public string DateSeparator { get; set; }
    public string TimeSeparator { get; set; } = ":";
    public string DateTimeSeparator { get; set; } = " ";
    public DateSequence DateSequence { get; set; } = DateSequence.DayMonthYear;

    public static DateCompleterConfig Create(IConfigurationSection parentSection)
    {
        var section = parentSection.GetSection("DateCompleterConfig");
        bool parseSuccess = Enum.TryParse<DateSequence>(section?["DateSequence"], out var sequence);
        if (!parseSuccess)
        {
            sequence = Configuration.DateSequence.DayMonthYear;
        }
        return new DateCompleterConfig
        {
            DateSeparator = section?["DateSeparator"] ?? ".",
            TimeSeparator = section?["TimeSeparator"] ?? ":",
            DateTimeSeparator = section?["DateTimeSeparator"] ?? " ",
            DateSequence = sequence,
        };
    }
}

public enum DateSequence
{
    DayMonthYear,
    MonthDayYear,
}
