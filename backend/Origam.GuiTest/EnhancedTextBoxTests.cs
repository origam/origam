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


namespace Origam.GuiTest
{
    [TestFixture]
    public class EnhancedTextBoxTests
    {

        [TestCase("0912","cs-CZ", "09.12.2017" )]
        [TestCase("0912","cs-CZ", "09.12.2017" )]
        [TestCase("09122020","cs-CZ", "09.12.2020" )]
        [TestCase("091220","cs-CZ", "09.12.2020" )]
        [TestCase("0912 1230","cs-CZ", "09.12.2017 12:30:00" )]
        [TestCase("0912 12","cs-CZ", "09.12.2017 12:00:00" )]
        [TestCase("09122020 123020","cs-CZ", "09.12.2020 12:30:20" )]
        
        [TestCase("129","en-US", "12/9/2017" )]
        [TestCase("12092020","en-US", "12/9/2020" )]
        [TestCase("120920","en-US", "12/9/2020" )]
        [TestCase("129 1230","en-US", "12/9/2017 12:30:00 PM" )]
        [TestCase("129 12","en-US", "12/9/2017 12:00:00 PM" )]
        [TestCase("1209 12","en-US", "12/9/2017 12:00:00 PM" )]
        [TestCase("12092020 123020","en-US", "12/9/2020 12:30:20 PM" )]
        
        [TestCase("Tuesday, December 5, 2017","en-US", "12/5/2017" )]
        
        [TestCase("5/5","en-US", "5/5/2017" )]
        [TestCase("5/5 14","en-US", "5/5/2017 2:00:00 PM" )]
        [TestCase("5/5 1430","en-US", "5/5/2017 2:30:00 PM" )]
        [TestCase("5/5 14:30","en-US", "5/5/2017 2:30:00 PM" )]
        [TestCase("5/5/16","en-US", "5/5/2016" )]
        
        [TestCase("5 ","en-US", "12/5/2017" )]
        [TestCase("5 ","cs-CZ", "05.12.2017" )]
        [TestCase("05 ","cs-CZ", "05.12.2017" )]
        
        [TestCase("05 ","zh", "2017/12/5" )]
        [TestCase("0503 ","zh", "2017/5/3" )]
        [TestCase("5/3 ","zh", "2017/5/3" )]
        [TestCase("20170503 ","zh", "2017/5/3" )]
        [TestCase("5/3 12","zh", "2017/5/3 12:00:00" )]
        public void ShouldAutoCompleteDateAndTime(string incompleteDate, 
            string culture, string expectedDate)
        {
            SetCulture(culture);

            var sut = new EnhancedTextBox(timeNowFunc: ()=> new DateTime(2017,12,14))
            {
                DataType = typeof(DateTime),
                Text = incompleteDate
            };

            TriggerLeaveEvent(sut);
           
            Assert.That(sut.Text,Is.EqualTo(expectedDate));
            Assert.That(sut.Value,Is.TypeOf<DateTime>());
        }


        [TestCase("dd. MM. yyyy HH:mm:ss.fff","21. 12. 2017 12:11:57.000","en-US")]
        [TestCase("dd. MM. yyyy HH:mm:ss.fff","21. 12. 2017 12:11:57.000","cs-CZ")]
        [TestCase("dd. MM. yyyy HH:mm:ss.fff","21. 12. 2017 12:11:57.000","zh")]
        public void ShouldParseDateInCustomFormatRegardlessOfCurrentCulture(string customFormat,
            string dateToParse,string culture)
        {
            SetCulture(culture);

            var sut = new EnhancedTextBox(timeNowFunc: ()=> new DateTime(2017,12,14))
            {
                DataType = typeof(DateTime),
                CustomFormat = customFormat,
                Text = dateToParse
            };
            TriggerLeaveEvent(sut);
            Assert.That(sut.Value,Is.TypeOf<DateTime>());
        }

        [TestCase("52","cs-CZ" )]
        [TestCase("0121","cs-CZ" )]
        [TestCase("521","cs-CZ" )]
        [TestCase("11111111111111","cs-CZ" )]
        
        [TestCase("52","en-US")] 
        [TestCase("521","en-US")] 
        [TestCase("11111111111111","en-US")] 
        [TestCase("Saturday, February 1, 2017","en-US")] // 2/1/2017 is not Saturday!
        
        [TestCase("20175503","zh")]
        //[TestCase("300503","zh")]
        public void ShouldFailToAutoCompleteDateAndTimeWithoutThrowingExceptions(
            string incompleteDate, string culture)
        {
            SetCulture(culture);

            var sut = new EnhancedTextBox
            {
                DataType = typeof(DateTime),
                Text = incompleteDate
            };

            TriggerLeaveEvent(sut);
            
            // no change in Text indicates failure to parse the date
            Assert.That(sut.Text,Is.EqualTo(incompleteDate)); 
            Assert.That(sut.Value,Is.EqualTo(DBNull.Value));
        }
        
        [TestCase(1000, typeof(int), "en-US", "1,000")] 
        [TestCase(-1000, typeof(int), "en-US", "-1,000")] 
        [TestCase("-1,000", typeof(int), "en-US", "-1,000")] 
        
        [TestCase(1000000, typeof(long), "en-US", "1,000,000")] 
        [TestCase("1,000,000", typeof(long), "en-US", "1,000,000")] 
        [TestCase(-1000000, typeof(long), "en-US", "-1,000,000")] 
        [TestCase("-1,000,000", typeof(long), "en-US", "-1,000,000")] 
        
        
        [TestCase(151513.258, typeof(float), "en-US", "151,513.3")]
        [TestCase(-151513.258, typeof(float), "en-US", "-151,513.3")] 
        [TestCase("-151,513.258", typeof(float), "en-US", "-151,513.3")]
        
        [TestCase(151513.258, typeof(double), "en-US", "151,513.258")]
        [TestCase(-151513.258, typeof(double), "en-US", "-151,513.258")] 
        [TestCase("-151,513.258", typeof(double), "en-US", "-151,513.258")] 
        
        [TestCase(151513.258, typeof(decimal), "en-US", "151,513.258")]
        [TestCase(-151513.258, typeof(decimal), "en-US", "-151,513.258")] 
        [TestCase("-151,513.258", typeof(decimal), "en-US", "-151,513.258")] 
        public void ShouldFormatNumber(object input, Type inpType,
            string culture, string expectedText)
        {
            SetCulture(culture);

            var sut = new EnhancedTextBox
            {
                DataType = inpType,
                Text = input.ToString()
            };
            Console.WriteLine(sut.Text);
            TriggerLeaveEvent(sut);
           
            Assert.That(sut.Text,Is.EqualTo(expectedText));
            Assert.True(sut.Value.GetType() == inpType);
        }

        private void SetCulture(string culture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture); 
        }

        private static void TriggerLeaveEvent(EnhancedTextBox sut)
        {
            var dynMethod = sut.GetType().GetMethod(
                "OnLeave",
                BindingFlags.NonPublic | BindingFlags.Instance,
                Type.DefaultBinder,
                new[] {typeof(EventArgs)},
                null
            );

            dynMethod.Invoke(sut, new object[] {EventArgs.Empty});
        }
    }
}