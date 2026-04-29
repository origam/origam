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
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Origam.Gui.UI;

namespace Origam.GuiTest;

[TestFixture]
public class EnhancedTextBoxTests
{
    [TestCase(arg1: "0912", arg2: "cs-CZ", arg3: "09.12.2017")]
    [TestCase(arg1: "0912", arg2: "cs-CZ", arg3: "09.12.2017")]
    [TestCase(arg1: "09122020", arg2: "cs-CZ", arg3: "09.12.2020")]
    [TestCase(arg1: "091220", arg2: "cs-CZ", arg3: "09.12.2020")]
    [TestCase(arg1: "0912 1230", arg2: "cs-CZ", arg3: "09.12.2017 12:30:00")]
    [TestCase(arg1: "0912 12", arg2: "cs-CZ", arg3: "09.12.2017 12:00:00")]
    [TestCase(arg1: "09122020 123020", arg2: "cs-CZ", arg3: "09.12.2020 12:30:20")]
    [TestCase(arg1: "129", arg2: "en-US", arg3: "12/9/2017")]
    [TestCase(arg1: "12092020", arg2: "en-US", arg3: "12/9/2020")]
    [TestCase(arg1: "120920", arg2: "en-US", arg3: "12/9/2020")]
    [TestCase(arg1: "129 1230", arg2: "en-US", arg3: "12/9/2017 12:30:00 PM")]
    [TestCase(arg1: "129 12", arg2: "en-US", arg3: "12/9/2017 12:00:00 PM")]
    [TestCase(arg1: "1209 12", arg2: "en-US", arg3: "12/9/2017 12:00:00 PM")]
    [TestCase(arg1: "12092020 123020", arg2: "en-US", arg3: "12/9/2020 12:30:20 PM")]
    [TestCase(arg1: "Tuesday, December 5, 2017", arg2: "en-US", arg3: "12/5/2017")]
    [TestCase(arg1: "5/5", arg2: "en-US", arg3: "5/5/2017")]
    [TestCase(arg1: "5/5 14", arg2: "en-US", arg3: "5/5/2017 2:00:00 PM")]
    [TestCase(arg1: "5/5 1430", arg2: "en-US", arg3: "5/5/2017 2:30:00 PM")]
    [TestCase(arg1: "5/5 14:30", arg2: "en-US", arg3: "5/5/2017 2:30:00 PM")]
    [TestCase(arg1: "5/5/16", arg2: "en-US", arg3: "5/5/2016")]
    [TestCase(arg1: "5 ", arg2: "en-US", arg3: "12/5/2017")]
    [TestCase(arg1: "5 ", arg2: "cs-CZ", arg3: "05.12.2017")]
    [TestCase(arg1: "05 ", arg2: "cs-CZ", arg3: "05.12.2017")]
    [TestCase(arg1: "05 ", arg2: "zh", arg3: "2017/12/5")]
    [TestCase(arg1: "0503 ", arg2: "zh", arg3: "2017/5/3")]
    [TestCase(arg1: "5/3 ", arg2: "zh", arg3: "2017/5/3")]
    [TestCase(arg1: "20170503 ", arg2: "zh", arg3: "2017/5/3")]
    [TestCase(arg1: "5/3 12", arg2: "zh", arg3: "2017/5/3 12:00:00")]
    public void ShouldAutoCompleteDateAndTime(
        string incompleteDate,
        string culture,
        string expectedDate
    )
    {
        SetCulture(culture: culture);
        var sut = new EnhancedTextBox(timeNowFunc: () =>
            new DateTime(year: 2017, month: 12, day: 14)
        )
        {
            DataType = typeof(DateTime),
            Text = incompleteDate,
        };
        TriggerLeaveEvent(sut: sut);

        Assert.That(actual: sut.Text, expression: Is.EqualTo(expected: expectedDate));
        Assert.That(actual: sut.Value, expression: Is.TypeOf<DateTime>());
    }

    [TestCase(arg1: "dd. MM. yyyy HH:mm:ss.fff", arg2: "21. 12. 2017 12:11:57.000", arg3: "en-US")]
    [TestCase(arg1: "dd. MM. yyyy HH:mm:ss.fff", arg2: "21. 12. 2017 12:11:57.000", arg3: "cs-CZ")]
    [TestCase(arg1: "dd. MM. yyyy HH:mm:ss.fff", arg2: "21. 12. 2017 12:11:57.000", arg3: "zh")]
    public void ShouldParseDateInCustomFormatRegardlessOfCurrentCulture(
        string customFormat,
        string dateToParse,
        string culture
    )
    {
        SetCulture(culture: culture);
        var sut = new EnhancedTextBox(timeNowFunc: () =>
            new DateTime(year: 2017, month: 12, day: 14)
        )
        {
            DataType = typeof(DateTime),
            CustomFormat = customFormat,
            Text = dateToParse,
        };
        TriggerLeaveEvent(sut: sut);
        Assert.That(actual: sut.Value, expression: Is.TypeOf<DateTime>());
    }

    [TestCase(arg1: "52", arg2: "cs-CZ")]
    [TestCase(arg1: "0121", arg2: "cs-CZ")]
    [TestCase(arg1: "521", arg2: "cs-CZ")]
    [TestCase(arg1: "11111111111111", arg2: "cs-CZ")]
    [TestCase(arg1: "52", arg2: "en-US")]
    [TestCase(arg1: "521", arg2: "en-US")]
    [TestCase(arg1: "11111111111111", arg2: "en-US")]
    [TestCase(arg1: "Saturday, February 1, 2017", arg2: "en-US")] // 2/1/2017 is not Saturday!
    [TestCase(arg1: "20175503", arg2: "zh")]
    //[TestCase("300503","zh")]
    public void ShouldFailToAutoCompleteDateAndTimeWithoutThrowingExceptions(
        string incompleteDate,
        string culture
    )
    {
        SetCulture(culture: culture);
        var sut = new EnhancedTextBox { DataType = typeof(DateTime), Text = incompleteDate };
        TriggerLeaveEvent(sut: sut);

        // no change in Text indicates failure to parse the date
        Assert.That(actual: sut.Text, expression: Is.EqualTo(expected: incompleteDate));
        Assert.That(actual: sut.Value, expression: Is.EqualTo(expected: DBNull.Value));
    }

    [TestCase(arguments: new object[] { 1000, typeof(int), "en-US", "1,000" })]
    [TestCase(arguments: new object[] { -1000, typeof(int), "en-US", "-1,000" })]
    [TestCase(arguments: new object[] { "-1,000", typeof(int), "en-US", "-1,000" })]
    [TestCase(arguments: new object[] { 1000000, typeof(long), "en-US", "1,000,000" })]
    [TestCase(arguments: new object[] { "1,000,000", typeof(long), "en-US", "1,000,000" })]
    [TestCase(arguments: new object[] { -1000000, typeof(long), "en-US", "-1,000,000" })]
    [TestCase(arguments: new object[] { "-1,000,000", typeof(long), "en-US", "-1,000,000" })]
    [TestCase(arguments: new object[] { 151513.258, typeof(float), "en-US", "151,513.3" })]
    [TestCase(arguments: new object[] { -151513.258, typeof(float), "en-US", "-151,513.3" })]
    [TestCase(arguments: new object[] { "-151,513.258", typeof(float), "en-US", "-151,513.3" })]
    [TestCase(arguments: new object[] { 151513.258, typeof(double), "en-US", "151,513.258" })]
    [TestCase(arguments: new object[] { -151513.258, typeof(double), "en-US", "-151,513.258" })]
    [TestCase(arguments: new object[] { "-151,513.258", typeof(double), "en-US", "-151,513.258" })]
    [TestCase(arguments: new object[] { 151513.258, typeof(decimal), "en-US", "151,513.258" })]
    [TestCase(arguments: new object[] { -151513.258, typeof(decimal), "en-US", "-151,513.258" })]
    [TestCase(arguments: new object[] { "-151,513.258", typeof(decimal), "en-US", "-151,513.258" })]
    public void ShouldFormatNumber(object input, Type inpType, string culture, string expectedText)
    {
        SetCulture(culture: culture);
        var sut = new EnhancedTextBox { DataType = inpType, Text = input.ToString() };
        Console.WriteLine(value: sut.Text);
        TriggerLeaveEvent(sut: sut);

        Assert.That(actual: sut.Text, expression: Is.EqualTo(expected: expectedText));
        Assert.True(condition: sut.Value.GetType() == inpType);
    }

    private void SetCulture(string culture)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo(name: culture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(name: culture);
    }

    private static void TriggerLeaveEvent(EnhancedTextBox sut)
    {
        var dynMethod = sut.GetType()
            .GetMethod(
                name: "OnLeave",
                bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
                binder: Type.DefaultBinder,
                types: new[] { typeof(EventArgs) },
                modifiers: null
            );
        dynMethod.Invoke(obj: sut, parameters: new object[] { EventArgs.Empty });
    }
}
