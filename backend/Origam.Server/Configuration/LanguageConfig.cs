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
        IConfigurationSection languageSection = configuration.GetSectionOrThrow(
            key: "LanguageConfig"
        );

        string defaultCulture = languageSection.GetStringOrThrow(key: "Default");
        if (string.IsNullOrWhiteSpace(value: defaultCulture))
        {
            throw new Exception(message: "Default culture in \"LanguageConfig\" cannot be empty");
        }
        DefaultCulture = new RequestCulture(culture: defaultCulture);

        CultureItems = languageSection
            .GetSectionOrThrow(key: "Allowed")
            .GetChildren()
            .Select(selector: CultureItem.Create)
            .ToArray();
        var defaultCultureExists = CultureItems.Any(predicate: item =>
            item.CultureName == defaultCulture
        );
        if (!defaultCultureExists)
        {
            throw new Exception(
                message: $"The default culture \"{defaultCulture}\" is not among the allowed cultures in the \"LanguageConfig\" section."
            );
        }
        AllowedCultures = CultureItems
            .Select(selector: item => new CultureInfo(name: item.CultureName))
            .ToArray();
    }
}

public class CultureItem
{
    public static CultureItem Create(IConfigurationSection section)
    {
        return new CultureItem
        {
            CultureName = section.GetStringOrThrow(key: "Culture"),
            Caption = section.GetStringOrThrow(key: "Caption"),
            ResetPasswordMailSubject = section[key: "ResetPasswordMailSubject"],
            ResetPasswordMailBodyFileName = section[key: "ResetPasswordMailBodyFileName"],
            DateCompleterConfig = DateCompleterConfig.Create(parentSection: section),
            DefaultDateFormats = DefaultDateFormats.Create(parentSection: section),
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
        var section = parentSection.GetSection(key: "DefaultDateFormats");
        return new DefaultDateFormats
        {
            Short = section?[key: "Short"] ?? "dd.MM.yyyy",
            Long = section?[key: "Long"] ?? "dd.MM.yyyy HH:mm:ss",
            Time = section?[key: "Time"] ?? "HH:mm:ss",
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
            DateSeparator = section?[key: "DateSeparator"] ?? ".",
            TimeSeparator = section?[key: "TimeSeparator"] ?? ":",
            DateTimeSeparator = section?[key: "DateTimeSeparator"] ?? " ",
            DateSequence = sequence,
        };
    }
}

public enum DateSequence
{
    DayMonthYear,
    MonthDayYear,
}
