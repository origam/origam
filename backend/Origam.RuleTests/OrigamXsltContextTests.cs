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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using Moq;
using NUnit.Framework;
using Origam.DA;
using Origam.Schema;
using Origam.Workbench.Services;
using Assert = NUnit.Framework.Assert;

namespace Origam.Rule.Tests
{
    [TestFixture]
    public class OrigamXsltContextTests
    {
        RuleEngine ruleEngine;
        private Mock<IBusinessServicesService> businessServiceMock;
        private Mock<IParameterService> parameterServiceMock;
            
        [SetUp]
        public void Init()
        {
            var dataLookupServiceMock = new Mock<IDataLookupService>();
            businessServiceMock = new Mock<IBusinessServicesService>();
            businessServiceMock
                .Setup(service => service.XslFunctionProviderServiceAgents)
                .Returns(new  List<IServiceAgent>());
            var stateMachineServiceMock = new Mock<IStateMachineService>();
            var tracingServiceMock = new Mock<ITracingService>();
            var documentationServiceMock = new Mock<IDocumentationService>();
            parameterServiceMock = new Mock<IParameterService>();

            // .GetParameterValue(name, OrigamDataType.String)
            ruleEngine = new RuleEngine(
                new Hashtable(), 
                null,
                new NullPersistenceService(),
                dataLookupServiceMock.Object,
                parameterServiceMock.Object,
                businessServiceMock.Object,
                stateMachineServiceMock.Object,
                tracingServiceMock.Object,
                documentationServiceMock.Object
            );
        }
        
        [Test]
        public void ShouldAddTime()
        {
            string xpath = "AS:AddMinutes('2022-07-13', 10)";
            TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime(2022,7,13));
            
            object expectedResult = "2022-07-13T00:10:00.0000000+" + offset.Hours.ToString("00") + ":00" ;
            XPathNavigator nav = new XmlDocument().CreateNavigator();
            XPathExpression expr = nav.Compile(xpath);

            OrigamXsltContext sut = new OrigamXsltContext(
                new NameTable(), 
                ruleEngine, 
                businessServiceMock.Object
            );
            expr.SetContext(sut);
            object result = nav.Evaluate(expr);
            Assert.That(result, Is.EqualTo(expectedResult));
        }        
        
        [Test]
        public void ShouldGetConstant()
        {
            string expectedResult = "constant1_value";
            string xpath = "AS:GetConstant('constant1')";
            
            XPathNavigator nav = new XmlDocument().CreateNavigator();
            XPathExpression expr = nav.Compile(xpath);
            
            parameterServiceMock
                .Setup(service => service.GetParameterValue("constant1", OrigamDataType.String, null))
                .Returns(expectedResult);
            OrigamXsltContext sut = new OrigamXsltContext(
                new NameTable(), 
                ruleEngine, 
                businessServiceMock.Object
            );
            expr.SetContext(sut);
            object result = nav.Evaluate(expr);
            Assert.That(result, Is.EqualTo(expectedResult));
        } 
        
        [TestCase("string1", new object[0], "string1_value")]
        [TestCase("string1 {0} {1}", new object[]{"1", "2"}, "string1_value 1 2")]
        [TestCase("string1 {0} {1} {2} {3}", new object[]{"1", "2", "3", "4"}, "string1_value 1 2 3 4")]
        public void ShouldGetString(string stringName, object[] args, string expectedResult)
        {
            string argsString = string.Join(
                ", ", 
                args.Cast<string>().Select(arg => $"'{arg}'"));
            if (argsString != "")
            {
                argsString = ", " + argsString;
            }

            string xpath = $"AS:GetString('{stringName}'{argsString})";
            
            XPathNavigator nav = new XmlDocument().CreateNavigator();
            XPathExpression expr = nav.Compile(xpath);

            parameterServiceMock
                .Setup(service => service.GetString(stringName, true, args))
                .Returns(expectedResult);
            OrigamXsltContext sut = new OrigamXsltContext(
                new NameTable(), 
                ruleEngine, 
                businessServiceMock.Object
            );
            expr.SetContext(sut);
            object result = nav.Evaluate(expr);
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}