#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Origam.Schema.GuiModel;
using System.Web;
using System.Xml.XPath;
using System.Xml;
using Origam.Rule;
using System.Data;
using Origam.Workbench.Services;
using Origam.DA;
using Origam.JSON;
using core = Origam.Workbench.Services.CoreServices;
using System.Collections;
using Microsoft.AspNetCore.Http;
using Origam;
using Origam.Server;
using Origam.ServerCommon;

namespace Origam.ServerCommon.Pages
{
    class XsltPageRequestHandler : AbstractPageRequestHandler
    {
        private const string  MIME_JSON = "application/json";
        private const string MIME_HTML = "text/html";
        private const string MIME_OCTET_STREAM = "application/octet-stream";

        public override void Execute(AbstractPage page, Dictionary<string, object> parameters, IRequestWrapper request, IResponseWrapper response)
        {
            XsltDataPage xsltPage = page as XsltDataPage;
            IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            IXsltEngine transformer = null;
            RuleEngine ruleEngine = new RuleEngine(null, null);
            Hashtable transformParams = new Hashtable();
            QueryParameterCollection qparams = new QueryParameterCollection();
            Hashtable preprocessorParams = GetPreprocessorParameters(request);

            // convert parameters to QueryParameterCollection for data service and hashtable for transformation service
            foreach (KeyValuePair<string, object> p in parameters)
            {
                qparams.Add(new QueryParameter(p.Key, p.Value));
                transformParams.Add(p.Key, p.Value);
            }

            // copy also the preprocessor parameters to the transformation parameters
            foreach (DictionaryEntry rp in preprocessorParams)
            {
                transformParams.Add(rp.Key, rp.Value);
            }

			Validate(null, transformParams, ruleEngine, xsltPage.InputValidationRule);
			if (xsltPage.DisableConstraintForInputValidation)
			{
				// reenable constraints for context parameter
				foreach (KeyValuePair<string, object> p in parameters)
				{
					if (p.Value as IDataDocument != null)
					{
						(p.Value as IDataDocument).DataSet.EnforceConstraints = true;
					} 
				}
			}

            IXmlContainer xmlData;
            DataSet data = null;

            if (xsltPage.DataStructure == null)
            {
                // no data source
                xmlData = new XmlContainer("<ROOT/>");
            }
            else
            {
                data = core.DataService.LoadData(xsltPage.DataStructureId, xsltPage.DataStructureMethodId, Guid.Empty, xsltPage.DataStructureSortSetId, null, qparams);

                if (request.HttpMethod == "DELETE")
                {
                    HandleDELETE(xsltPage, data, transformParams, ruleEngine);
                    return;
                }

                xmlData = DataDocumentFactory.New(data);

                if (request.HttpMethod == "PUT")
                {
                    HandlePUT(parameters, xsltPage, (IDataDocument)xmlData, transformParams, ruleEngine);
                    return;
                }
                
            }

            bool xpath = xsltPage.ResultXPath != null && xsltPage.ResultXPath != String.Empty;

            IXmlContainer result = null;
            bool isProcessed = false;

            if (xsltPage.Transformation == null && !xpath && page.MimeType == MIME_JSON)
            {
                // pure dataset > json serialization
                response.WriteToOutput(textWriter => JsonUtils.SerializeToJson(textWriter, data, false));
                isProcessed = true;
            }
            else if (xsltPage.Transformation == null)
            {
                // no transformation
                result = xmlData;
            }
            else
            {
                AsTransform.GetXsltEngine(
                    persistence.SchemaProvider, xsltPage.TransformationId);
                result = transformer.Transform(xmlData,
					xsltPage.TransformationId,
					new Guid("5b4f2532-a0e1-4ffc-9486-3f35d766af71"),
					transformParams, preprocessorParams, ruleEngine,
					xsltPage.TransformationOutputStructure, false);

                IDataDocument resultDataDocument = result as IDataDocument;
				// pure dataset > json serialization
				if (resultDataDocument != null && !xpath && page.MimeType == MIME_JSON)
				{
				    response.WriteToOutput(textWriter =>
				        JsonUtils.SerializeToJson(textWriter, resultDataDocument.DataSet, false));
					isProcessed = true;
				}
			}

            if (xpath)
            {
                // subset of the returned xml - json | html not supported
                // it is mainly used for extracting pure text out of the result xml
                // so json | html serialization would have to be produced by the
                // xslt or stored directly in the resulting data
                XPathNavigator nav = result.Xml.CreateNavigator();
                nav.Select(xsltPage.ResultXPath);

                if (page.MimeType == MIME_OCTET_STREAM)
                {
                    byte[] bytes = UTF8Encoding.UTF8.GetBytes(nav.Value);
                    response.AddHeader("Content-Length", bytes.LongLength.ToString());
                    response.BinaryWrite(bytes);
                }
                else
                {
                    response.AddHeader("Content-Length", nav.Value.Length.ToString());
                    response.Write(nav.Value);
                }
            }
            else if(! isProcessed)
            {
                if (page.MimeType == MIME_JSON)
                {
                    response.WriteToOutput(textWriter =>
                        JsonUtils.SerializeToJson(textWriter, result, xsltPage.OmitJsonRootElement));
                }
                else
                {
                    if (page.MimeType == MIME_HTML)
                    {
                        response.Write("<!DOCTYPE html>");
                    }
                    response.WriteToOutput(textWriter => result.Xml.Save(textWriter));
                }
            }

            if (Analytics.Instance.IsAnalyticsEnabled && xsltPage.LogTransformation != null)
            {
                Type type = this.GetType();
                IXsltEngine logTransformer = AsTransform.GetXsltEngine(
                    persistence.SchemaProvider, xsltPage.LogTransformationId);
                IXmlContainer log = logTransformer.Transform(xmlData, xsltPage.LogTransformationId, transformParams, ruleEngine, null, false);

                XPathNavigator nav = log.Xml.CreateNavigator();
                XPathNodeIterator iter = nav.Select("/ROOT/LogContext");
                while (iter.MoveNext())
                {
                    Dictionary<string, string> properties = new Dictionary<string, string>();
                    XPathNavigator current = iter.Current;
                    string message;
                    if (current.Value == string.Empty)
                    {
                        message = "DATA_ACCESS";
                    }
                    else
                    {
                        message = current.Value;
                    }
                    current.MoveToFirstAttribute();
                    do
                    {
                        properties[current.Name] = current.Value;
                    } while (current.MoveToNextAttribute());
                    Analytics.Instance.Log(type, message, properties);
                }
            }
        }

        private static void HandlePUT(Dictionary<string, object> parameters, XsltDataPage xsltPage, IDataDocument data, Hashtable transformParams, RuleEngine ruleEngine)
        {
            Validate(data, transformParams, ruleEngine, xsltPage.SaveValidationBeforeMerge);
            
            string bodyKey = null;
            foreach (string key in parameters.Keys)
            {
                if (parameters[key] is XmlDocument)
                {
                    bodyKey = key;
                    break;
                }
            }
            if (bodyKey == null)
            {
                throw new Exception(Resources.ErrorPseudoparameterBodyNotDefined);
            }
            UserProfile profile = SecurityManager.CurrentUserProfile();
            DataSet original = (parameters[bodyKey] as IDataDocument).DataSet;
            MergeParams mergeParams = new MergeParams(profile.Id);
            mergeParams.TrueDelete = true;
            DatasetTools.MergeDataSet(data.DataSet, original, null, mergeParams);
            
            Validate(data, transformParams, ruleEngine, xsltPage.SaveValidationAfterMerge);

            core.DataService.StoreData(xsltPage.DataStructureId, data.DataSet, false, null);
            return;
        }

        private static void HandleDELETE(XsltDataPage xsltPage, DataSet data, Hashtable transformParams, RuleEngine ruleEngine)
        {
            IXmlContainer xmldoc = new XmlContainer();
            xmldoc.Xml.LoadXml(data.GetXml());
            Validate(xmldoc, transformParams, ruleEngine, xsltPage.SaveValidationBeforeMerge);

            foreach (DataTable table in data.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    row.Delete();
                }
            }
            core.DataService.StoreData(xsltPage.DataStructureId, data, false, null);
            return;
        }
    }
}
