#region license

/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Origam.Rule.Xslt;
using Origam.Rule.XsltFunctions;
using Origam.Service.Core;
using Assert = NUnit.Framework.Assert;

namespace Origam.RuleTests;

[TestFixture]
public class FileSystemXsltFunctionContainerTests
{
    private List<XsltFunctionsDefinition> xsltFunctionDefinitions;

    private string xsltScriptTemplate =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
        + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"\n"
        + "    xmlns:fs=\"http://xsl.origam.com/filesystem\">\n"
        + "    <xsl:template match=\"ROOT\">\n"
        + "        <ROOT>\n"
        + "            <SD Id=\"05b0209f-8f7d-4759-9814-fa45af953c63\">\n"
        + "                <xsl:attribute name=\"d1\">\n"
        + "                  <xsl:value-of select=\"{0}\"/>\n"
        + "              </xsl:attribute>\n"
        + "            </SD>\n"
        + "        </ROOT>\n"
        + "    </xsl:template>\n"
        + "</xsl:stylesheet>\n";

    [SetUp]
    public void Init()
    {
        xsltFunctionDefinitions = new List<XsltFunctionsDefinition>
        {
            new(
                Container: new FileSystemXsltFunctionContainer(),
                NameSpacePrefix: "fs",
                NameSpaceUri: "http://xsl.origam.com/filesystem"
            ),
        };
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

    private string RunInXslt(string xsltCall, XmlDocument document = null)
    {
        string xsltScript = string.Format(format: xsltScriptTemplate, arg0: xsltCall);

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
        Match match = regex.Match(input: resultContainer.Xml.OuterXml);
        return match.Success ? match.Groups[groupnum: 1].Value : "";
    }

    [Test]
    public void ShouldCombinePath()
    {
        string expectedResult = Path.Combine(path1: "root", path2: "file.txt");
        string xsltCall = "fs:CombinePath('root', 'file.txt')";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));

        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldCombinePathWithThreeSegments()
    {
        string expectedResult = Path.Combine(path1: "root", path2: "folder", path3: "file.txt");
        string xsltCall = "fs:CombinePath('root', 'folder', 'file.txt')";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));

        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }

    [Test]
    public void ShouldCombinePathWithFourSegments()
    {
        string expectedResult = Path.Combine(
            path1: "root",
            path2: "folder",
            path3: "child",
            path4: "file.txt"
        );
        string xsltCall = "fs:CombinePath('root', 'folder', 'child', 'file.txt')";

        object xPathResult = RunInXpath(xsltCall: xsltCall);
        Assert.That(actual: xPathResult, expression: Is.EqualTo(expected: expectedResult));

        string xsltResult = RunInXslt(xsltCall: xsltCall);
        Assert.That(actual: xsltResult, expression: Is.EqualTo(expected: expectedResult));
    }
}
