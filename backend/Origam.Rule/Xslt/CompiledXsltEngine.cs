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
using Origam.Schema.EntityModel;
using Origam.Service.Core;

namespace Origam.Rule.Xslt
{
    public class CompiledXsltEngine : MicrosoftXsltEngine
    {
        #region Constructors

        public CompiledXsltEngine()
        {
        }

        public CompiledXsltEngine(IEnumerable<XsltFunctionsDefinition> functionsDefinitions) 
            : base (functionsDefinitions)
		{
		}

        public CompiledXsltEngine(IPersistenceProvider persistence)
            : base(persistence)
		{
		}
		#endregion

        internal override object GetTransform(IXmlContainer xslt)
        {
            XslCompiledTransform engine = new XslCompiledTransform();
            engine.Load(new XmlNodeReader(xslt.Xml), new XsltSettings(), new ModelXmlResolver());
            return engine;
        }

        internal override object GetTransform(string xsl)
        {
            XslCompiledTransform engine = new XslCompiledTransform();
            StringReader xslReader = new StringReader(xsl);
            XPathDocument xslDoc = new XPathDocument(xslReader);
            engine.Load(xslDoc, new XsltSettings(), new ModelXmlResolver());
            return engine;
        }

        public override void Transform(object engine, XsltArgumentList xslArg, XPathDocument sourceXpathDoc, IXmlContainer resultDoc)
        {
            XslCompiledTransform xslt = engine as XslCompiledTransform;
            MemoryStream stream = new MemoryStream();
            xslt.Transform(sourceXpathDoc, xslArg, stream);
            stream.Position = 0;
            using (XmlReader reader = XmlReader.Create(stream))
            {
                resultDoc.Load(reader);
            }
        }

        public override void Transform(object engine, XsltArgumentList xslArg, XPathDocument sourceXpathDoc, XmlTextWriter xwr)
        {
            XslCompiledTransform xslt = engine as XslCompiledTransform;
            xslt.Transform(sourceXpathDoc, xslArg, xwr);
        }

        public override void Transform(object engine, XsltArgumentList xslArg, IXPathNavigable input, Stream output)
        {
            XslCompiledTransform xslt = engine as XslCompiledTransform;
            xslt.Transform(input, xslArg, output);
        }
#region Transformation Cache
        private static Hashtable _transformationCache = new Hashtable();
        protected override bool IsTransformationCached(Guid transformationId)
        {
            return _transformationCache.ContainsKey(transformationId);
        }

        protected override object GetCachedTransformation(Guid tranformationId)
        {
            return _transformationCache[tranformationId];
        }

        protected override void PutTransformationToCache(
            Guid transformationId, object transformation)
        {
            _transformationCache[transformationId] = transformation;
        }
#endregion

    }
}
