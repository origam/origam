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
using NUnit.Framework.Internal;
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
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
        "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n" +
        "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n" +
        "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n" +
        "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n" +
        "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n" +
        "	<xsl:template match=\"ROOT\">\n" +
        "		<ROOT>\n" +
        "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n" +
        "				<xsl:attribute name=\"d1\">\n" +
        "                  <xsl:value-of select=\"{0}\"/>\n" +
        "              </xsl:attribute>\n" +
        "			</SD>\n" +
        "		</ROOT>\n" +
        "	</xsl:template>\n" +
        "</xsl:stylesheet>\n";

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
        functionSchemaItemProvider =
            new Mock<IXsltFunctionSchemaItemProvider>();
        xPathEvaluatorMock = new Mock<IXpathEvaluator>();
        httpToolsMock = new Mock<IHttpTools>();
        resourceToolsMock = new Mock<IResourceTools>();

        var functionCollection = new XsltFunctionCollection();
        functionCollection.AssemblyName = "Origam.Rule";
        functionCollection.FullClassName =
            "Origam.Rule.XsltFunctions.LegacyXsltFunctionContainer";
        functionCollection.XslNameSpaceUri =
            "http://schema.advantages.cz/AsapFunctions";
        functionCollection.XslNameSpacePrefix = "AS";
        functionSchemaItemProvider
            .Setup(x =>
                x.ChildItemsByType(XsltFunctionCollection.CategoryConst))
            .Returns(new System.Collections.Generic.List<ISchemaItem> { functionCollection });

        xsltFunctionDefinitions = XsltFunctionContainerFactory.Create(
            businessServiceMock.Object,
            functionSchemaItemProvider.Object,
            persistenceServiceMock.Object,
            lookupServiceMock.Object,
            parameterServiceMock.Object,
            stateMachineServiceMock.Object,
            tracingServiceMock.Object,
            documentationServiceMock.Object,
            dataServiceMock.Object,
            authorizationProvider.Object,
            userProfileGetterMock.Object,
            xPathEvaluatorMock.Object,
            httpToolsMock.Object,
            resourceToolsMock.Object,
            null
        ).ToList();
    }

    private object RunInXpath(string xsltCall, XmlDocument document = null)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("cs-CZ");
        XPathNavigator nav = (document ?? new XmlDocument()).CreateNavigator();
        XPathExpression expr = nav.Compile(xsltCall);
        OrigamXsltContext sut = new OrigamXsltContext(
            new NameTable(),
            xsltFunctionDefinitions
        );
        expr.SetContext(sut);
        return nav.Evaluate(expr);
    }

    private string RunInXslt(string xsltCall, XmlDocument document = null,
        string xsltScript = null)
    {
        xsltScript ??= string.Format(xsltScriptTemplate, xsltCall);

        var transformer = new CompiledXsltEngine(xsltFunctionDefinitions);
        if (document == null)
        {
            document = new XmlDocument();
            document.LoadXml("<ROOT></ROOT>");
        }

        XmlContainer xmlContainer = new XmlContainer(document);
        IXmlContainer resultContainer = transformer.Transform(
            xmlContainer, xsltScript, new Hashtable(), null,
            null, false);

        var regex = new Regex("d1=\"(.*)\"");
        var match = regex.Match(resultContainer.Xml.OuterXml);
        return match.Success
            ? match.Groups[1].Value
            : "";
    }

    [Test]
    public void ShouldGetConstant()
    {
        string expectedResult = "constant1_value";
        string xsltCall = "AS:GetConstant('constant1')";
        parameterServiceMock
            .Setup(service =>
                service.GetParameterValue("constant1", OrigamDataType.String,
                    null))
            .Returns(expectedResult);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));

        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("string1", new object[0], "string1_value")]
    [TestCase("string1 {0} {1}", new object[] { "1", "2" },
        "string1_value 1 2")]
    [TestCase("string1 {0} {1} {2}", new object[] { "1", "2", "3" },
        "string1_value 1 2 3")]
    [TestCase("string1 {0} {1} {2} {3}", new object[] { "1", "2", "3", "4" },
        "string1_value 1 2 3 4")]
    [TestCase("string1 {0} {1} {2} {3} {4}",
        new object[] { "1", "2", "3", "4", "5" }, "string1_value 1 2 3 4 5")]
    [TestCase("string1 {0} {1} {2} {3} {4} {5}",
        new object[] { "1", "2", "3", "4", "5", "6" },
        "string1_value 1 2 3 4 5 6")]
    [TestCase("string1 {0} {1} {2} {3} {4} {5} {6}",
        new object[] { "1", "2", "3", "4", "5", "6", "7" },
        "string1_value 1 2 3 4 5 6 7")]
    [TestCase("string1 {0} {1} {2} {3} {4} {5} {6} {7}",
        new object[] { "1", "2", "3", "4", "5", "6", "7", "8" },
        "string1_value 1 2 3 4 5 6 7 8")]
    [TestCase("string1 {0} {1} {2} {3} {4} {5} {6} {7} {8}",
        new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" },
        "string1_value 1 2 3 4 5 6 7 8 9")]
    [TestCase("string1 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
        new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" },
        "string1_value 1 2 3 4 5 6 7 8 9 10")]
    public void ShouldGetString(string stringName, object[] args,
        string expectedResult)
    {
        string argsString = string.Join(
            ", ",
            args.Cast<string>().Select(arg => $"'{arg}'"));
        if (argsString != "")
        {
            argsString = ", " + argsString;
        }

        string xsltCall = $"AS:GetString('{stringName}'{argsString})";

        parameterServiceMock
            .Setup(service => service.GetString(stringName, true, args))
            .Returns(expectedResult);
        parameterServiceMock
            .Setup(service => service.GetString(stringName, args))
            .Returns(expectedResult);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldRunNumberOperand()
    {
        string xsltCall = "AS:NumberOperand('1', '1', 'PLUS')";
        object expectedResult = "2";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("Plus", "1", "1", "2")]
    [TestCase("Minus", "1", "1", "0")]
    [TestCase("Div", "4", "2", "2")]
    [TestCase("Mul", "2", "2", "4")]
    [TestCase("Mod", "5", "2", "1")]
    public void ShouldRunMathFunctions(string functionName, string parameter1,
        string parameter2, string expectedResult)
    {
        string xsltCall = $"AS:{functionName}('{parameter1}', '{parameter2}')";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldFormatNumber()
    {
        string xsltCall = "AS:FormatNumber('1.54876', 'E')";
        object expectedResult = "1,548760E+000";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("MinString", "a")]
    [TestCase("MaxString", "w")]
    public void ShouldTestStringOperationFunctions(string functionName,
        string expectedResult)
    {
        string xsltCall = $"AS:{functionName}(/ROOT/N1/@name)";

        var document = new XmlDocument();
        document.LoadXml(
            "<ROOT><N1 name=\"w\"></N1><N1 name=\"a\"></N1><N1  name=\"e\"></N1></ROOT>");

        object xPathResult = RunInXpath(xsltCall, document);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall, document);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("AS:LookupValue('{0}', '{1}')", new[] { "lookupValue" })]
    [TestCase("AS:LookupValue('{0}', '{1}', '{2}', '{3}', '{4}')",
        new[] { "par1", "val1", "par2", "val2" })]
    [TestCase("AS:LookupValue('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
        new[] { "par1", "val1", "par2", "val2", "par3", "val3" })]
    public void ShouldLookupValue(string xpathTemplate, string[] args)
    {
        string lookupId = "45b07cce-3d02-448c-afca-1b6f1eb158b5";
        var formatArguments = new List<object> { lookupId };
        formatArguments.AddRange(args);
        string xsltCall =
            string.Format(xpathTemplate, formatArguments.ToArray());
        object expectedResult = "lookupResult";

        var paramTable = new Dictionary<string, object>(3);
        if (args.Length >= 4)
        {
            paramTable[args[0]] = args[1];
            paramTable[args[2]] = args[3];
        }

        if (args.Length >= 6)
        {
            paramTable[args[4]] = args[5];
        }

        if (args.Length == 1)
        {
            lookupServiceMock
                .Setup(service => service.GetDisplayText(
                    Guid.Parse(lookupId),
                    args[0],
                    false,
                    false,
                    null))
                .Returns(expectedResult);
        }
        else
        {
            lookupServiceMock
                .Setup(service => service.GetDisplayText(
                    Guid.Parse(lookupId),
                    paramTable,
                    false,
                    false,
                    null))
                .Returns(expectedResult);
        }

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("AS:LookupList('{0}')", new string[0])]
    [TestCase("AS:LookupList('{0}', '{1}', '{2}')", new[] { "par1", "val1" })]
    [TestCase("AS:LookupList('{0}', '{1}', '{2}', '{3}', '{4}')",
        new[] { "par1", "val1", "par2", "val2" })]
    [TestCase("AS:LookupList('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
        new[] { "par1", "val1", "par2", "val2", "par3", "val3" })]
    public void ShouldTestLookupList(string xsltTemplate, string[] args)
    {
        string lookupId = "f62d6323-92ec-4c67-8fed-0e48d30dae8f";
        var argList = new List<object> { lookupId };
        argList.AddRange(args);

        string xsltCall = string.Format(xsltTemplate, argList.ToArray());
        string expectedResult = "testValue";

        var dataTable = new DataTable();
        var column = dataTable.Columns.Add("Test", typeof(string));
        column.ColumnMapping = MappingType.Element;
        var dataRow = dataTable.NewRow();
        dataRow["Test"] = expectedResult;
        dataTable.Rows.Add(dataRow);
        dataTable.AcceptChanges();
        var dataView = new DataView(dataTable);

        var parameters = new Dictionary<string, object>();
        if (args.Length >= 2)
        {
            parameters[args[0]] = args[1];
        }

        if (args.Length >= 4)
        {
            parameters[args[2]] = args[3];
        }

        if (args.Length >= 6)
        {
            parameters[args[4]] = args[5];
        }

        lookupServiceMock
            .Setup(x => x.GetList(new Guid(lookupId), parameters, null))
            .Returns(dataView);

        object xPathResult = RunInXpath(xsltCall);
        XPathNodeIterator iterator = (XPathNodeIterator)xPathResult;
        iterator.MoveNext();
        string resultXml = iterator.Current.OuterXml;
        var regex = new Regex("<Test>(\\w+)</Test>");
        var match = regex.Match(resultXml);
        Assert.That(match.Success, Is.True);
        Assert.That(match.Groups[1].Value, Is.EqualTo(expectedResult));

        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("AS:LookupValueEx('{0}', $parameters1)")]
    [TestCase("AS:LookupOrCreateEx('{0}', $parameters1, $parameters1)")]
    public void ShouldTestLookupValueEx(string xsltCallTemplate)
    {
        string lookupId = "f62d6323-92ec-4c67-8fed-0e48d30dae8f";
        string xsltCall = string.Format(xsltCallTemplate, lookupId);
        string expectedResult = "testValue";

        string xsltScriptTemplate =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n" +
            "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n" +
            "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n" +
            "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n" +
            "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n" +
            "	<xsl:template match=\"ROOT\">\n" +
            "       <xsl:variable name=\"parameters1\">" +
            "           <parameter key=\"par1\" value=\"val1\"/>\n" +
            "       </xsl:variable>\n" +
            "		<ROOT>\n" +
            "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n" +
            "				<xsl:attribute name=\"d1\">\n" +
            $"                  <xsl:value-of select=\"{xsltCall}\"/>\n" +
            "              </xsl:attribute>\n" +
            "			</SD>\n" +
            "		</ROOT>\n" +
            "	</xsl:template>\n" +
            "</xsl:stylesheet>\n";

        var document = new XmlDocument();
        document.LoadXml("<ROOT></ROOT>");

        var parameters = new Dictionary<string, object>
        {
            ["par1"] = "val1"
        };
        lookupServiceMock
            .Setup(service => service.GetDisplayText(Guid.Parse(lookupId),
                parameters, false, false, null))
            .Returns(expectedResult);

        // Xpath test is omitted because this function is not useful in xpath.

        string xsltResult = RunInXslt("", document, xsltScriptTemplate);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldTestLookupOrCreate()
    {
        string recordId = "f6fe6efe-8b95-4c78-8bb2-bd8d6eadc780";
        string lookupId = "45b07cce-3d02-448c-afca-1b6f1eb158b5";
        string xsltCall =
            $"AS:LookupOrCreate('{lookupId}', '{recordId}', /ROOT)";
        object expectedResult = "1";

        lookupServiceMock
            .Setup(service => service.GetDisplayText(
                Guid.Parse(lookupId),
                recordId,
                false,
                false,
                null))
            .Returns(expectedResult);

        // Xpath test is omitted because this function is not useful in xpath.

        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("AS:NormalRound(1.54876, 2)", "1.55")]
    [TestCase("AS:Round(1.54876)", "2")]
    [TestCase("AS:Ceiling(1.54876)", "2")]
    public void ShouldRoundNumber(string xsltCall, object expectedResult)
    {
        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    // ResizeImage(string inputData, int width, int height, string keepAspectRatio, string outFormat)

    private const string resizedImage1 =
        "iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAAAXNSR0IArs4c6QA" +
        "AAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAmSURBVBhXY/" +
        "j3798KID4PwyCBm/+RAEjgIpQNBiCBy1A2EPz/DwAwcj5F+ErgqwAAAABJRU5Er" +
        "kJgggAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAA==";

    private const string resizedImage2 =
        "iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAIAAAAmkwkpAAAAAXNSR0IArs4c6QAA" +
        "AARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiUAABYlAUlSJPAAAAAlSURBVBhXY1ixY" +
        "sV5GGC4efPmfxhguHjxIpQJ5Fy+fBnK/P8fADtAK5y4oNB3AAAAAElFTkSuQmCCAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAA==";

    private const string resizedImage1Linux =
        "iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAABGdBTUEAALGPC/xhBQ" +
        "AAAAFzUkdCAK7OHOkAAAAgY0hSTQAAeiYAAICEAAD6AAAAgOgAAHUwAADqYAAAOpgAA" +
        "BdwnLpRPAAAACFJREFUCJlj/P///38GKPj///9/JgY0gCHAgqyFgYGBAQAVfgv+xs48" +
        "NQAAAABJRU5ErkJgggAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAA==";
    private const string resizedImage2Linux =
        "iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAIAAAAmkwkpAAAABGdBTUEAALGPC/xhBQ" +
        "AAAAFzUkdCAK7OHOkAAAAgY0hSTQAAeiYAAICEAAD6AAAAgOgAAHUwAADqYAAAOpgAA" +
        "BdwnLpRPAAAACFJREFUCJlj/P//PwMDAwMDw////5kYkAAKhwWujIGBAQAx+wkBrIw4" +
        "XAAAAABJRU5ErkJgggAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAA==";

    // white square 6x6, png format.
    private byte[] image =
    {
        137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73,
        72, 68, 82, 0, 0, 0, 6, 0, 0, 0, 6, 8, 2, 0, 0, 0, 111, 174, 120,
        31, 0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0,
        4, 103, 65, 77, 65, 0, 0, 177, 143, 11, 252, 97, 5, 0, 0, 0, 9,
        112, 72, 89, 115, 0, 0, 22, 37, 0, 0, 22, 37, 1, 73, 82, 36, 240,
        0, 0, 0, 23, 73, 68, 65, 84, 24, 87, 99, 252, 255, 255, 63, 3, 42,
        96, 130, 210, 72, 128, 122, 66, 12, 12, 0, 81, 221, 3, 9, 217,
        253, 155, 178, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130
    };

    [TestCase("AS:ResizeImage('{0}', '4', '4')", resizedImage1)]
    [TestCase("AS:ResizeImage('{0}', '4', '4', 'true', 'png')", resizedImage2)]
    [Platform (Include = "Win")]
    public void ShouldConvertImage(string xsltCallTemplate,
        string expectedResult)
    {
        string xsltCall =
            string.Format(xsltCallTemplate, Convert.ToBase64String(image));
        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("AS:ResizeImage('{0}', '4', '4')", resizedImage1Linux)]
    [TestCase("AS:ResizeImage('{0}', '4', '4', 'true', 'png')", resizedImage2Linux)]
    [Platform(Include = "Linux")]
    public void ShouldConvertImageLinux(string xsltCallTemplate,
     string expectedResult)
    {
        string xsltCall =
           string.Format(xsltCallTemplate, Convert.ToBase64String(image));
        object xPathResult = RunInXpath(xsltCall);
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldGetImageDimensions()
    {
        string xsltCall =
            $"AS:GetImageDimensions('{Convert.ToBase64String(image)}')";
        string expectedResult = "6";

        string xsltScriptTemplate =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n" +
            "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n" +
            "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n" +
            "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n" +
            "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n" +
            "	<xsl:template match=\"ROOT\">\n" +
            "		<ROOT>\n" +
            "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n" +
            "				<xsl:attribute name=\"d1\">\n" +
            $"                  <xsl:value-of select=\"{xsltCall}/ROOT/Dimensions/@Width\"/>\n" +
            "              </xsl:attribute>\n" +
            "			</SD>\n" +
            "		</ROOT>\n" +
            "	</xsl:template>\n" +
            "</xsl:stylesheet>\n";

        var document = new XmlDocument();
        document.LoadXml("<ROOT></ROOT>");

        object xPathResult = RunInXpath(xsltCall);
        XPathNodeIterator iterator = (XPathNodeIterator)xPathResult;
        iterator.MoveNext();
        string resultXml = iterator.Current.OuterXml;
        var regex = new Regex("Width=\"(\\d)\" Height=\"(\\d)\"");
        var match = regex.Match(resultXml);
        Assert.That(match.Success, Is.True);
        Assert.That(match.Groups[1].Value, Is.EqualTo(expectedResult));
        Assert.That(match.Groups[2].Value, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt("", document, xsltScriptTemplate);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldDoOrigamRound()
    {
        string xsltCall = $"AS:OrigamRound('1.569456', 'rounding1')";
        object expectedResult = "2";

        lookupServiceMock
            .Setup(service =>
                service.GetDisplayText(
                    Guid.Parse("7d3d6933-648b-42cb-8947-0d2cb700152b"),
                    "rounding1", null))
            .Returns(1m);
        lookupServiceMock
            .Setup(service =>
                service.GetDisplayText(
                    Guid.Parse("994608ad-9634-439b-975a-484067f5b5a6"),
                    "rounding1", false, false, null))
            .Returns("9ecc0d91-f4bd-411e-936d-e4a8066b38dd");

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldExecureiif()
    {
        string xsltCall = $"AS:iif('true', 1, 0)";
        object expectedResult = "1";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase(new object[] { null, "1" })]
    [TestCase(new object[] { null, null, "1" })]
    [TestCase(new object[] { null, null, null, "1" })]
    public void ShouldExecureisnull(object[] arguments)
    {
        var strArguments = arguments.Select(x => x == null ? "null" : $"'{x}'");
        string xsltCall = $"AS:isnull({string.Join(", ", strArguments)})";
        object expectedResult = arguments[arguments.Length - 1];

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldEncodeDataForUri()
    {
        string xsltCall = $"AS:EncodeDataForUri('http://test?p=193')";
        string expectedResult = "http%3A%2F%2Ftest%3Fp%3D193";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldDecodeDataFromUri()
    {
        string xsltCall = "AS:DecodeDataFromUri('http%3A%2F%2Ftest%3Fp%3D193')";
        string expectedResult = $"http://test?p=193";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("AS:AddMinutes('2022-07-13', 10)",
        "2022-07-13T00:10:00.0000000")]
    [TestCase("AS:AddHours('2022-07-13', 1)",
        "2022-07-13T01:00:00.0000000")]
    [TestCase("AS:AddDays('2022-07-13', 1)",
        "2022-07-14T00:00:00.0000000")]
    [TestCase("AS:AddMonths('2022-07-13', 1)",
        "2022-08-13T00:00:00.0000000")]
    [TestCase("AS:AddYears('2022-07-13', 1)",
        "2023-07-13T00:00:00.0000000")]
    public void ShouldAddTime(string xsltCall, string expectedResultWithoutTimeZone)
    {
        var dateTime = DateTime.Parse("2022-07-13");
        TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
        string expectedResult = expectedResultWithoutTimeZone + $"+{offset:hh}:00";
        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("AS:DifferenceInDays('2022-07-13', '2022-07-14')", 1.0)]
    [TestCase("AS:DifferenceInMinutes('2022-07-13', '2022-07-14')", 1440.0)]
    [TestCase("AS:DifferenceInSeconds('2022-07-13', '2022-07-14')", 86_400.0)]
    public void ShouldGetTimeDifference(string xsltCall, double expectedResult)
    {
        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult.ToString()));
    }

    [TestCase("AS:UTCDateTime()")]
    [TestCase("AS:LocalDateTime()")]
    public void ShouldGetDateTimeNow(string xsltCall)
    {
        DateTime minTestTime = DateTime.Now - new TimeSpan(0, 0, 0, 1);
        DateTime maxTestTime = DateTime.Now + new TimeSpan(0, 0, 0, 1);

        object xPathResult = RunInXpath(xsltCall);
        DateTime xpathResultDateTime = DateTime.Parse((string)xPathResult);
        Assert.That(xpathResultDateTime, Is.GreaterThan(minTestTime));
        Assert.That(xpathResultDateTime, Is.LessThan(maxTestTime));

        string xsltResult = RunInXslt(xsltCall);
        DateTime xsltResultDateTime = DateTime.Parse((string)xsltResult);
        Assert.That(xsltResultDateTime, Is.GreaterThan(minTestTime));
        Assert.That(xsltResultDateTime, Is.LessThan(maxTestTime));
    }

    [Test]
    public void ShouldTestIfFeatureOn()
    {
        string xsltCall = "AS:IsFeatureOn('testFeature')";
        bool expectedResult = true;
        parameterServiceMock
            .Setup(service => service.IsFeatureOn("testFeature"))
            .Returns(true);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(expectedResult.ToString().ToLower()));
    }

    [Test]
    public void ShouldTestIsInRole()
    {
        string xsltCall = "AS:IsInRole('testRole')";
        bool expectedResult = true;
        authorizationProvider
            .Setup(service =>
                service.Authorize(SecurityManager.CurrentPrincipal, "testRole"))
            .Returns(true);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(expectedResult.ToString().ToLower()));
    }

    [TestCase("AS:ActiveProfileBusinessUnitId()",
        "3eed4998-4ca1-445f-a02d-d9851ea978a4")]
    [TestCase("AS:ActiveProfileId()", "e93a81d4-2520-4f14-9af9-574a61c609b0")]
    [TestCase("AS:ActiveProfileOrganizationId()",
        "4f68699e-6755-4b7d-be93-257ae28f32f5")]
    public void ShouldGetActiveProfileBusinessUnitId(string xsltCall,
        string expectedResult)
    {
        userProfileGetterMock.Setup(x => x.Invoke())
            .Returns(new UserProfile
                {
                    BusinessUnitId =
                        Guid.Parse("3eed4998-4ca1-445f-a02d-d9851ea978a4"),
                    Id = Guid.Parse("e93a81d4-2520-4f14-9af9-574a61c609b0"),
                    OrganizationId =
                        Guid.Parse("4f68699e-6755-4b7d-be93-257ae28f32f5")
                }
            );
        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldTestIsUserAuthenticated()
    {
        string xsltCall = "AS:IsUserAuthenticated()";
        bool expectedResult = false;

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(expectedResult.ToString().ToLower()));
    }

    [Test]
    public void ShouldReturnNull()
    {
        string xsltCall = "AS:null()";
        object xPathResult = RunInXpath(xsltCall);

        Assert.That(xPathResult, Is.AssignableTo(typeof(XPathNodeIterator)));
        XPathNodeIterator resultIterator = (XPathNodeIterator)xPathResult;
        Assert.That(resultIterator, Has.Count.EqualTo(0));
        // this function is not available in the xslt transformations
    }

    [Test]
    public void ShouldRunToXml()
    {
        string testXml = "<TestNode>test</TestNode>";
        string xsltCall = $"AS:ToXml('{testXml}')";
        string xsltCallXslt =
            $"AS:ToXml('&lt;TestNode&gt;test&lt;/TestNode&gt;')";
        // string xsltCallXslt = $"AS:ToXml('bla')";
        string expectedResultXpath = testXml;
        string expectedResultXslt = "test";

        string xsltScriptTemplate =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n" +
            "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n" +
            "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n" +
            "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n" +
            "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n" +
            "	<xsl:template match=\"ROOT\">\n" +
            "		<ROOT>\n" +
            "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n" +
            "				<xsl:attribute name=\"d1\">\n" +
            $"                  <xsl:value-of select=\"{xsltCallXslt}/TestNode\"/>\n" +
            "              </xsl:attribute>\n" +
            "			</SD>\n" +
            "		</ROOT>\n" +
            "	</xsl:template>\n" +
            "</xsl:stylesheet>\n";

        var document = new XmlDocument();
        document.LoadXml("<ROOT></ROOT>");

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.AssignableTo(typeof(XPathNodeIterator)));
        XPathNodeIterator resultIterator = (XPathNodeIterator)xPathResult;
        resultIterator.MoveNext();
        Assert.That(resultIterator.Current.OuterXml,
            Is.EqualTo(expectedResultXpath));

        string xsltResult = RunInXslt("", document, xsltScriptTemplate);
        Assert.That(xsltResult, Is.EqualTo(expectedResultXslt));
    }

    [TestCase("AS:GenerateSerial('counter1')")]
    [TestCase("AS:GenerateSerial('counter1', '0001-01-01')")]
    public void ShouldGenerateSerial(string xsltCall)
    {
        string counterCode = "counter1";
        string expectedResult = "result1";

        Thread.CurrentThread.CurrentCulture = new CultureInfo("cs-CZ");
        XPathNavigator nav = new XmlDocument().CreateNavigator();
        XPathExpression expr = nav.Compile(xsltCall);

        counterMock
            .Setup(x => x.GetNewCounter(counterCode, DateTime.MinValue, null))
            .Returns("result1");

        List<XsltFunctionsDefinition> containers =
            new List<XsltFunctionsDefinition>
            {
                new XsltFunctionsDefinition(
                    Container: new LegacyXsltFunctionContainer(counterMock
                        .Object),
                    NameSpaceUri: "http://schema.advantages.cz/AsapFunctions",
                    NameSpacePrefix: "AS")
            };

        OrigamXsltContext sut = new OrigamXsltContext(
            new NameTable(),
            containers
        );

        expr.SetContext(sut);
        object xPathResult = nav.Evaluate(expr);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));

        string xslScript = string.Format(xsltScriptTemplate, xsltCall);
        var transformer = new CompiledXsltEngine(containers);
        XmlDocument document = new XmlDocument();
        document.LoadXml("<ROOT></ROOT>");
        XmlContainer xmlContainer = new XmlContainer(document);
        IXmlContainer resultContainer = transformer.Transform(
            xmlContainer, xslScript, new Hashtable(), null,
            null, false);

        var regex = new Regex("d1=\"(.*)\"");
        var match = regex.Match(resultContainer.Xml.OuterXml);
        string xsltResult = match.Success
            ? match.Groups[1].Value
            : "";

        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldTestIsInState()
    {
        Guid entityId = Guid.Parse("153c5d36-0aa7-4073-a4f9-b0dc31f2edea");
        Guid fieldId = Guid.Parse("1038de85-5c44-4e75-9052-70a4c3c35dab");
        string currentState = "testState";
        Guid targetStateId = Guid.Parse("2157eb16-6643-45f7-b304-bd6fea811e16");

        string xsltCall =
            $"AS:IsInState('{entityId}', '{fieldId}', '{currentState}', '{targetStateId}')";
        bool expectedResult = true;

        stateMachineServiceMock
            .Setup(x =>
                x.IsInState(entityId, fieldId, currentState, targetStateId))
            .Returns(true);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(expectedResult.ToString().ToLower()));
    }

    [Test]
    public void ShouldTestFormatLink()
    {
        string xsltCall = $"AS:FormatLink('http://localhost', 'test1')";
        string expectedResult =
            "<a href=\"http://localhost\" target=\"_blank\"><u><font color=\"#0000ff\">test1</font></u></a>";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(HttpUtility.HtmlEncode(expectedResult)));
    }

    [Test]
    public void ShouldTestIsUserLockedOut()
    {
        string userId = "4c738d1f-b0a8-4816-9f06-4d58ef49fcda";
        string xsltCall = $"AS:IsUserLockedOut('{userId}')";
        bool expectedResult = true;

        var agentMock = new Mock<IServiceAgent>();
        var agentParameters = new Hashtable();
        agentMock
            .SetupGet(x => x.Parameters)
            .Returns(agentParameters);
        agentMock
            .SetupGet(x => x.Parameters)
            .Returns(agentParameters);
        agentMock
            .SetupGet(x => x.Result)
            .Returns(expectedResult);
        agentMock
            .SetupSet(x => x.MethodName = "IsLockedOut")
            .Verifiable();
        businessServiceMock
            .Setup(x => x.GetAgent("IdentityService", null, null))
            .Returns(agentMock.Object);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        Assert.That(agentParameters, Has.Count.EqualTo(1));
        Assert.That(agentParameters.ContainsKey("UserId"));
        Assert.That(agentParameters["UserId"], Is.EqualTo(Guid.Parse(userId)));
        agentMock.Verify();
        agentMock.Verify(x => x.Run(), Times.Once);

        agentParameters.Clear();
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(expectedResult.ToString().ToLower()));
        Assert.That(agentParameters, Has.Count.EqualTo(1));
        Assert.That(agentParameters.ContainsKey("UserId"));
        Assert.That(agentParameters["UserId"], Is.EqualTo(Guid.Parse(userId)));
        agentMock.Verify();
        agentMock.Verify(x => x.Run(), Times.Exactly(2));
    }

    [TestCase("IsUserLockedOut", "IsLockedOut")]
    [TestCase("IsUserEmailConfirmed", "IsEmailConfirmed")]
    [TestCase("Is2FAEnforced", "Is2FAEnforced")]
    public void ShouldTestUserInfoMethods(string asMethodName,
        string methodName)
    {
        string userId = "4c738d1f-b0a8-4816-9f06-4d58ef49fcda";
        string xsltCall = $"AS:{asMethodName}('{userId}')";
        bool expectedResult = true;

        var agentMock = new Mock<IServiceAgent>();
        var agentParameters = new Hashtable();
        agentMock
            .SetupGet(x => x.Parameters)
            .Returns(agentParameters);
        agentMock
            .SetupGet(x => x.Parameters)
            .Returns(agentParameters);
        agentMock
            .SetupGet(x => x.Result)
            .Returns(expectedResult);
        agentMock
            .SetupSet(x => x.MethodName = methodName)
            .Verifiable();
        businessServiceMock
            .Setup(x => x.GetAgent("IdentityService", null, null))
            .Returns(agentMock.Object);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        Assert.That(agentParameters, Has.Count.EqualTo(1));
        Assert.That(agentParameters.ContainsKey("UserId"));
        Assert.That(agentParameters["UserId"], Is.EqualTo(Guid.Parse(userId)));
        agentMock.Verify();
        agentMock.Verify(x => x.Run(), Times.Once);

        agentParameters.Clear();
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(expectedResult.ToString().ToLower()));
        Assert.That(agentParameters, Has.Count.EqualTo(1));
        Assert.That(agentParameters.ContainsKey("UserId"));
        Assert.That(agentParameters["UserId"], Is.EqualTo(Guid.Parse(userId)));
        agentMock.Verify();
        agentMock.Verify(x => x.Run(), Times.Exactly(2));
    }

    [Test]
    public void ShouldTestInitPosition()
    {
        var id = "f62d6323-92ec-4c67-8fed-0e48d30dae8f";
        string xsltCallInit = $"AS:InitPosition('{id}', 1)";
        string xsltCallNext = $"AS:NextPosition('{id}', 1)";
        decimal expectedResult1 = 2;
        decimal expectedResult2 = 3;

        RunInXpath(xsltCallInit);
        object xPathResult = RunInXpath(xsltCallNext);
        Assert.That(xPathResult, Is.EqualTo(expectedResult1));
        string xsltResult = RunInXslt(xsltCallNext);
        Assert.That(xsltResult,
            Is.EqualTo(HttpUtility.HtmlEncode(expectedResult2)));
    }

    [Test]
    public void ShouldTestGetMenuId()
    {
        var lookupId = "f62d6323-92ec-4c67-8fed-0e48d30dae8f";
        string value = "lookupValue";
        string xsltCall = $"AS:GetMenuId('{lookupId}', '{value}')";
        string expectedResult = "02e08f1c-7d6a-4f43-8b19-8e4a19bc7f2f";
        var bindingResultMock = new Mock<IMenuBindingResult>();
        bindingResultMock
            .SetupGet(x => x.MenuId)
            .Returns(expectedResult);

        lookupServiceMock
            .Setup(service =>
                service.GetMenuBinding(Guid.Parse(lookupId), value))
            .Returns(bindingResultMock.Object);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(HttpUtility.HtmlEncode(expectedResult)));
    }

    [Test]
    public void ShouldDecodeSignedOverpunch()
    {
        string xsltCall = "AS:DecodeSignedOverpunch('0000000000{', 2)";
        double expectedResult = 0.0;

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult,
            Is.EqualTo(expectedResult).Within(0.000000000000001));
        string xsltResult = RunInXslt(xsltCall);
        double xsltResultDouble = double.Parse(xsltResult);
        Assert.That(xsltResultDouble,
            Is.EqualTo(expectedResult).Within(0.000000000000001));
    }

    [Test]
    public void ShouldRandomlyDistributeValues()
    {
        string xsltScriptTemplate =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n" +
            "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n" +
            "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n" +
            "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n" +
            "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n" +
            "	<xsl:template match=\"ROOT\">\n" +
            "       <xsl:variable name=\"parameters1\">\n" +
            "           <parameter value=\"val0\" quantity=\"0\"/>\n" +
            "           <parameter value=\"val1\" quantity=\"1\"/>\n" +
            "           <parameter value=\"val2\" quantity=\"2\"/>\n" +
            "       </xsl:variable>\n" +
            "		<ROOT>\n" +
            "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n" +
            "				<xsl:attribute name=\"d1\">\n" +
            $"                  <xsl:value-of select=\"AS:RandomlyDistributeValues($parameters1)\"/>\n" +
            "              </xsl:attribute>\n" +
            "			</SD>\n" +
            "		</ROOT>\n" +
            "	</xsl:template>\n" +
            "</xsl:stylesheet>\n";

        var document = new XmlDocument();
        document.LoadXml("<ROOT></ROOT>");

        // Xpath test is omitted because this function is not useful in xpath.

        string xsltResult = RunInXslt("", document, xsltScriptTemplate);
        Assert.That(xsltResult.Contains("val"));
    }

    [Test]
    public void ShouldTestRandom()
    {
        string xsltCall = "AS:Random()";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.LessThanOrEqualTo(1));
        Assert.That(xPathResult, Is.GreaterThanOrEqualTo(0));

        string xsltResult = RunInXslt(xsltCall);
        double xsltDoubleResult = double.Parse(
            xsltResult, NumberStyles.Any, CultureInfo.InvariantCulture);
        Assert.That(xsltDoubleResult, Is.LessThanOrEqualTo(1));
        Assert.That(xsltDoubleResult, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void ShouldTestIsUriValid()
    {
        string xsltCall = "AS:IsUriValid('https://www.google.com/')";
        bool expectedResult = true;

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(expectedResult.ToString().ToLower()));
    }

    [Test]
    public void ShouldTestReferenceCount()
    {
        string entityId = "7948c229-39e5-4fe7-9cb0-4d0ad21cd899";
        string value = "testValue";
        string xsltCall = $"AS:ReferenceCount('{entityId}', '{value}')";
        long expectedResult = 1;

        dataServiceMock
            .Setup(x => x.ReferenceCount(Guid.Parse(entityId), value, null))
            .Returns(expectedResult);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(expectedResult.ToString().ToLower()));
    }

    [Test]
    public void ShouldTestGenerateId()
    {
        string xsltCall = "AS:GenerateId()";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.InstanceOf<string>());
        bool xPathResultIsGuid = Guid.TryParse((string)xPathResult, out _);
        Assert.That(xPathResultIsGuid);
        string xsltResult = RunInXslt(xsltCall);
        bool xsltResultIsGuid = Guid.TryParse(xsltResult, out _);
        Assert.That(xsltResultIsGuid);
    }

    [Test]
    public void ShouldListDays()
    {
        var dateTime = DateTime.Parse("2022-01-01");
        TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
        string xsltCall = "AS:ListDays('2022-01-01', '2022-01-03')";
        string expectedResultXpath = "<list>" + Environment.NewLine +
                                     $"  <item>2022-01-01T00:00:00.0000000+{offset:hh}:00</item>" + Environment.NewLine +
                                     $"  <item>2022-01-02T00:00:00.0000000+{offset:hh}:00</item>" + Environment.NewLine +
                                     $"  <item>2022-01-03T00:00:00.0000000+{offset:hh}:00</item>" + Environment.NewLine +
                                     "</list>";
        string expectedResultXslt =
            $"2022-01-01T00:00:00.0000000+{offset:hh}:002022-01-02T00:00:00.0000000+{offset:hh}:002022-01-03T00:00:00.0000000+{offset:hh}:00";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.InstanceOf<XPathNodeIterator>());
        var nodeIterator = xPathResult as XPathNodeIterator;
        nodeIterator.MoveNext();
        Assert.That(nodeIterator.Current.OuterXml,
            Is.EqualTo(expectedResultXpath));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResultXslt));
    }

    [Test]
    public void ShouldTestIsDateBetween()
    {
        string xsltCall =
            "AS:IsDateBetween('2022-01-02', '2022-01-01', '2022-01-03')";
        bool expectedResult = true;

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult,
            Is.EqualTo(expectedResult.ToString().ToLower()));
    }

    [Test]
    public void ShouldTestAddWorkingDays()
    {
        Guid calendarId = Guid.Parse("186ec5ec-7877-4204-8323-2ffdd816332d");
        string xsltCall =
            $"AS:AddWorkingDays('2022-01-02', '2', '{calendarId}')";

        var expectedDate = DateTime.Parse("2022-01-04");
        TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(expectedDate);
        string expectedResult = $"2022-01-04T00:00:00.0000000+{offset:hh}:00";

        var agentMock = new Mock<IServiceAgent>();
        var agentParameters = new Hashtable();
        agentMock
            .SetupGet(x => x.Parameters)
            .Returns(agentParameters);
        agentMock
            .SetupGet(x => x.Result)
            .Returns(new DataSet());
        agentMock
            .SetupSet(x => x.MethodName = "LoadDataByQuery")
            .Verifiable();

        businessServiceMock
            .Setup(x => x.GetAgent("DataService", null, null))
            .Returns(agentMock.Object);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        Assert.That(agentParameters, Has.Count.EqualTo(1));
        Assert.That(agentParameters.ContainsKey("Query"));
        agentMock.Verify();
        agentMock.Verify(x => x.Run(), Times.Once);

        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
        Assert.That(agentParameters, Has.Count.EqualTo(1));
        Assert.That(agentParameters.ContainsKey("Query"));
        agentMock.Verify();
        agentMock.Verify(x => x.Run(), Times.Exactly(2));
    }

    [TestCase("AS:LastDayOfMonth('2022-01-02')", "2022-01-31")]
    [TestCase("AS:FirstDayNextMonthDate('2022-01-02')", "2022-02-01")]
    [TestCase("AS:Year('2022-01-02')", "2022")]
    [TestCase("AS:Month('2022-01-02')", "01")]
    public void ShouldTestDateFunctions(string xsltCall, string expectedResult)
    {
        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldTestDecimalNumber()
    {
        string xsltCall = "AS:DecimalNumber('5')";
        string expectedResult = "5";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }


    [TestCase("AS:avg(/ROOT/N1/@count)", "2")]
    [TestCase("AS:sum(/ROOT/N1/@count)", "6")]
    [TestCase("AS:Sum(/ROOT/N1/@count)", "6")]
    [TestCase("AS:min(/ROOT/N1/@count)", "1")]
    [TestCase("AS:Min(/ROOT/N1/@count)", "1")]
    [TestCase("AS:max(/ROOT/N1/@count)", "3")]
    [TestCase("AS:Max(/ROOT/N1/@count)", "3")]
    public void ShouldListFunctions(string xsltCall, string expectedResult)
    {
        var document = new XmlDocument();
        document.LoadXml(
            "<ROOT><N1 count=\"1\"></N1><N1 count=\"2\"></N1><N1  count=\"3\"></N1></ROOT>");

        object xPathResult = RunInXpath(xsltCall, document);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall, document);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [TestCase("AS:Uppercase('test')", "TEST")]
    [TestCase("AS:Lowercase('TEST')", "test")]
    public void ShouldTestStringManipulationFunctions(string xsltCall,
        string expectedResult)
    {
        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
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
        dataTable.Columns.Add("Source", typeof(string));
        dataTable.Columns.Add("Target", typeof(string));
        var dataRow = dataTable.NewRow();
        dataRow["Source"] = text;
        dataRow["Target"] = expectedResult;
        dataTable.Rows.Add(dataRow);
        dataTable.AcceptChanges();
        dataSet.Tables.Add(dataTable);

        dataServiceMock.Setup(x => x.LoadData(
            new Guid("9268abd0-a08e-4c97-b5f7-219eacf171c0"),
            new Guid("c2cd04cd-9a47-49d8-aa03-2e07044b3c7c"),
            Guid.Empty,
            new Guid("26b8f31b-a6ce-4a0a-905d-0915855cd934"),
            null,
            "OrigamCharacterTranslationDetail_parOrigamCharacterTranslationId",
            new Guid(dictionaryId))).Returns(dataSet);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    class TestXmlContainer : XmlContainer
    {
        protected bool Equals(XmlContainer other)
        {
            return Equals(Xml.OuterXml, other.Xml.OuterXml);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is XmlContainer)) return false;
            return Equals((XmlContainer)obj);
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
        doc.LoadXml("<ROOT><TestNode name=\"testNode\"></TestNode></ROOT>");

        // Mocking AllowedStateValues does not work here because XmlContainer does not override Equals
        // IXmlContainer document = new TestXmlContainer();
        // document.LoadXml("<row name=\"testNode\" />");
        // stateMachineServiceMock
        //     .Setup(x => x.AllowedStateValues(new Guid(entityId), new Guid(fieldId), currentStateValue, document, null))
        //     .Returns(new object[]{"1", "2", "3"});

        object xPathResult = RunInXpath(xsltCall, doc.Xml);
        Assert.That(xPathResult, Is.InstanceOf<XPathNodeIterator>());
        var nodeIterator = xPathResult as XPathNodeIterator;
        nodeIterator.MoveNext();
        Assert.That(nodeIterator.Current.OuterXml,
            Is.EqualTo(expectedResultXpath));

        string xsltResult = RunInXslt(xsltCall, doc.Xml);
        Assert.That(xsltResult, Is.EqualTo(expectedResultXslt));
    }

    [Test]
    public void ShouldTestNodeSet()
    {
        string xsltCall = "AS:NodeSet(<TestNode>test</TestNode>)/TestNode";
        string expectedResult = "test";

        string xsltScript =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n" +
            "	xmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"\n" +
            "    xmlns:fs=\"http://xsl.origam.com/filesystem\"\n" +
            "    xmlns:CR=\"http://xsl.origam.com/crypto\"\n" +
            "	 xmlns:date=\"http://exslt.org/dates-and-times\">\n" +
            "	<xsl:template match=\"ROOT\">\n" +
            "       <xsl:variable name=\"testInput\"><TestNode>test</TestNode></xsl:variable>" +
            "		<ROOT>\n" +
            "			<SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n" +
            "				<xsl:attribute name=\"d1\">\n" +
            $"                  <xsl:value-of select=\"AS:NodeSet($testInput)/TestNode\"/>\n" +
            "              </xsl:attribute>\n" +
            "			</SD>\n" +
            "		</ROOT>\n" +
            "	</xsl:template>\n" +
            "</xsl:stylesheet>\n";

        // Xpath test is omitted because this function is not useful in xpath.

        var document = new XmlDocument();
        document.LoadXml("<ROOT></ROOT>");


        string xsltResult = RunInXslt("", document, xsltScript);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldTestNodeToString()
    {
        string xsltCall = "AS:NodeToString(/ROOT)";
        string expectedResultXpath = "<ROOT>" + Environment.NewLine +
                                     "  <N1 count=\"1\">" + Environment.NewLine +
                                     "  </N1>" + Environment.NewLine +
                                     "  <N1 count=\"2\">" + Environment.NewLine + 
                                     "  </N1>" + Environment.NewLine +
                                     "  <N1 count=\"3\">" + Environment.NewLine +
                                     "  </N1>" + Environment.NewLine +
                                     "</ROOT>";
        string newLineCR = "&#xD;";
        string newLineLF = "&#xA;";
        string newLinePlatform = newLineCR + newLineLF;
        if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            newLinePlatform = newLineLF;
        }

        string expectedResultXslt =
            "&lt;ROOT&gt;"+ newLinePlatform + "  &lt;N1 count=&quot;1&quot;&gt;"+ newLinePlatform + "  " +
            "&lt;/N1&gt;"+ newLinePlatform + "  &lt;N1 count=&quot;2&quot;&gt;"+ newLinePlatform + "  " +
            "&lt;/N1&gt;"+ newLinePlatform + "  &lt;N1 count=&quot;3&quot;&gt;"+ newLinePlatform + "  " +
            "&lt;/N1&gt;"+ newLinePlatform + "&lt;/ROOT&gt;";
        var document = new XmlDocument();
        document.LoadXml(
            "<ROOT><N1 count=\"1\"></N1><N1 count=\"2\"></N1><N1  count=\"3\"></N1></ROOT>");

        object xPathResult = RunInXpath(xsltCall, document);
        Assert.That(xPathResult, Is.EqualTo(expectedResultXpath));
        string xsltResult = RunInXslt(xsltCall, document);
        Assert.That(xsltResult, Is.EqualTo(expectedResultXslt));
    }

    [Test]
    public void ShouldTestDecodeTextFromBase64()
    {
        string xsltCall = "AS:DecodeTextFromBase64('dGVzdA==', 'UTF-8')";
        string expectedResult = "test";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldTestPointFromJtsk()
    {
        string xsltCall = "AS:PointFromJtsk(1, 1)";
        string expectedResult = "POINT(19.55554477337343 54.55554477337342)";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldTestAbs()
    {
        string xsltCall = "AS:abs(-1)";
        string expectedResult = "1";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldTestEvaluateXPath()
    {
        string xsltCall = "AS:EvaluateXPath('nodes', 'path')";
        string expectedResult = "testResult";

        xPathEvaluatorMock
            .Setup(x => x.Evaluate("nodes", "path"))
            .Returns("testResult");

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldTestHttpRequest()
    {
        string url = "http://localhost";
        string xsltCall = $"AS:HttpRequest('{url}')";
        string expectedResult = "testResult";

        httpToolsMock
            .Setup(x => x.SendRequest(new Request(url, null, null, null,
                new Hashtable(), null, null, null, false,
                null, true, null, false)))
            .Returns(new HttpResult(expectedResult, null, null, null, null));

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ShouldTestProcessMarkdown()
    {
        string xsltCall = $"AS:ProcessMarkdown('# test')";
        string expectedXpathResult = "<h1>test</h1>\n";
        string expectedXsltResult = "&lt;h1&gt;test&lt;/h1&gt;&#xA;";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedXpathResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedXsltResult));
    }

    [Test]
    public void ShouldTestDiff()
    {
        string xsltCall = $"AS:Diff('old text', 'new text')";
        string expectedXpathResult = "<lines>" + Environment.NewLine +
                                     "  <line changeType=\"Deleted\">old text</line>" + Environment.NewLine +
                                     "  <line changeType=\"Inserted\" position=\"1\">new text</line>" + Environment.NewLine +
                                     "</lines>";
        string expectedXsltResult = "old textnew text";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.InstanceOf<XPathNodeIterator>());
        var nodeIterator = xPathResult as XPathNodeIterator;
        nodeIterator.MoveNext();
        Assert.That(nodeIterator.Current.OuterXml,
            Is.EqualTo(expectedXpathResult));

        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedXsltResult));
    }

    [Test]
    public void ShouldTestResourceIdByActiveProfile()
    {
        string xsltCall = $"AS:ResourceIdByActiveProfile()";
        string expectedResult = "testId";

        resourceToolsMock
            .Setup(x => x.ResourceIdByActiveProfile())
            .Returns(expectedResult);

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));
        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }
}