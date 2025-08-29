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
using System.Windows.Forms;
using Origam.Extensions;

namespace Origam.Gui.UI;
internal class DatetimeFormatter : Formatter
{
    private const string DateTimeSeparator = " ";
    private readonly Func<DateTime> timeNowFunc;
    public DatetimeFormatter(TextBox textBox, string customFormat, Func<DateTime> timeNowFunc)
        : base(textBox, customFormat)
    {
        this.timeNowFunc = timeNowFunc;
    }
    private static string DateFormat => Culture.DateTimeFormat.ShortDatePattern;
    private static string DateSeparator => Culture.DateTimeFormat.DateSeparator;
    private static string TimeSeparator => Culture.DateTimeFormat.TimeSeparator;
    public override void OnLeave(object sender, EventArgs e)
    {
        Text = Text.Trim();
        if(string.IsNullOrEmpty(Text))
        {
            return;
        }

        var dateStr = IsAutoCompleteAble(Text) ? AutoComplete(Text) : Text;
        var result = ParseToDate(dateStr);
        var parseSuccess = result.Item1;
        var date = result.Item2;
        if(!parseSuccess)
        {
            NotifyInputError("Text cannot be parsed to a valid date.");
            return;
        }
        WriteToTextInProperFormat(date);
    }
    private void WriteToTextInProperFormat(DateTime date)
    {
        if(string.IsNullOrEmpty(customFormat))
        {
            Text = date.IsMidnight()
                ? date.ToShortDateString()
                : date.ToString();
        }
        else
        {
            Text = date.ToString(customFormat);
        }
    }
    private (bool parseSucess, DateTime parsedDate) ParseToDate(string dateStr)
    {
        var defaultFormatParseSuccess = DateTime.TryParse(dateStr, out var date1);
        if(defaultFormatParseSuccess || string.IsNullOrEmpty(customFormat))
        {
            return (defaultFormatParseSuccess, date1);
        }
        var customFormatParseSuccess = DateTime.TryParseExact(dateStr,
            customFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date2);
        return (customFormatParseSuccess, date2);
    }
    private bool IsAutoCompleteAble(string dateStr)
    {
        var dateParts = dateStr.Split(DateTimeSeparator);
        if(dateParts.Length > 2)
        {
            return false;
        }

        var datePart = dateParts[0];
        if(datePart.Contains(DateSeparator))
        {
            if(datePart.Length > 10)
            {
                return false;
            }
        }
        else
        {
            if(datePart.Length > 8)
            {
                return false;
            }
        }
        if(dateParts.Length == 2 && dateParts[1].Length > 6)
        {
            return false;
        }

        return true;
    }
    public override object GetValue()
    {
        (bool parseSucess, DateTime parsedDate) = ParseToDate(Text);
        if(parseSucess)
        {
            return parsedDate;
        }
        return DBNull.Value;
    }
    private string AutoComplete(string text)
    {
        return new DateCompleter(
                dateFormat: DateFormat,
                dateSeparator: DateSeparator,
                timeSeparator: TimeSeparator,
                dateTimeSeparator: DateTimeSeparator,
                timeNowFunc: timeNowFunc
                )
            .AutoComplete(text);
    }
    protected override bool IsValidChar(char input)
    {
        return true;
    }
}
class DateCompleter
{
    private readonly string dateFormat;
    private readonly string dateSeparator;
    private readonly string timeSeparator;
    private readonly string dateTimeSeparator;
    private readonly Func<DateTime> timeNowFunc;
    public DateCompleter(string dateFormat, string dateSeparator,
        string timeSeparator, string dateTimeSeparator, Func<DateTime> timeNowFunc)
    {
        this.dateFormat = dateFormat;
        this.dateSeparator = dateSeparator;
        this.timeSeparator = timeSeparator;
        this.dateTimeSeparator = dateTimeSeparator;
        this.timeNowFunc = timeNowFunc;
    }
    public string AutoComplete(string text)
    {
        var dateAndTime = text.Split(dateTimeSeparator);
        var dateText = dateAndTime[0];
        var completeDate = AutoCompleteDate(dateText);
        if(dateAndTime.Length == 2)
        {
            var timeText = dateAndTime[1];
            var completeTime = AutoCompleteTime(timeText);
            return completeDate + dateTimeSeparator + completeTime;
        }
        return completeDate;
    }
    private string AutoCompleteTime(string incompleteTime)
    {
        if(incompleteTime.Contains(timeSeparator))
        {
            return CompleteTimeWithSeparators(incompleteTime);
        }
        return CompleteTimeWithoutSeparators(incompleteTime);
    }
    private string CompleteTimeWithoutSeparators(string incompleteTime)
    {
        switch(incompleteTime.Length)
        {
            case 1:
            case 2:
                return incompleteTime + timeSeparator +
                       "00" + timeSeparator +
                       "00";
            case 3:
            case 4:
                return incompleteTime.Substring(0, 2) + timeSeparator +
                       incompleteTime.Substring(2) + timeSeparator +
                       "00";
            default:
                return incompleteTime.Substring(0, 2) + timeSeparator +
                       incompleteTime.Substring(2, 2) + timeSeparator +
                       incompleteTime.Substring(4);
        }
    }
    private static string CompleteTimeWithSeparators(string incompleteTime)
    {
        var parseSuccess = DateTime.TryParse(incompleteTime, out var date);
        return parseSuccess ? date.ToShortTimeString() : incompleteTime;
    }
    private string AutoCompleteDate(string incompleteDate)
    {
        if(incompleteDate.Contains(dateSeparator) && (incompleteDate.Split(dateSeparator).Length - 1) == 2)
        {
            return CompleteDateWithSeparators(incompleteDate);
        }
        return CompleteDateWithoutSeparators(incompleteDate);
    }
    private  string CompleteDateWithSeparators(string incompleteDate)
    {
        var parseSuccess = DateTime.TryParse(incompleteDate, out var date);
        return parseSuccess ? date.ToShortDateString() : incompleteDate;
    }
    private string CompleteDateWithoutSeparators(string incompleteDate)
    {
        switch(incompleteDate.Length)
        {
            case 1:
            case 2:
                {
                    // assuming input is day.
                    return AddMonthAndYear(incompleteDate);
                }
            case 3:
            case 4:
                {
                    // assuming input is day and month in order specified by
                    // current culture
                    return AddYear(incompleteDate);
                }
            case 6:
                {
                    // assuming input is day and month in order specified by
                    // current culture followed by incomplete year (yy)
                    var incompleteWithSeparators = AddSeparators(incompleteDate);
                    return incompleteWithSeparators;
                }
            default:
                return AddSeparators(incompleteDate);
        }
    }
    private string AddMonthAndYear(string day)
    {
        var now = timeNowFunc.Invoke();
        var usDateString = $"{now.Month}/{day}/{now.Year}";
        var isValidDate = DateTime.TryParse(usDateString,
            CultureInfo.CreateSpecificCulture("en-US"),
            DateTimeStyles.None,
            out var date);
        return isValidDate ? date.ToShortDateString() : day;
    }
    private string AddYear(string dayAndMonth)
    {
        return dayAndMonth.Substring(0, 2).Replace(dateSeparator,"") + dateSeparator
              + dayAndMonth.Substring(2).Replace(dateSeparator, "") + dateSeparator
              + timeNowFunc.Invoke().Year;
    }
    private string AddSeparators(string incompleteDate)
    {
        var format = GetDoubleDayAndMonthFormat();
        var firstIndex = format.IndexOf(dateSeparator);
        var secondIndex = format.LastIndexOf(dateSeparator);
        var dateLength = incompleteDate.Length;
        if(firstIndex < dateLength & secondIndex >= dateLength)
        {
            return incompleteDate.Substring(0, firstIndex)
                            + dateSeparator
                            + incompleteDate.Substring(firstIndex);
        }
        if(firstIndex < dateLength & secondIndex < dateLength)
        {
            return incompleteDate.Substring(0, firstIndex)
                            + dateSeparator
                            + incompleteDate.Substring(firstIndex, secondIndex - firstIndex - 1)
                            + dateSeparator
                            + incompleteDate.Substring(secondIndex - 1);
        }
        return incompleteDate;
    }
    private string GetDoubleDayAndMonthFormat()
    {
        // dateFormat might be d/m/yyyy, this,
        // method makes sure we get dd/mm/yyyy
        var formatHasSingleDigitDayAndMonth = dateFormat.Length == 8;
        string format;
        if(formatHasSingleDigitDayAndMonth)
        {
            format = dateFormat.ToLower().Replace("d", "dd")
                .Replace("m", "mm");
        }
        else
        {
            format = dateFormat;
        }
        return format;
    }
}
static class DateExtensions
{
    public static bool IsIn21stOr20thCentury(this DateTime date)
    {
        return date.Year < 2100 && date.Year > 1900;
    }
    public static bool IsMidnight(this DateTime date)
    {
        return date.Hour == 0 && date.Minute == 0 && date.Second == 0;
    }
}
