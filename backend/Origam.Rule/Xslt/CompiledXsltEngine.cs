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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Origam.DA.ObjectPersistence;
using Origam.Rule.XsltFunctions;
using Origam.Service.Core;

namespace Origam.Rule.Xslt;

public class CompiledXsltEngine : MicrosoftXsltEngine
{
    #region Constructors
    public CompiledXsltEngine() { }

    public CompiledXsltEngine(IEnumerable<XsltFunctionsDefinition> functionsDefinitions)
        : base(functionsDefinitions: functionsDefinitions) { }

    public CompiledXsltEngine(IPersistenceProvider persistence)
        : base(persistence: persistence) { }
    #endregion
    internal override object GetTransform(IXmlContainer xslt)
    {
        XslCompiledTransform engine = new XslCompiledTransform();
        engine.Load(
            stylesheet: new XmlNodeReader(node: xslt.Xml),
            settings: new XsltSettings(),
            stylesheetResolver: new ModelXmlResolver()
        );
        return engine;
    }

    internal override object GetTransform(string xsl)
    {
        XslCompiledTransform engine = new XslCompiledTransform();
        StringReader xslReader = new StringReader(s: xsl);
        XPathDocument xslDoc = new XPathDocument(textReader: xslReader);
        engine.Load(
            stylesheet: xslDoc,
            settings: new XsltSettings(),
            stylesheetResolver: new ModelXmlResolver()
        );
        return engine;
    }

    public override void Transform(
        object engine,
        XsltArgumentList xslArg,
        XPathDocument sourceXpathDoc,
        IXmlContainer resultDoc
    )
    {
        XslCompiledTransform xslt = engine as XslCompiledTransform;
        MemoryStream stream = new MemoryStream();
        xslt.Transform(input: sourceXpathDoc, arguments: xslArg, results: stream);
        stream.Position = 0;
        using (XmlReader reader = XmlReader.Create(input: stream))
        {
            resultDoc.Load(xmlReader: reader);
        }
    }

    public override void Transform(
        object engine,
        XsltArgumentList xslArg,
        XPathDocument sourceXpathDoc,
        XmlTextWriter xwr
    )
    {
        XslCompiledTransform xslt = engine as XslCompiledTransform;
        xslt.Transform(input: sourceXpathDoc, arguments: xslArg, results: xwr);
    }

    public override void Transform(
        object engine,
        XsltArgumentList xslArg,
        IXPathNavigable input,
        Stream output
    )
    {
        XslCompiledTransform xslt = engine as XslCompiledTransform;
        xslt.Transform(input: input, arguments: xslArg, results: output);
    }

    #region Transformation Cache
    private static Hashtable _transformationCache = new Hashtable();

    protected override bool IsTransformationCached(Guid transformationId)
    {
        return _transformationCache.ContainsKey(key: transformationId);
    }

    protected override object GetCachedTransformation(Guid tranformationId)
    {
        return _transformationCache[key: tranformationId];
    }

    protected override void PutTransformationToCache(Guid transformationId, object transformation)
    {
        _transformationCache[key: transformationId] = transformation;
    }
    #endregion
}
