#region license

/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.

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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using Moq;
using NUnit.Framework;
using Origam.DA;
using Origam.Rule;
using Origam.Rule.Xslt;
using Origam.Rule.XsltFunctions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Service.Core;
using Origam.Workbench;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;
using Assert = NUnit.Framework.Assert;

namespace Origam.RuleTests;

[TestFixture]
public class XsltTests
{
    private Mock<IBusinessServicesService> businessServiceMock;
    private Mock<IParameterService> parameterServiceMock;
    private Mock<IDataLookupService> lookupServiceMock;
    private Mock<IOrigamAuthorizationProvider> authorizationProvider;
    private Mock<Func<UserProfile>> userProfileGetterMock;
    private Mock<ICounter> counterMock;
    private Mock<IStateMachineService> stateMachineServiceMock;
    private Mock<IDocumentationService> documentationServiceMock;
    private Mock<ITracingService> tracingServiceMock;
    private Mock<IXsltFunctionSchemaItemProvider> functionSchemaItemProvider;
    private Mock<IPersistenceService> persistenceServiceMock;
    private Mock<ICoreDataService> dataServiceMock;
    private List<XsltFunctionsDefinition> xsltFunctionDefinitions;
    private Mock<IXpathEvaluator> xPathEvaluatorMock;
    private Mock<IHttpTools> httpToolsMock;
    private Mock<IResourceTools> resourceToolsMock;

    private string xsltScriptTemplate =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
        + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n"
        + "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n"
        + "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n"
        + "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n"
        + "    xmlns:GE=\"http://xsl.origam.com/geo\"\n"
        + "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n"
        + "	<xsl:template match=\"ROOT\">\n"
        + "		<ROOT>\n"
        + "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n"
        + "				<xsl:attribute name=\"d1\">\n"
        + "                  <xsl:value-of select=\"{0}\"/>\n"
        + "              </xsl:attribute>\n"
        + "			</SD>\n"
        + "		</ROOT>\n"
        + "	</xsl:template>\n"
        + "</xsl:stylesheet>\n";

    [SetUp]
    public void Init()
    {
        lookupServiceMock = new Mock<IDataLookupService>();
        businessServiceMock = new Mock<IBusinessServicesService>();
        stateMachineServiceMock = new Mock<IStateMachineService>();
        tracingServiceMock = new Mock<ITracingService>();
        documentationServiceMock = new Mock<IDocumentationService>();
        parameterServiceMock = new Mock<IParameterService>();
        authorizationProvider = new Mock<IOrigamAuthorizationProvider>();
        userProfileGetterMock = new Mock<Func<UserProfile>>();
        persistenceServiceMock = new Mock<IPersistenceService>();
        dataServiceMock = new Mock<ICoreDataService>();
        counterMock = new Mock<ICounter>();
        functionSchemaItemProvider = new Mock<IXsltFunctionSchemaItemProvider>();
        xPathEvaluatorMock = new Mock<IXpathEvaluator>();
        httpToolsMock = new Mock<IHttpTools>();
        resourceToolsMock = new Mock<IResourceTools>();

        var asCollection = new XsltFunctionCollection();
        asCollection.AssemblyName = "Origam.Rule";
        asCollection.FullClassName = "Origam.Rule.XsltFunctions.LegacyXsltFunctionContainer";
        asCollection.XslNameSpaceUri = "http://schema.advantages.cz/AsapFunctions";
        asCollection.XslNameSpacePrefix = "AS";

        var geCollection = new XsltFunctionCollection();
        geCollection.AssemblyName = "Origam.Rule";
        geCollection.FullClassName = "Origam.Rule.XsltFunctions.OrigamGeoContainer";
        geCollection.XslNameSpaceUri = "http://xsl.origam.com/geo";
        geCollection.XslNameSpacePrefix = "GE";
        functionSchemaItemProvider
            .Setup(expression: x =>
                x.ChildItemsByType<XsltFunctionCollection>(XsltFunctionCollection.CategoryConst)
            )
            .Returns(value: new List<XsltFunctionCollection> { asCollection, geCollection });

        xsltFunctionDefinitions = XsltFunctionContainerFactory
            .Create(
                businessService: businessServiceMock.Object,
                xsltFunctionSchemaItemProvider: functionSchemaItemProvider.Object,
                persistence: persistenceServiceMock.Object,
                lookupService: lookupServiceMock.Object,
                parameterService: parameterServiceMock.Object,
                stateMachineService: stateMachineServiceMock.Object,
                tracingService: tracingServiceMock.Object,
                documentationService: documentationServiceMock.Object,
                dataService: dataServiceMock.Object,
                authorizationProvider: authorizationProvider.Object,
                userProfileGetter: userProfileGetterMock.Object,
                xpathEvaluator: xPathEvaluatorMock.Object,
                httpTools: httpToolsMock.Object,
                resourceTools: resourceToolsMock.Object,
                transactionId: null
            )
            .ToList();
    }

    private object RunInXpath(string xsltCall, XmlDocument document = null)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo(name: "cs-CZ");
        XPathNavigator nav = (document ?? new XmlDocument()).CreateNavigator();
        XPathExpression expr = nav.Compile(xpath: xsltCall);
        OrigamXsltContext sut = new OrigamXsltContext(
            nt: new NameTable(),
            xsltFunctionsDefinitions: xsltFunctionDefinitions
        );
        expr.SetContext(nsManager: sut);
        return nav.Evaluate(expr: expr);
    }

    private string RunInXslt(string xsltCall, XmlDocument document = null, string xsltScript = null)
    {
        xsltScript ??= string.Format(format: xsltScriptTemplate, arg0: xsltCall);

        var transformer = new CompiledXsltEngine(functionsDefinitions: xsltFunctionDefinitions);
        if (document == null)
        {
            document = new XmlDocument();
            document.LoadXml(xml: "<ROOT></ROOT>");
        }

        XmlContainer xmlContainer = new XmlContainer(xmlDocument: document);
        IXmlContainer resultContainer = transformer.Transform(
            data: xmlContainer,
            xsl: xsltScript,
            parameters: new Hashtable(),
            transactionId: null,
            outputStructure: null,
            validateOnly: false
        );

        var regex = new Regex(pattern: "d1=\"(.*)\"");
        var match = regex.Match(input: resultContainer.Xml.OuterXml);
        return match.Success ? match.Groups[groupnum: 1].Value : "";
    }

    [Test]
    public void ShouldGetConstant()
    {
        string expectedResult = "constant1_value";
        string xsltCall = "AS:GetConstant('constant1')";
        parameterServiceMock
            .Setup(expression: service =>
                service.GetParameterValue("constant1", OrigamDataType.String, null)
            )
            .Returns(value: expectedResult);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));

        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arg1: "string1", arg2: new object[0], arg3: "string1_value")]
    [TestCase(arg1: "string1 {0} {1}", arg2: new object[] { "1", "2" }, arg3: "string1_value 1 2")]
    [TestCase(
        arg1: "string1 {0} {1} {2}",
        arg2: new object[] { "1", "2", "3" },
        arg3: "string1_value 1 2 3"
    )]
    [TestCase(
        arg1: "string1 {0} {1} {2} {3}",
        arg2: new object[] { "1", "2", "3", "4" },
        arg3: "string1_value 1 2 3 4"
    )]
    [TestCase(
        arg1: "string1 {0} {1} {2} {3} {4}",
        arg2: new object[] { "1", "2", "3", "4", "5" },
        arg3: "string1_value 1 2 3 4 5"
    )]
    [TestCase(
        arg1: "string1 {0} {1} {2} {3} {4} {5}",
        arg2: new object[] { "1", "2", "3", "4", "5", "6" },
        arg3: "string1_value 1 2 3 4 5 6"
    )]
    [TestCase(
        arg1: "string1 {0} {1} {2} {3} {4} {5} {6}",
        arg2: new object[] { "1", "2", "3", "4", "5", "6", "7" },
        arg3: "string1_value 1 2 3 4 5 6 7"
    )]
    [TestCase(
        arg1: "string1 {0} {1} {2} {3} {4} {5} {6} {7}",
        arg2: new object[] { "1", "2", "3", "4", "5", "6", "7", "8" },
        arg3: "string1_value 1 2 3 4 5 6 7 8"
    )]
    [TestCase(
        arg1: "string1 {0} {1} {2} {3} {4} {5} {6} {7} {8}",
        arg2: new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" },
        arg3: "string1_value 1 2 3 4 5 6 7 8 9"
    )]
    [TestCase(
        arg1: "string1 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
        arg2: new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" },
        arg3: "string1_value 1 2 3 4 5 6 7 8 9 10"
    )]
    public void ShouldGetString(string stringName, object[] args, string expectedResult)
    {
        string argsString = string.Join(
            separator: ", ",
            values: args.Cast<string>().Select(selector: arg => $"'{arg}'")
        );
        if (argsString != "")
        {
            argsString = ", " + argsString;
        }

        string xsltCall = $"AS:GetString('{stringName}'{argsString})";

        parameterServiceMock
            .Setup(expression: service => service.GetString(stringName, true, args))
            .Returns(value: expectedResult);
        parameterServiceMock
            .Setup(expression: service => service.GetString(stringName, args))
            .Returns(value: expectedResult);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldRunNumberOperand()
    {
        string xsltCall = "AS:NumberOperand('1', '1', 'PLUS')";
        object expectedResult = "2";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arguments: new object[] { "Plus", "1", "1", "2" })]
    [TestCase(arguments: new object[] { "Minus", "1", "1", "0" })]
    [TestCase(arguments: new object[] { "Div", "4", "2", "2" })]
    [TestCase(arguments: new object[] { "Mul", "2", "2", "4" })]
    [TestCase(arguments: new object[] { "Mod", "5", "2", "1" })]
    public void ShouldRunMathFunctions(
        string functionName,
        string parameter1,
        string parameter2,
        string expectedResult
    )
    {
        string xsltCall = $"AS:{functionName}('{parameter1}', '{parameter2}')";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldFormatNumber()
    {
        string xsltCall = "AS:FormatNumber('1.54876', 'E')";
        object expectedResult = "1,548760E+000";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arg1: "MinString", arg2: "a")]
    [TestCase(arg1: "MaxString", arg2: "w")]
    public void ShouldTestStringOperationFunctions(string functionName, string expectedResult)
    {
        string xsltCall = $"AS:{functionName}(/ROOT/N1/@name)";

        var document = new XmlDocument();
        document.LoadXml(
            xml: "<ROOT><N1 name=\"w\"></N1><N1 name=\"a\"></N1><N1  name=\"e\"></N1></ROOT>"
        );

        object xPathResult = RunInXpath(xsltCall: xsltCall, document: document);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall, document: document);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arg1: "AS:LookupValue('{0}', '{1}')", arg2: new[] { "lookupValue" })]
    [TestCase(
        arg1: "AS:LookupValue('{0}', '{1}', '{2}', '{3}', '{4}')",
        arg2: new[] { "par1", "val1", "par2", "val2" }
    )]
    [TestCase(
        arg1: "AS:LookupValue('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
        arg2: new[] { "par1", "val1", "par2", "val2", "par3", "val3" }
    )]
    public void ShouldLookupValue(string xpathTemplate, string[] args)
    {
        string lookupId = "45b07cce-3d02-448c-afca-1b6f1eb158b5";
        var formatArguments = new List<object> { lookupId };
        formatArguments.AddRange(collection: args);
        string xsltCall = string.Format(format: xpathTemplate, args: formatArguments.ToArray());
        object expectedResult = "lookupResult";

        var paramTable = new Dictionary<string, object>(capacity: 3);
        if (args.Length >= 4)
        {
            paramTable[key: args[0]] = args[1];
            paramTable[key: args[2]] = args[3];
        }

        if (args.Length >= 6)
        {
            paramTable[key: args[4]] = args[5];
        }

        if (args.Length == 1)
        {
            lookupServiceMock
                .Setup(expression: service =>
                    service.GetDisplayText(Guid.Parse(lookupId), args[0], false, false, null)
                )
                .Returns(value: expectedResult);
        }
        else
        {
            lookupServiceMock
                .Setup(expression: service =>
                    service.GetDisplayText(Guid.Parse(lookupId), paramTable, false, false, null)
                )
                .Returns(value: expectedResult);
        }

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arg1: "AS:LookupList('{0}')", arg2: new string[0])]
    [TestCase(arg1: "AS:LookupList('{0}', '{1}', '{2}')", arg2: new[] { "par1", "val1" })]
    [TestCase(
        arg1: "AS:LookupList('{0}', '{1}', '{2}', '{3}', '{4}')",
        arg2: new[] { "par1", "val1", "par2", "val2" }
    )]
    [TestCase(
        arg1: "AS:LookupList('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
        arg2: new[] { "par1", "val1", "par2", "val2", "par3", "val3" }
    )]
    public void ShouldTestLookupList(string xsltTemplate, string[] args)
    {
        string lookupId = "f62d6323-92ec-4c67-8fed-0e48d30dae8f";
        var argList = new List<object> { lookupId };
        argList.AddRange(collection: args);

        string xsltCall = string.Format(format: xsltTemplate, args: argList.ToArray());
        string expectedResult = "testValue";

        var dataTable = new DataTable();
        var column = dataTable.Columns.Add(columnName: "Test", type: typeof(string));
        column.ColumnMapping = MappingType.Element;
        var dataRow = dataTable.NewRow();
        dataRow[columnName: "Test"] = expectedResult;
        dataTable.Rows.Add(row: dataRow);
        dataTable.AcceptChanges();
        var dataView = new DataView(table: dataTable);

        var parameters = new Dictionary<string, object>();
        if (args.Length >= 2)
        {
            parameters[key: args[0]] = args[1];
        }

        if (args.Length >= 4)
        {
            parameters[key: args[2]] = args[3];
        }

        if (args.Length >= 6)
        {
            parameters[key: args[4]] = args[5];
        }

        lookupServiceMock
            .Setup(expression: x => x.GetList(new Guid(lookupId), parameters, null))
            .Returns(value: dataView);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        XPathNodeIterator iterator = (XPathNodeIterator)xPathResult;
        iterator.MoveNext();
        string resultXml = iterator.Current.OuterXml;
        var regex = new Regex(pattern: "<Test>(\\w+)</Test>");
        var match = regex.Match(input: resultXml);
        Assert.That(actual: match.Success, expression: Is.True);
        Assert.That(
            actual: match.Groups[groupnum: 1].Value,
            expression: Is.EqualTo(expected: expectedResult)
        );

        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arg: "AS:LookupValueEx('{0}', $parameters1)")]
    [TestCase(arg: "AS:LookupOrCreateEx('{0}', $parameters1, $parameters1)")]
    public void ShouldTestLookupValueEx(string xsltCallTemplate)
    {
        string lookupId = "f62d6323-92ec-4c67-8fed-0e48d30dae8f";
        string xsltCall = string.Format(format: xsltCallTemplate, arg0: lookupId);
        string expectedResult = "testValue";

        string xsltScriptTemplate =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n"
            + "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n"
            + "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n"
            + "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n"
            + "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n"
            + "	<xsl:template match=\"ROOT\">\n"
            + "       <xsl:variable name=\"parameters1\">"
            + "           <parameter key=\"par1\" value=\"val1\"/>\n"
            + "       </xsl:variable>\n"
            + "		<ROOT>\n"
            + "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n"
            + "				<xsl:attribute name=\"d1\">\n"
            + $"                  <xsl:value-of select=\"{xsltCall}\"/>\n"
            + "              </xsl:attribute>\n"
            + "			</SD>\n"
            + "		</ROOT>\n"
            + "	</xsl:template>\n"
            + "</xsl:stylesheet>\n";

        var document = new XmlDocument();
        document.LoadXml(xml: "<ROOT></ROOT>");

        var parameters = new Dictionary<string, object> { [key: "par1"] = "val1" };
        lookupServiceMock
            .Setup(expression: service =>
                service.GetDisplayText(Guid.Parse(lookupId), parameters, false, false, null)
            )
            .Returns(value: expectedResult);

        // Xpath test is omitted because this function is not useful in xpath.

        string xsltResult = RunInXslt(
            xsltCall: "",
            document: document,
            xsltScript: xsltScriptTemplate
        );
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestLookupOrCreate()
    {
        string recordId = "f6fe6efe-8b95-4c78-8bb2-bd8d6eadc780";
        string lookupId = "45b07cce-3d02-448c-afca-1b6f1eb158b5";
        string xsltCall = $"AS:LookupOrCreate('{lookupId}', '{recordId}', /ROOT)";
        object expectedResult = "1";

        lookupServiceMock
            .Setup(expression: service =>
                service.GetDisplayText(Guid.Parse(lookupId), recordId, false, false, null)
            )
            .Returns(value: expectedResult);

        // Xpath test is omitted because this function is not useful in xpath.

        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arg1: "AS:NormalRound(1.54876, 2)", arg2: "1.55")]
    [TestCase(arg1: "AS:Round(1.54876)", arg2: "2")]
    [TestCase(arg1: "AS:Ceiling(1.54876)", arg2: "2")]
    public void ShouldRoundNumber(string xsltCall, object expectedResult)
    {
        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    // ResizeImage(string inputData, int width, int height, string keepAspectRatio, string outFormat)

    private const string resizedImage1 =
        "iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAAAXNSR0IArs4c6QA"
        + "AAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAmSURBVBhXY/"
        + "j3798KID4PwyCBm/+RAEjgIpQNBiCBy1A2EPz/DwAwcj5F+ErgqwAAAABJRU5Er"
        + "kJgggAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        + "AAAAAAAAAAAAAAAAAAAAAAAAAAA==";

    private const string resizedImage2 =
        "iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAIAAAAmkwkpAAAAAXNSR0IArs4c6QAA"
        + "AARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiUAABYlAUlSJPAAAAAlSURBVBhXY1ixY"
        + "sV5GGC4efPmfxhguHjxIpQJ5Fy+fBnK/P8fADtAK5y4oNB3AAAAAElFTkSuQmCCAA"
        + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        + "AAAAAAAAAAAAAAAA==";

    private const string resizedImage1Linux =
        "iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAABGdBTUEAALGPC/xhBQ"
        + "AAAAFzUkdCAK7OHOkAAAAgY0hSTQAAeiYAAICEAAD6AAAAgOgAAHUwAADqYAAAOpgAA"
        + "BdwnLpRPAAAACFJREFUCJlj/P///38GKPj///9/JgY0gCHAgqyFgYGBAQAVfgv+xs48"
        + "NQAAAABJRU5ErkJgggAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        + "AAAAAAAA==";
    private const string resizedImage2Linux =
        "iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAIAAAAmkwkpAAAABGdBTUEAALGPC/xhBQ"
        + "AAAAFzUkdCAK7OHOkAAAAgY0hSTQAAeiYAAICEAAD6AAAAgOgAAHUwAADqYAAAOpgAA"
        + "BdwnLpRPAAAACFJREFUCJlj/P//PwMDAwMDw////5kYkAAKhwWujIGBAQAx+wkBrIw4"
        + "XAAAAABJRU5ErkJgggAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        + "AAAAAAAA==";

    // white square 6x6, png format.
    private byte[] image =
    {
        137,
        80,
        78,
        71,
        13,
        10,
        26,
        10,
        0,
        0,
        0,
        13,
        73,
        72,
        68,
        82,
        0,
        0,
        0,
        6,
        0,
        0,
        0,
        6,
        8,
        2,
        0,
        0,
        0,
        111,
        174,
        120,
        31,
        0,
        0,
        0,
        1,
        115,
        82,
        71,
        66,
        0,
        174,
        206,
        28,
        233,
        0,
        0,
        0,
        4,
        103,
        65,
        77,
        65,
        0,
        0,
        177,
        143,
        11,
        252,
        97,
        5,
        0,
        0,
        0,
        9,
        112,
        72,
        89,
        115,
        0,
        0,
        22,
        37,
        0,
        0,
        22,
        37,
        1,
        73,
        82,
        36,
        240,
        0,
        0,
        0,
        23,
        73,
        68,
        65,
        84,
        24,
        87,
        99,
        252,
        255,
        255,
        63,
        3,
        42,
        96,
        130,
        210,
        72,
        128,
        122,
        66,
        12,
        12,
        0,
        81,
        221,
        3,
        9,
        217,
        253,
        155,
        178,
        0,
        0,
        0,
        0,
        73,
        69,
        78,
        68,
        174,
        66,
        96,
        130,
    };

    [TestCase("AS:ResizeImage('{0}', '4', '4')", resizedImage1)]
    [TestCase("AS:ResizeImage('{0}', '4', '4', 'true', 'png')", resizedImage2)]
    [Platform(Include = "Win")]
    public void ShouldConvertImage(string xsltCallTemplate, string expectedResult)
    {
        string xsltCall = string.Format(xsltCallTemplate, Convert.ToBase64String(image));
        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase(arg1: "AS:ResizeImage('{0}', '4', '4')", arg2: resizedImage1Linux)]
    [TestCase(arg1: "AS:ResizeImage('{0}', '4', '4', 'true', 'png')", arg2: resizedImage2Linux)]
    [Platform(Include = "Linux")]
    public void ShouldConvertImageLinux(string xsltCallTemplate, string expectedResult)
    {
        string xsltCall = string.Format(
            format: xsltCallTemplate,
            arg0: Convert.ToBase64String(inArray: image)
        );
        object xPathResult = RunInXpath(xsltCall: xsltCall);
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldGetImageDimensions()
    {
        string xsltCall = $"AS:GetImageDimensions('{Convert.ToBase64String(inArray: image)}')";
        string expectedResult = "6";

        string xsltScriptTemplate =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n"
            + "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n"
            + "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n"
            + "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n"
            + "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n"
            + "	<xsl:template match=\"ROOT\">\n"
            + "		<ROOT>\n"
            + "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n"
            + "				<xsl:attribute name=\"d1\">\n"
            + $"                  <xsl:value-of select=\"{xsltCall}/ROOT/Dimensions/@Width\"/>\n"
            + "              </xsl:attribute>\n"
            + "			</SD>\n"
            + "		</ROOT>\n"
            + "	</xsl:template>\n"
            + "</xsl:stylesheet>\n";

        var document = new XmlDocument();
        document.LoadXml(xml: "<ROOT></ROOT>");

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        XPathNodeIterator iterator = (XPathNodeIterator)xPathResult;
        iterator.MoveNext();
        string resultXml = iterator.Current.OuterXml;
        var regex = new Regex(pattern: "Width=\"(\\d)\" Height=\"(\\d)\"");
        var match = regex.Match(input: resultXml);
        Assert.That(actual: match.Success, expression: Is.True);
        Assert.That(
            actual: match.Groups[groupnum: 1].Value,
            expression: Is.EqualTo(expected: expectedResult)
        );
        Assert.That(
            actual: match.Groups[groupnum: 2].Value,
            expression: Is.EqualTo(expected: expectedResult)
        );
        string xsltResult = RunInXslt(
            xsltCall: "",
            document: document,
            xsltScript: xsltScriptTemplate
        );
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldDoOrigamRound()
    {
        string xsltCall = $"AS:OrigamRound('1.569456', 'rounding1')";
        object expectedResult = "2";

        lookupServiceMock
            .Setup(expression: service =>
                service.GetDisplayText(
                    Guid.Parse("7d3d6933-648b-42cb-8947-0d2cb700152b"),
                    "rounding1",
                    null
                )
            )
            .Returns(value: 1m);
        lookupServiceMock
            .Setup(expression: service =>
                service.GetDisplayText(
                    Guid.Parse("994608ad-9634-439b-975a-484067f5b5a6"),
                    "rounding1",
                    false,
                    false,
                    null
                )
            )
            .Returns(value: "9ecc0d91-f4bd-411e-936d-e4a8066b38dd");

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldExecureiif()
    {
        string xsltCall = $"AS:iif('true', 1, 0)";
        object expectedResult = "1";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arguments: new object[] { null, "1" })]
    [TestCase(arguments: new object[] { null, null, "1" })]
    [TestCase(arguments: new object[] { null, null, null, "1" })]
    public void ShouldExecureisnull(object[] arguments)
    {
        var strArguments = arguments.Select(selector: x => x == null ? "null" : $"'{x}'");
        string xsltCall = $"AS:isnull({string.Join(separator: ", ", values: strArguments)})";
        object expectedResult = arguments[arguments.Length - 1];

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldEncodeDataForUri()
    {
        string xsltCall = $"AS:EncodeDataForUri('http://test?p=193')";
        string expectedResult = "http%3A%2F%2Ftest%3Fp%3D193";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldDecodeDataFromUri()
    {
        string xsltCall = "AS:DecodeDataFromUri('http%3A%2F%2Ftest%3Fp%3D193')";
        string expectedResult = $"http://test?p=193";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arg1: "AS:AddMinutes('2022-07-13', 10)", arg2: "2022-07-13T00:10:00.0000000")]
    [TestCase(arg1: "AS:AddHours('2022-07-13', 1)", arg2: "2022-07-13T01:00:00.0000000")]
    [TestCase(arg1: "AS:AddDays('2022-07-13', 1)", arg2: "2022-07-14T00:00:00.0000000")]
    [TestCase(arg1: "AS:AddMonths('2022-07-13', 1)", arg2: "2022-08-13T00:00:00.0000000")]
    [TestCase(arg1: "AS:AddYears('2022-07-13', 1)", arg2: "2023-07-13T00:00:00.0000000")]
    public void ShouldAddTime(string xsltCall, string expectedResultWithoutTimeZone)
    {
        var dateTime = DateTime.Parse(s: "2022-07-13");
        TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(time: dateTime);
        string expectedResult = expectedResultWithoutTimeZone + $"+{offset:hh}:00";
        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arg1: "AS:DifferenceInDays('2022-07-13', '2022-07-14')", arg2: 1.0)]
    [TestCase(arg1: "AS:DifferenceInMinutes('2022-07-13', '2022-07-14')", arg2: 1440.0)]
    [TestCase(arg1: "AS:DifferenceInSeconds('2022-07-13', '2022-07-14')", arg2: 86_400.0)]
    public void ShouldGetTimeDifference(string xsltCall, double expectedResult)
    {
        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: expectedResult.ToString())
        );
    }

    [TestCase(arg: "AS:UTCDateTime()")]
    [TestCase(arg: "AS:LocalDateTime()")]
    public void ShouldGetDateTimeNow(string xsltCall)
    {
        DateTime minTestTime =
            DateTime.Now - new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 1);
        DateTime maxTestTime =
            DateTime.Now + new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 1);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        DateTime xpathResultDateTime = DateTime.Parse(s: (string)xPathResult);
        Assert.That(actual: xpathResultDateTime, expression: Is.GreaterThan(expected: minTestTime));
        Assert.That(actual: xpathResultDateTime, expression: Is.LessThan(expected: maxTestTime));

        string xsltResult = RunInXslt(xsltCall: xsltCall);
        DateTime xsltResultDateTime = DateTime.Parse(s: (string)xsltResult);
        Assert.That(actual: xsltResultDateTime, expression: Is.GreaterThan(expected: minTestTime));
        Assert.That(actual: xsltResultDateTime, expression: Is.LessThan(expected: maxTestTime));
    }

    [Test]
    public void ShouldTestIfFeatureOn()
    {
        string xsltCall = "AS:IsFeatureOn('testFeature')";
        bool expectedResult = true;
        parameterServiceMock
            .Setup(expression: service => service.IsFeatureOn("testFeature"))
            .Returns(value: true);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: expectedResult.ToString().ToLower())
        );
    }

    [Test]
    public void ShouldTestIsInRole()
    {
        string xsltCall = "AS:IsInRole('testRole')";
        bool expectedResult = true;
        authorizationProvider
            .Setup(expression: service =>
                service.Authorize(SecurityManager.CurrentPrincipal, "testRole")
            )
            .Returns(value: true);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: expectedResult.ToString().ToLower())
        );
    }

    [TestCase(
        arg1: "AS:ActiveProfileBusinessUnitId()",
        arg2: "3eed4998-4ca1-445f-a02d-d9851ea978a4"
    )]
    [TestCase(arg1: "AS:ActiveProfileId()", arg2: "e93a81d4-2520-4f14-9af9-574a61c609b0")]
    [TestCase(
        arg1: "AS:ActiveProfileOrganizationId()",
        arg2: "4f68699e-6755-4b7d-be93-257ae28f32f5"
    )]
    public void ShouldGetActiveProfileBusinessUnitId(string xsltCall, string expectedResult)
    {
        userProfileGetterMock
            .Setup(expression: x => x.Invoke())
            .Returns(
                value: new UserProfile
                {
                    BusinessUnitId = Guid.Parse(input: "3eed4998-4ca1-445f-a02d-d9851ea978a4"),
                    Id = Guid.Parse(input: "e93a81d4-2520-4f14-9af9-574a61c609b0"),
                    OrganizationId = Guid.Parse(input: "4f68699e-6755-4b7d-be93-257ae28f32f5"),
                }
            );
        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestIsUserAuthenticated()
    {
        string xsltCall = "AS:IsUserAuthenticated()";
        bool expectedResult = false;

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: expectedResult.ToString().ToLower())
        );
    }

    [Test]
    public void ShouldReturnNull()
    {
        string xsltCall = "AS:null()";
        object xPathResult = RunInXpath(xsltCall: xsltCall);

        Assert.That(
            actual: xPathResult,
            expression: Is.AssignableTo(expectedType: typeof(XPathNodeIterator))
        );
        XPathNodeIterator resultIterator = (XPathNodeIterator)xPathResult;
        Assert.That(actual: resultIterator, expression: Has.Count.EqualTo(expected: 0));
        // this function is not available in the xslt transformations
    }

    [Test]
    public void ShouldRunToXml()
    {
        string testXml = "<TestNode>test</TestNode>";
        string xsltCall = $"AS:ToXml('{testXml}')";
        string xsltCallXslt = $"AS:ToXml('&lt;TestNode&gt;test&lt;/TestNode&gt;')";
        // string xsltCallXslt = $"AS:ToXml('bla')";
        string expectedResultXpath = testXml;
        string expectedResultXslt = "test";

        string xsltScriptTemplate =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n"
            + "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n"
            + "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n"
            + "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n"
            + "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n"
            + "	<xsl:template match=\"ROOT\">\n"
            + "		<ROOT>\n"
            + "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n"
            + "				<xsl:attribute name=\"d1\">\n"
            + $"                  <xsl:value-of select=\"{xsltCallXslt}/TestNode\"/>\n"
            + "              </xsl:attribute>\n"
            + "			</SD>\n"
            + "		</ROOT>\n"
            + "	</xsl:template>\n"
            + "</xsl:stylesheet>\n";

        var document = new XmlDocument();
        document.LoadXml(xml: "<ROOT></ROOT>");

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(
            actual: xPathResult,
            expression: Is.AssignableTo(expectedType: typeof(XPathNodeIterator))
        );
        XPathNodeIterator resultIterator = (XPathNodeIterator)xPathResult;
        resultIterator.MoveNext();
        Assert.That(
            actual: resultIterator.Current.OuterXml,
            expression: Is.EqualTo(expected: expectedResultXpath)
        );

        string xsltResult = RunInXslt(
            xsltCall: "",
            document: document,
            xsltScript: xsltScriptTemplate
        );
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResultXslt));
    }

    [TestCase(arg: "AS:GenerateSerial('counter1')")]
    [TestCase(arg: "AS:GenerateSerial('counter1', '0001-01-01')")]
    public void ShouldGenerateSerial(string xsltCall)
    {
        string counterCode = "counter1";
        string expectedResult = "result1";

        Thread.CurrentThread.CurrentCulture = new CultureInfo(name: "cs-CZ");
        XPathNavigator nav = new XmlDocument().CreateNavigator();
        XPathExpression expr = nav.Compile(xpath: xsltCall);

        counterMock
            .Setup(expression: x => x.GetNewCounter(counterCode, DateTime.MinValue, null))
            .Returns(value: "result1");

        List<XsltFunctionsDefinition> containers = new List<XsltFunctionsDefinition>
        {
            new XsltFunctionsDefinition(
                Container: new LegacyXsltFunctionContainer(counter: counterMock.Object),
                NameSpacePrefix: "AS",
                NameSpaceUri: "http://schema.advantages.cz/AsapFunctions"
            ),
        };

        OrigamXsltContext sut = new OrigamXsltContext(
            nt: new NameTable(),
            xsltFunctionsDefinitions: containers
        );

        expr.SetContext(nsManager: sut);
        object xPathResult = nav.Evaluate(expr: expr);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));

        string xslScript = string.Format(format: xsltScriptTemplate, arg0: xsltCall);
        var transformer = new CompiledXsltEngine(functionsDefinitions: containers);
        XmlDocument document = new XmlDocument();
        document.LoadXml(xml: "<ROOT></ROOT>");
        XmlContainer xmlContainer = new XmlContainer(xmlDocument: document);
        IXmlContainer resultContainer = transformer.Transform(
            data: xmlContainer,
            xsl: xslScript,
            parameters: new Hashtable(),
            transactionId: null,
            outputStructure: null,
            validateOnly: false
        );

        var regex = new Regex(pattern: "d1=\"(.*)\"");
        var match = regex.Match(input: resultContainer.Xml.OuterXml);
        string xsltResult = match.Success ? match.Groups[groupnum: 1].Value : "";

        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestIsInState()
    {
        Guid entityId = Guid.Parse(input: "153c5d36-0aa7-4073-a4f9-b0dc31f2edea");
        Guid fieldId = Guid.Parse(input: "1038de85-5c44-4e75-9052-70a4c3c35dab");
        string currentState = "testState";
        Guid targetStateId = Guid.Parse(input: "2157eb16-6643-45f7-b304-bd6fea811e16");

        string xsltCall =
            $"AS:IsInState('{entityId}', '{fieldId}', '{currentState}', '{targetStateId}')";
        bool expectedResult = true;

        stateMachineServiceMock
            .Setup(expression: x => x.IsInState(entityId, fieldId, currentState, targetStateId))
            .Returns(value: true);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: expectedResult.ToString().ToLower())
        );
    }

    [Test]
    public void ShouldTestFormatLink()
    {
        string xsltCall = $"AS:FormatLink('http://localhost', 'test1')";
        string expectedResult =
            "<a href=\"http://localhost\" target=\"_blank\"><u><font color=\"#0000ff\">test1</font></u></a>";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: HttpUtility.HtmlEncode(s: expectedResult))
        );
    }

    [Test]
    public void ShouldTestIsUserLockedOut()
    {
        string userId = "4c738d1f-b0a8-4816-9f06-4d58ef49fcda";
        string xsltCall = $"AS:IsUserLockedOut('{userId}')";
        bool expectedResult = true;

        var agentMock = new Mock<IServiceAgent>();
        var agentParameters = new Hashtable();
        agentMock.SetupGet(expression: x => x.Parameters).Returns(value: agentParameters);
        agentMock.SetupGet(expression: x => x.Parameters).Returns(value: agentParameters);
        agentMock.SetupGet(expression: x => x.Result).Returns(value: expectedResult);
        agentMock.SetupSet(setterExpression: x => x.MethodName = "IsLockedOut").Verifiable();
        businessServiceMock
            .Setup(expression: x => x.GetAgent("IdentityService", null, null))
            .Returns(value: agentMock.Object);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        Assert.That(actual: agentParameters, expression: Has.Count.EqualTo(expected: 1));
        Assert.That(condition: agentParameters.ContainsKey(key: "UserId"));
        Assert.That(
            actual: agentParameters[key: "UserId"],
            expression: Is.EqualTo(expected: Guid.Parse(input: userId))
        );
        agentMock.Verify();
        agentMock.Verify(expression: x => x.Run(), times: Times.Once);

        agentParameters.Clear();
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: expectedResult.ToString().ToLower())
        );
        Assert.That(actual: agentParameters, expression: Has.Count.EqualTo(expected: 1));
        Assert.That(condition: agentParameters.ContainsKey(key: "UserId"));
        Assert.That(
            actual: agentParameters[key: "UserId"],
            expression: Is.EqualTo(expected: Guid.Parse(input: userId))
        );
        agentMock.Verify();
        agentMock.Verify(expression: x => x.Run(), times: Times.Exactly(callCount: 2));
    }

    [TestCase(arg1: "IsUserLockedOut", arg2: "IsLockedOut")]
    [TestCase(arg1: "IsUserEmailConfirmed", arg2: "IsEmailConfirmed")]
    [TestCase(arg1: "Is2FAEnforced", arg2: "Is2FAEnforced")]
    public void ShouldTestUserInfoMethods(string asMethodName, string methodName)
    {
        string userId = "4c738d1f-b0a8-4816-9f06-4d58ef49fcda";
        string xsltCall = $"AS:{asMethodName}('{userId}')";
        bool expectedResult = true;

        var agentMock = new Mock<IServiceAgent>();
        var agentParameters = new Hashtable();
        agentMock.SetupGet(expression: x => x.Parameters).Returns(value: agentParameters);
        agentMock.SetupGet(expression: x => x.Parameters).Returns(value: agentParameters);
        agentMock.SetupGet(expression: x => x.Result).Returns(value: expectedResult);
        agentMock.SetupSet(setterExpression: x => x.MethodName = methodName).Verifiable();
        businessServiceMock
            .Setup(expression: x => x.GetAgent("IdentityService", null, null))
            .Returns(value: agentMock.Object);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        Assert.That(actual: agentParameters, expression: Has.Count.EqualTo(expected: 1));
        Assert.That(condition: agentParameters.ContainsKey(key: "UserId"));
        Assert.That(
            actual: agentParameters[key: "UserId"],
            expression: Is.EqualTo(expected: Guid.Parse(input: userId))
        );
        agentMock.Verify();
        agentMock.Verify(expression: x => x.Run(), times: Times.Once);

        agentParameters.Clear();
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: expectedResult.ToString().ToLower())
        );
        Assert.That(actual: agentParameters, expression: Has.Count.EqualTo(expected: 1));
        Assert.That(condition: agentParameters.ContainsKey(key: "UserId"));
        Assert.That(
            actual: agentParameters[key: "UserId"],
            expression: Is.EqualTo(expected: Guid.Parse(input: userId))
        );
        agentMock.Verify();
        agentMock.Verify(expression: x => x.Run(), times: Times.Exactly(callCount: 2));
    }

    [Test]
    public void ShouldTestInitPosition()
    {
        var id = "f62d6323-92ec-4c67-8fed-0e48d30dae8f";
        string xsltCallInit = $"AS:InitPosition('{id}', 1)";
        string xsltCallNext = $"AS:NextPosition('{id}', 1)";
        decimal expectedResult1 = 2;
        decimal expectedResult2 = 3;

        RunInXpath(xsltCall: xsltCallInit);
        object xPathResult = RunInXpath(xsltCall: xsltCallNext);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult1));
        string xsltResult = RunInXslt(xsltCall: xsltCallNext);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: HttpUtility.HtmlEncode(value: expectedResult2))
        );
    }

    [Test]
    public void ShouldTestGetMenuId()
    {
        var lookupId = "f62d6323-92ec-4c67-8fed-0e48d30dae8f";
        string value = "lookupValue";
        string xsltCall = $"AS:GetMenuId('{lookupId}', '{value}')";
        string expectedResult = "02e08f1c-7d6a-4f43-8b19-8e4a19bc7f2f";
        var bindingResultMock = new Mock<IMenuBindingResult>();
        bindingResultMock.SetupGet(expression: x => x.MenuId).Returns(value: expectedResult);

        lookupServiceMock
            .Setup(expression: service => service.GetMenuBinding(Guid.Parse(lookupId), value))
            .Returns(value: bindingResultMock.Object);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: HttpUtility.HtmlEncode(s: expectedResult))
        );
    }

    [Test]
    public void ShouldDecodeSignedOverpunch()
    {
        string xsltCall = "AS:DecodeSignedOverpunch('0000000000{', 2)";
        double expectedResult = 0.0;

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(
            actual: xPathResult,
            expression: Is.EqualTo(expected: expectedResult).Within(amount: 0.000000000000001)
        );
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        double xsltResultDouble = double.Parse(s: xsltResult);
        Assert.That(
            actual: xsltResultDouble,
            expression: Is.EqualTo(expected: expectedResult).Within(amount: 0.000000000000001)
        );
    }

    [Test]
    public void ShouldRandomlyDistributeValues()
    {
        string xsltScriptTemplate =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n"
            + "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n"
            + "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n"
            + "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n"
            + "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n"
            + "	<xsl:template match=\"ROOT\">\n"
            + "       <xsl:variable name=\"parameters1\">\n"
            + "           <parameter value=\"val0\" quantity=\"0\"/>\n"
            + "           <parameter value=\"val1\" quantity=\"1\"/>\n"
            + "           <parameter value=\"val2\" quantity=\"2\"/>\n"
            + "       </xsl:variable>\n"
            + "		<ROOT>\n"
            + "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n"
            + "				<xsl:attribute name=\"d1\">\n"
            + $"                  <xsl:value-of select=\"AS:RandomlyDistributeValues($parameters1)\"/>\n"
            + "              </xsl:attribute>\n"
            + "			</SD>\n"
            + "		</ROOT>\n"
            + "	</xsl:template>\n"
            + "</xsl:stylesheet>\n";

        var document = new XmlDocument();
        document.LoadXml(xml: "<ROOT></ROOT>");

        // Xpath test is omitted because this function is not useful in xpath.

        string xsltResult = RunInXslt(
            xsltCall: "",
            document: document,
            xsltScript: xsltScriptTemplate
        );
        Assert.That(condition: xsltResult.Contains(value: "val"));
    }

    [Test]
    public void ShouldTestRandom()
    {
        string xsltCall = "AS:Random()";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.LessThanOrEqualTo(expected: 1));
        Assert.That(actual: xPathResult, expression: Is.GreaterThanOrEqualTo(expected: 0));

        string xsltResult = RunInXslt(xsltCall: xsltCall);
        double xsltDoubleResult = double.Parse(
            s: xsltResult,
            style: NumberStyles.Any,
            provider: CultureInfo.InvariantCulture
        );
        Assert.That(actual: xsltDoubleResult, expression: Is.LessThanOrEqualTo(expected: 1));
        Assert.That(actual: xsltDoubleResult, expression: Is.GreaterThanOrEqualTo(expected: 0));
    }

    [Test]
    public void ShouldTestIsUriValid()
    {
        string xsltCall = "AS:IsUriValid('https://www.google.com/')";
        bool expectedResult = true;

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: expectedResult.ToString().ToLower())
        );
    }

    [Test]
    public void ShouldTestReferenceCount()
    {
        string entityId = "7948c229-39e5-4fe7-9cb0-4d0ad21cd899";
        string value = "testValue";
        string xsltCall = $"AS:ReferenceCount('{entityId}', '{value}')";
        long expectedResult = 1;

        dataServiceMock
            .Setup(expression: x => x.ReferenceCount(Guid.Parse(entityId), value, null))
            .Returns(value: expectedResult);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: expectedResult.ToString().ToLower())
        );
    }

    [Test]
    public void ShouldTestGenerateId()
    {
        string xsltCall = "AS:GenerateId()";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.InstanceOf<string>());
        bool xPathResultIsGuid = Guid.TryParse(input: (string)xPathResult, result: out _);
        Assert.That(condition: xPathResultIsGuid);
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        bool xsltResultIsGuid = Guid.TryParse(input: xsltResult, result: out _);
        Assert.That(condition: xsltResultIsGuid);
    }

    [Test]
    public void ShouldListDays()
    {
        var dateTime = DateTime.Parse(s: "2022-01-01");
        TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(time: dateTime);
        string xsltCall = "AS:ListDays('2022-01-01', '2022-01-03')";
        string expectedResultXpath =
            "<list>"
            + Environment.NewLine
            + $"  <item>2022-01-01T00:00:00+{offset:hh}:00</item>"
            + Environment.NewLine
            + $"  <item>2022-01-02T00:00:00+{offset:hh}:00</item>"
            + Environment.NewLine
            + $"  <item>2022-01-03T00:00:00+{offset:hh}:00</item>"
            + Environment.NewLine
            + "</list>";
        string expectedResultXslt =
            $"2022-01-01T00:00:00+{offset:hh}:002022-01-02T00:00:00+{offset:hh}:002022-01-03T00:00:00+{offset:hh}:00";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.InstanceOf<XPathNodeIterator>());
        var nodeIterator = xPathResult as XPathNodeIterator;
        nodeIterator.MoveNext();
        Assert.That(
            actual: nodeIterator.Current.OuterXml,
            expression: Is.EqualTo(expected: expectedResultXpath)
        );
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResultXslt));
    }

    [Test]
    public void ShouldTestIsDateBetween()
    {
        string xsltCall = "AS:IsDateBetween('2022-01-02', '2022-01-01', '2022-01-03')";
        bool expectedResult = true;

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(
            actual: xsltResult,
            expression: Is.EqualTo(expected: expectedResult.ToString().ToLower())
        );
    }

    [Test]
    public void ShouldTestAddWorkingDays()
    {
        Guid calendarId = Guid.Parse(input: "186ec5ec-7877-4204-8323-2ffdd816332d");
        string xsltCall = $"AS:AddWorkingDays('2022-01-02', '2', '{calendarId}')";

        var expectedDate = DateTime.Parse(s: "2022-01-04");
        TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(time: expectedDate);
        string expectedResult = $"2022-01-04T00:00:00.0000000+{offset:hh}:00";

        var agentMock = new Mock<IServiceAgent>();
        var agentParameters = new Hashtable();
        agentMock.SetupGet(expression: x => x.Parameters).Returns(value: agentParameters);
        agentMock.SetupGet(expression: x => x.Result).Returns(value: new DataSet());
        agentMock.SetupSet(setterExpression: x => x.MethodName = "LoadDataByQuery").Verifiable();

        businessServiceMock
            .Setup(expression: x => x.GetAgent("DataService", null, null))
            .Returns(value: agentMock.Object);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        Assert.That(actual: agentParameters, expression: Has.Count.EqualTo(expected: 1));
        Assert.That(condition: agentParameters.ContainsKey(key: "Query"));
        agentMock.Verify();
        agentMock.Verify(expression: x => x.Run(), times: Times.Once);

        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
        Assert.That(actual: agentParameters, expression: Has.Count.EqualTo(expected: 1));
        Assert.That(condition: agentParameters.ContainsKey(key: "Query"));
        agentMock.Verify();
        agentMock.Verify(expression: x => x.Run(), times: Times.Exactly(callCount: 2));
    }

    [TestCase(arg1: "AS:LastDayOfMonth('2022-01-02')", arg2: "2022-01-31")]
    [TestCase(arg1: "AS:FirstDayNextMonthDate('2022-01-02')", arg2: "2022-02-01")]
    [TestCase(arg1: "AS:Year('2022-01-02')", arg2: "2022")]
    [TestCase(arg1: "AS:Month('2022-01-02')", arg2: "01")]
    public void ShouldTestDateFunctions(string xsltCall, string expectedResult)
    {
        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestDecimalNumber()
    {
        string xsltCall = "AS:DecimalNumber('5')";
        string expectedResult = "5";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arg1: "AS:avg(/ROOT/N1/@count)", arg2: "2")]
    [TestCase(arg1: "AS:sum(/ROOT/N1/@count)", arg2: "6")]
    [TestCase(arg1: "AS:Sum(/ROOT/N1/@count)", arg2: "6")]
    [TestCase(arg1: "AS:min(/ROOT/N1/@count)", arg2: "1")]
    [TestCase(arg1: "AS:Min(/ROOT/N1/@count)", arg2: "1")]
    [TestCase(arg1: "AS:max(/ROOT/N1/@count)", arg2: "3")]
    [TestCase(arg1: "AS:Max(/ROOT/N1/@count)", arg2: "3")]
    public void ShouldListFunctions(string xsltCall, string expectedResult)
    {
        var document = new XmlDocument();
        document.LoadXml(
            xml: "<ROOT><N1 count=\"1\"></N1><N1 count=\"2\"></N1><N1  count=\"3\"></N1></ROOT>"
        );

        object xPathResult = RunInXpath(xsltCall: xsltCall, document: document);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall, document: document);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [TestCase(arg1: "AS:Uppercase('test')", arg2: "TEST")]
    [TestCase(arg1: "AS:Lowercase('TEST')", arg2: "test")]
    public void ShouldTestStringManipulationFunctions(string xsltCall, string expectedResult)
    {
        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestTranslate()
    {
        string dictionaryId = "852be9b7-fb22-4669-8ff4-902fdcff51da";
        string text = "test";
        string xsltCall = $"AS:Translate('{dictionaryId}', '{text}')";
        string expectedResult = "5";

        var dataSet = new DataSet();
        var dataTable = new DataTable();
        dataTable.Columns.Add(columnName: "Source", type: typeof(string));
        dataTable.Columns.Add(columnName: "Target", type: typeof(string));
        var dataRow = dataTable.NewRow();
        dataRow[columnName: "Source"] = text;
        dataRow[columnName: "Target"] = expectedResult;
        dataTable.Rows.Add(row: dataRow);
        dataTable.AcceptChanges();
        dataSet.Tables.Add(table: dataTable);

        dataServiceMock
            .Setup(expression: x =>
                x.LoadData(
                    new Guid("9268abd0-a08e-4c97-b5f7-219eacf171c0"),
                    new Guid("c2cd04cd-9a47-49d8-aa03-2e07044b3c7c"),
                    Guid.Empty,
                    new Guid("26b8f31b-a6ce-4a0a-905d-0915855cd934"),
                    null,
                    "OrigamCharacterTranslationDetail_parOrigamCharacterTranslationId",
                    new Guid(dictionaryId)
                )
            )
            .Returns(value: dataSet);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    class TestXmlContainer : XmlContainer
    {
        protected bool Equals(XmlContainer other)
        {
            return Equals(objA: Xml.OuterXml, objB: other.Xml.OuterXml);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(objA: null, objB: obj))
            {
                return false;
            }

            if (ReferenceEquals(objA: this, objB: obj))
            {
                return true;
            }

            if (!(obj is XmlContainer))
            {
                return false;
            }

            return Equals(other: (XmlContainer)obj);
        }

        public override int GetHashCode()
        {
            return (Xml != null ? Xml.GetHashCode() : 0);
        }
    }

    [Test]
    public void ShouldTestNextStates()
    {
        string entityId = "cdb8611a-8e38-4613-bc4f-959225ff21f7";
        string fieldId = "cc8a9da4-8833-4c27-9b0c-a1d2eef3f078";
        string currentStateValue = "1";
        string xsltCall =
            $"AS:NextStates('{entityId}', '{fieldId}', '{currentStateValue}', /ROOT/TestNode[1])";
        string expectedResultXpath = "<states />";
        string expectedResultXslt = "";

        IXmlContainer doc = new XmlContainer();
        doc.LoadXml(xmlString: "<ROOT><TestNode name=\"testNode\"></TestNode></ROOT>");

        // Mocking AllowedStateValues does not work here because XmlContainer does not override Equals
        // IXmlContainer document = new TestXmlContainer();
        // document.LoadXml("<row name=\"testNode\" />");
        // stateMachineServiceMock
        //     .Setup(x => x.AllowedStateValues(new Guid(entityId), new Guid(fieldId), currentStateValue, document, null))
        //     .Returns(new object[]{"1", "2", "3"});

        object xPathResult = RunInXpath(xsltCall: xsltCall, document: doc.Xml);
        Assert.That(actual: xPathResult, expression: Is.InstanceOf<XPathNodeIterator>());
        var nodeIterator = xPathResult as XPathNodeIterator;
        nodeIterator.MoveNext();
        Assert.That(
            actual: nodeIterator.Current.OuterXml,
            expression: Is.EqualTo(expected: expectedResultXpath)
        );

        string xsltResult = RunInXslt(xsltCall: xsltCall, document: doc.Xml);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResultXslt));
    }

    [Test]
    public void ShouldTestNodeSet()
    {
        string expectedResult = "test";

        string xsltScript =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n"
            + "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n"
            + "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n"
            + "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n"
            + "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n"
            + "	<xsl:template match=\"ROOT\">\n"
            + "       <xsl:variable name=\"testInput\"><TestNode>test</TestNode></xsl:variable>"
            + "		<ROOT>\n"
            + "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n"
            + "				<xsl:attribute name=\"d1\">\n"
            + $"                  <xsl:value-of select=\"AS:NodeSet($testInput)/TestNode\"/>\n"
            + "              </xsl:attribute>\n"
            + "			</SD>\n"
            + "		</ROOT>\n"
            + "	</xsl:template>\n"
            + "</xsl:stylesheet>\n";

        // Xpath test is omitted because this function is not useful in xpath.

        var document = new XmlDocument();
        document.LoadXml(xml: "<ROOT></ROOT>");

        string xsltResult = RunInXslt(xsltCall: "", document: document, xsltScript: xsltScript);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestNodeToString()
    {
        string xsltCall = "AS:NodeToString(/ROOT)";
        string expectedResultXpath =
            "<ROOT>"
            + Environment.NewLine
            + "  <N1 count=\"1\">"
            + Environment.NewLine
            + "  </N1>"
            + Environment.NewLine
            + "  <N1 count=\"2\">"
            + Environment.NewLine
            + "  </N1>"
            + Environment.NewLine
            + "  <N1 count=\"3\">"
            + Environment.NewLine
            + "  </N1>"
            + Environment.NewLine
            + "</ROOT>";
        string newLineCR = "&#xD;";
        string newLineLF = "&#xA;";
        string newLinePlatform = newLineCR + newLineLF;
        if (
            RuntimeInformation.IsOSPlatform(
                osPlatform: System.Runtime.InteropServices.OSPlatform.Linux
            )
        )
        {
            newLinePlatform = newLineLF;
        }

        string expectedResultXslt =
            "&lt;ROOT&gt;"
            + newLinePlatform
            + "  &lt;N1 count=&quot;1&quot;&gt;"
            + newLinePlatform
            + "  "
            + "&lt;/N1&gt;"
            + newLinePlatform
            + "  &lt;N1 count=&quot;2&quot;&gt;"
            + newLinePlatform
            + "  "
            + "&lt;/N1&gt;"
            + newLinePlatform
            + "  &lt;N1 count=&quot;3&quot;&gt;"
            + newLinePlatform
            + "  "
            + "&lt;/N1&gt;"
            + newLinePlatform
            + "&lt;/ROOT&gt;";
        var document = new XmlDocument();
        document.LoadXml(
            xml: "<ROOT><N1 count=\"1\"></N1><N1 count=\"2\"></N1><N1  count=\"3\"></N1></ROOT>"
        );

        object xPathResult = RunInXpath(xsltCall: xsltCall, document: document);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResultXpath));
        string xsltResult = RunInXslt(xsltCall: xsltCall, document: document);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResultXslt));
    }

    [Test]
    public void ShouldTestDecodeTextFromBase64()
    {
        string xsltCall = "AS:DecodeTextFromBase64('dGVzdA==', 'UTF-8')";
        string expectedResult = "test";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestPointFromJtsk()
    {
        string xsltCall = "AS:PointFromJtsk(-740614.4101, -1030989.305)";
        string expectedResult = "POINT(14.428757007606 50.197251879067)";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestPolygonFromJstkWithSingleParentheses()
    {
        string xsltCall =
            "GE:PolygonFromJstk(\"POLYGON("
            + "-740614.4101 -1030989.305, "
            + "-741540.4803 -1030921.8493, "
            + "-741540.4149 -1030920.9517, "
            + "-741542.3916 -1030936.2522, "
            + "-741555.0577 -1031034.2896, "
            + "-741583.5733 -1031255.006, "
            + "-741589.2724 -1031299.1183, "
            + "-741588.2768 -1031299.2164, "
            + "-740960.9255 -1031344.9745, "
            + "-740959.4536 -1031345.103, "
            + "-740897.274 -1031297.208, "
            + "-740851.0788 -1031262.415, "
            + "-740803.977 -1031228.374, "
            + "-740769.1989 -1031197.8036, "
            + "-740738.9305 -1031169.0874, "
            + "-740729.4307 -1031158, "
            + "-740720.3038 -1031147.3563, "
            + "-740682.2743 -1031090.7, "
            + "-740643.6627 -1031028.611, "
            + "-740623.6778 -1031000.671, "
            + "-740614.4101 -1030989.305)\")";
        string expectedResult =
            "POLYGON("
            + "14.428757007606 50.197251879067, "
            + "14.415778426264 50.196717768985, "
            + "14.415777618954 50.196725844024, "
            + "14.415779421294 50.196587139862, "
            + "14.415790963027 50.195698397276, "
            + "14.415816949028 50.193697527127, "
            + "14.415822142591 50.193297633847, "
            + "14.415836143327 50.193297980994, "
            + "14.424627734011 50.193659413604, "
            + "14.424648401463 50.193660072512, "
            + "14.425419720668 50.194162859587, "
            + "14.425994275757 50.194529358653, "
            + "14.426582853846 50.19489026733, "
            + "14.42700706928 50.195205163884, "
            + "14.427372254891 50.195498019248, "
            + "14.427482911079 50.195608412368, "
            + "14.427589240292 50.195714396531, "
            + "14.428008810211 50.196265621681, "
            + "14.428426101023 50.196865947851, "
            + "14.428650096847 50.197139289959, "
            + "14.428757007606 50.197251879067)";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(
            xsltCall: xsltCall.Replace(oldValue: "\"", newValue: "&quot;")
        );
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestPolygonFromJstkWithDoubleParentheses()
    {
        string xsltCall =
            "GE:PolygonFromJstk(\"POLYGON (("
            + "-740614.4101 -1030989.305, "
            + "-740614.4101 -1030989.305))\")";
        string expectedResult =
            "POLYGON (("
            + "14.428757007606 50.197251879067, "
            + "14.428757007606 50.197251879067))";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(
            xsltCall: xsltCall.Replace(oldValue: "\"", newValue: "&quot;")
        );
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestEmptyPolygonFromJstk()
    {
        string xsltCall = "GE:PolygonFromJstk(\"POLYGON EMPTY\")";
        string expectedResult = "POLYGON EMPTY";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(
            xsltCall: xsltCall.Replace(oldValue: "\"", newValue: "&quot;")
        );
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestPolygonFromJstkWithTwoObjects()
    {
        string xsltCall =
            "GE:PolygonFromJstk(\"POLYGON (("
            + "-740614.4101 -1030989.305, "
            + "-741540.4803 -1030921.8493, "
            + "-741540.4149 -1030920.9517, "
            + "-741542.3916 -1030936.2522, "
            + "-741555.0577 -1031034.2896, "
            + "-741583.5733 -1031255.006, "
            + "-741589.2724 -1031299.1183, "
            + "-741588.2768 -1031299.2164, "
            + "-740960.9255 -1031344.9745, "
            + "-740959.4536 -1031345.103, "
            + "-740897.274 -1031297.208, "
            + "-740851.0788 -1031262.415, "
            + "-740803.977 -1031228.374, "
            + "-740769.1989 -1031197.8036, "
            + "-740738.9305 -1031169.0874, "
            + "-740729.4307 -1031158, "
            + "-740720.3038 -1031147.3563, "
            + "-740682.2743 -1031090.7, "
            + "-740643.6627 -1031028.611, "
            + "-740623.6778 -1031000.671, "
            + "-740614.4101 -1030989.305), ("
            + "-740614.4101 -1030989.305, "
            + "-741540.4803 -1030921.8493, "
            + "-741540.4149 -1030920.9517, "
            + "-741542.3916 -1030936.2522, "
            + "-741555.0577 -1031034.2896, "
            + "-741583.5733 -1031255.006, "
            + "-741589.2724 -1031299.1183, "
            + "-741588.2768 -1031299.2164, "
            + "-740960.9255 -1031344.9745, "
            + "-740959.4536 -1031345.103, "
            + "-740897.274 -1031297.208, "
            + "-740851.0788 -1031262.415, "
            + "-740803.977 -1031228.374, "
            + "-740769.1989 -1031197.8036, "
            + "-740738.9305 -1031169.0874, "
            + "-740729.4307 -1031158, "
            + "-740720.3038 -1031147.3563, "
            + "-740682.2743 -1031090.7, "
            + "-740643.6627 -1031028.611, "
            + "-740623.6778 -1031000.671,"
            + "-740614.4101 -1030989.305))\")";
        string expectedResult =
            "POLYGON ((14.428757007606 50.197251879067,"
            + " 14.415778426264 50.196717768985,"
            + " 14.415777618954 50.196725844024,"
            + " 14.415779421294 50.196587139862,"
            + " 14.415790963027 50.195698397276,"
            + " 14.415816949028 50.193697527127,"
            + " 14.415822142591 50.193297633847,"
            + " 14.415836143327 50.193297980994,"
            + " 14.424627734011 50.193659413604,"
            + " 14.424648401463 50.193660072512,"
            + " 14.425419720668 50.194162859587,"
            + " 14.425994275757 50.194529358653,"
            + " 14.426582853846 50.19489026733,"
            + " 14.42700706928 50.195205163884,"
            + " 14.427372254891 50.195498019248,"
            + " 14.427482911079 50.195608412368,"
            + " 14.427589240292 50.195714396531,"
            + " 14.428008810211 50.196265621681,"
            + " 14.428426101023 50.196865947851,"
            + " 14.428650096847 50.197139289959,"
            + " 14.428757007606 50.197251879067),"
            + " (14.428757007606 50.197251879067,"
            + " 14.415778426264 50.196717768985,"
            + " 14.415777618954 50.196725844024,"
            + " 14.415779421294 50.196587139862,"
            + " 14.415790963027 50.195698397276,"
            + " 14.415816949028 50.193697527127,"
            + " 14.415822142591 50.193297633847,"
            + " 14.415836143327 50.193297980994,"
            + " 14.424627734011 50.193659413604,"
            + " 14.424648401463 50.193660072512,"
            + " 14.425419720668 50.194162859587,"
            + " 14.425994275757 50.194529358653,"
            + " 14.426582853846 50.19489026733,"
            + " 14.42700706928 50.195205163884,"
            + " 14.427372254891 50.195498019248,"
            + " 14.427482911079 50.195608412368,"
            + " 14.427589240292 50.195714396531,"
            + " 14.428008810211 50.196265621681,"
            + " 14.428426101023 50.196865947851,"
            + " 14.428650096847 50.197139289959,"
            + "14.428757007606 50.197251879067))";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(
            xsltCall: xsltCall.Replace(oldValue: "\"", newValue: "&quot;")
        );
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestAbs()
    {
        string xsltCall = "AS:abs(-1)";
        string expectedResult = "1";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestEvaluateXPath()
    {
        string xsltCall = "AS:EvaluateXPath('nodes', 'path')";
        string expectedResult = "testResult";

        xPathEvaluatorMock
            .Setup(expression: x => x.Evaluate("nodes", "path"))
            .Returns(value: "testResult");

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestHttpRequest()
    {
        string url = "http://localhost";
        string xsltCall = $"AS:HttpRequest('{url}')";
        string expectedResult = "testResult";

        httpToolsMock
            .Setup(expression: x =>
                x.SendRequest(
                    new Request(
                        url,
                        null,
                        null,
                        null,
                        new Hashtable(),
                        null,
                        null,
                        null,
                        false,
                        null,
                        true,
                        null,
                        false
                    )
                )
            )
            .Returns(
                value: new HttpResult(
                    Content: expectedResult,
                    StatusCode: null,
                    StatusDescription: null,
                    Headers: null,
                    Exception: null
                )
            );

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldTestProcessMarkdown()
    {
        string xsltCall = $"AS:ProcessMarkdown('# test')";
        string expectedXpathResult = "<h1>test</h1>\n";
        string expectedXsltResult = "&lt;h1&gt;test&lt;/h1&gt;&#xA;";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedXpathResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedXsltResult));
    }

    [Test]
    public void ShouldTestDiff()
    {
        string xsltCall = $"AS:Diff('old text', 'new text')";
        string expectedXpathResult =
            "<lines>"
            + Environment.NewLine
            + "  <line changeType=\"Deleted\">old text</line>"
            + Environment.NewLine
            + "  <line changeType=\"Inserted\" position=\"1\">new text</line>"
            + Environment.NewLine
            + "</lines>";
        string expectedXsltResult = "old textnew text";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.InstanceOf<XPathNodeIterator>());
        var nodeIterator = xPathResult as XPathNodeIterator;
        nodeIterator.MoveNext();
        Assert.That(
            actual: nodeIterator.Current.OuterXml,
            expression: Is.EqualTo(expected: expectedXpathResult)
        );

        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedXsltResult));
    }

    [Test]
    public void ShouldTestResourceIdByActiveProfile()
    {
        string xsltCall = $"AS:ResourceIdByActiveProfile()";
        string expectedResult = "testId";

        resourceToolsMock
            .Setup(expression: x => x.ResourceIdByActiveProfile())
            .Returns(value: expectedResult);

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));
        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }
}
