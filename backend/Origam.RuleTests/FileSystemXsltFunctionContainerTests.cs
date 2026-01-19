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
            new(new FileSystemXsltFunctionContainer(), "fs", "http://xsl.origam.com/filesystem"),
        };
    }

    private object RunInXpath(string xsltCall, XmlDocument document = null)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("cs-CZ");
        XPathNavigator nav = (document ?? new XmlDocument()).CreateNavigator();
        XPathExpression expr = nav.Compile(xsltCall);
        OrigamXsltContext sut = new OrigamXsltContext(new NameTable(), xsltFunctionDefinitions);
        expr.SetContext(sut);
        return nav.Evaluate(expr);
    }

    private string RunInXslt(string xsltCall, XmlDocument document = null)
    {
        string xsltScript = string.Format(xsltScriptTemplate, xsltCall);

        var transformer = new CompiledXsltEngine(xsltFunctionDefinitions);
        if (document == null)
        {
            document = new XmlDocument();
            document.LoadXml("<ROOT></ROOT>");
        }

        XmlContainer xmlContainer = new XmlContainer(document);
        IXmlContainer resultContainer = transformer.Transform(
            xmlContainer,
            xsltScript,
            new Hashtable(),
            null,
            null,
            false
        );

        var regex = new Regex("d1=\"(.*)\"");
        Match match = regex.Match(resultContainer.Xml.OuterXml);
        return match.Success ? match.Groups[1].Value : "";
    }

    [Test]
    public void ShouldCombinePath()
    {
        string expectedResult = Path.Combine("root", "file.txt");
        string xsltCall = "fs:CombinePath('root', 'file.txt')";

        object xPathResult = RunInXpath(xsltCall);
        Assert.That(xPathResult, Is.EqualTo(expectedResult));

        string xsltResult = RunInXslt(xsltCall);
        Assert.That(xsltResult, Is.EqualTo(expectedResult));
    }
}
