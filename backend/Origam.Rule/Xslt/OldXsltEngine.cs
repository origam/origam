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

namespace Origam.Rule.Xslt
{
    class OldXsltEngine : MicrosoftXsltEngine
    {
#region Constructors

        public OldXsltEngine()
        {
        }

public OldXsltEngine(IEnumerable<XsltFunctionsDefinition> xsltFunctionDefinitions) : 
            base (xsltFunctionDefinitions)
		{
		}

        public OldXsltEngine(IPersistenceProvider persistence=null) : base (persistence)
		{
		}
#endregion

        internal override object GetTransform(IXmlContainer xslt)
        {
            XslTransform engine = new XslTransform();
#if NETSTANDARD
            engine.Load(new XmlNodeReader(xslt.Xml), new ModelXmlResolver());
#else
            engine.Load(new XmlNodeReader(xslt.Xml), new ModelXmlResolver(), this.GetType().Assembly.Evidence);
#endif
            return engine;
        }

        internal override object GetTransform(string xsl)
        {
            XslTransform engine = new XslTransform();
            StringReader xslReader = new StringReader(xsl);
            XPathDocument xslDoc = new XPathDocument(xslReader);
#if NETSTANDARD
            engine.Load(xslDoc, new ModelXmlResolver());
#else
            engine.Load(xslDoc, new ModelXmlResolver(), this.GetType().Assembly.Evidence);
#endif
            return engine;
        }

        public override void Transform(object engine, XsltArgumentList xslArg, XPathDocument sourceXpathDoc, IXmlContainer resultDoc)
        {
            XslTransform xslt = engine as XslTransform;
            XmlReader reader = xslt.Transform(sourceXpathDoc, xslArg, (XmlResolver)null);
            try
            {
                resultDoc.Xml.Load(reader);
            }
            catch (NullReferenceException)
            {
                // WORKAROUND: when loading data to a predefined DataSet (using XmlDataDocument)
                // and the result of the transformation is nothing (completely empty, not even a root node),
                // null reference exception is fired. We just ignore it.
                if(reader.ReadState == ReadState.EndOfFile)
                {
                    return;
                }
                throw;
            }
        }

        public override void Transform(object engine, XsltArgumentList xslArg, XPathDocument sourceXpathDoc, XmlTextWriter xwr)
        {
            XslTransform xslt = engine as XslTransform;
            xslt.Transform(sourceXpathDoc, xslArg, xwr, null);
        }

        public override void Transform(object engine, XsltArgumentList xslArg, IXPathNavigable input, Stream output)
        {
            XslTransform xslt = engine as XslTransform;
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