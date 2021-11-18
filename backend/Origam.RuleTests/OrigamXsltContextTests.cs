#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;

namespace Origam.Rule.Tests
{
    [TestClass()]
    public class OrigamXsltContextTests
    {
        [TestMethod()]
        public void FormatXmlDateTimeTest()
        {
            string inputTime = "2014-01-01";
            DateTime inputTimeDateTime = XmlConvert.ToDateTime(inputTime, XmlDateTimeSerializationMode.Local);
            string outtime1 = XmlConvert.ToString(inputTimeDateTime, XmlDateTimeSerializationMode.Local);
            string outtime2 = XmlConvert.ToString(inputTimeDateTime, XmlDateTimeSerializationMode.Utc);
            string outtime3 = XmlConvert.ToString(inputTimeDateTime, XmlDateTimeSerializationMode.RoundtripKind);
            string outtime4 = XmlConvert.ToString(inputTimeDateTime, XmlDateTimeSerializationMode.Unspecified);
            //string outtime5 = OrigamXsltContext.FormatXmlDateTime(inputTimeDateTime);
            string outtime6 = XmlConvert.ToString(inputTimeDateTime);
            //Assert.AreEqual(outtime5, outtime6);

            string inputTime2 = "2014-01-01T00:00:00.000023+0400";
            DateTime inputTimeDateTime2 = XmlConvert.ToDateTime(inputTime2);
            string outtime11 = XmlConvert.ToString(inputTimeDateTime2, XmlDateTimeSerializationMode.Local);
            string outtime12 = XmlConvert.ToString(inputTimeDateTime2, XmlDateTimeSerializationMode.Utc);
            string outtime13 = XmlConvert.ToString(inputTimeDateTime2, XmlDateTimeSerializationMode.RoundtripKind);
            string outtime14 = XmlConvert.ToString(inputTimeDateTime2, XmlDateTimeSerializationMode.Unspecified);
            //string outtime15 = OrigamXsltContext.FormatXmlDateTime(inputTimeDateTime2);
            string outtime16 = XmlConvert.ToString(inputTimeDateTime2);
            //Assert.AreEqual(outtime15, outtime16);
        }
    }
}