#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using System.Xml;
using System.Xml.XPath;
using Origam.Extensions;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;
using Origam.Service.Core;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Xslt;

public class XsltService(
    EditorService editorService,
    IBusinessServicesService businessServicesService,
    IPersistenceService persistenceService
)
{
    private static readonly List<string> ParameterTypes = GetParameterTypes();

    public ValidationResult Validate(TransformationInput input)
    {
        string xsl = GetXsl(input.SchemaItemId);
        return ValidateXslt(xsl, input);
    }

    public TransformationResult Transform(TransformationInput input)
    {
        string xsl = GetXsl(input.SchemaItemId);
        var result = new TransformationResult();
        return Transform(input: input, xslt: xsl, validateOnly: true, result: result);
    }

    public ParametersResult GetParameters(Guid schemaItemId)
    {
        string xsl = GetXsl(schemaItemId);
        return GetParameterList(xsl);
    }

    public IEnumerable<ShemaItemInfo> GetSettings()
    {
        ISchemaService schema = ServiceManager.Services.GetService<ISchemaService>();
        DataStructureSchemaItemProvider structures =
            schema.GetProvider<DataStructureSchemaItemProvider>();

        return structures
            .ChildItems.Select(item => new ShemaItemInfo
            {
                Name = item.Name,
                SchemaItemId = item.Id,
            })
            .OrderBy(x => x.Name);
    }

    private string GetXsl(Guid schemaItemId)
    {
        EditorData editorData = editorService.OpenDefaultEditor(schemaItemId);
        ISchemaItem item = editorData.Item;
        if (item is XslTransformation transformation)
        {
            return transformation.TextStore;
        }
        if (item is XslRule xslRule)
        {
            return xslRule.Xsl;
        }
        throw new Exception("Not a XslTransformation or XslRule");
    }

    private ValidationResult ValidateXslt(string xsl, TransformationInput input)
    {
        var result = new ValidationResult();
        if (
            LoadXslt(xsl, result) == null
            || Transform(input: input, xslt: xsl, validateOnly: true, result: result) == null
        )
        {
            result.Text = Strings.XsltValidationFailed;
            return result;
        }

        result.AddToOutput(Strings.XsltValidationSuccess);
        result.Text = Strings.XsltValidationSuccess;
        return result;
    }

    private XmlDocument LoadXslt(string xslt, IResult result)
    {
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xslt);
            return doc;
        }
        catch (Exception ex)
        {
            result.AddToOutput(ex.Message);
            return null;
        }
    }

    private TransformationResult Transform(
        TransformationInput input,
        string xslt,
        bool validateOnly,
        TransformationResult result
    )
    {
        result.AddToOutput("");
        string transactionId = Guid.NewGuid().ToString();
        try
        {
            IServiceAgent transformer = businessServicesService.GetAgent(
                "DataTransformationService",
                RuleEngine.Create(new Hashtable(), null),
                null
            );

            var doc = new XmlContainer(input.InputXml);
            transformer.MethodName = "TransformText";
            transformer.Parameters.Add("XslScript", xslt);
            transformer.Parameters.Add("Data", doc);
            transformer.Parameters.Add("ValidateOnly", validateOnly);
            transformer.TransactionId = transactionId;
            transformer.OutputStructure = input.TargetDataStructureId.IsDefault()
                ? null
                : persistenceService.SchemaListProvider.RetrieveInstance<DataStructure>(
                    input.TargetDataStructureId
                );

            // resolve transformation input parameters and try to put an empty xml document to each just
            // in case it expects a node set as a parameter
            Hashtable parameterValues = new Hashtable();
            foreach (var parameter in input.Parameters)
            {
                parameterValues.Add(parameter.Name, parameter.Value);
            }
            transformer.Parameters.Add("Parameters", parameterValues);
            transformer.Run();
            IXmlContainer container = transformer.Result as IXmlContainer;
            if (container == null)
            {
                return result;
            }
            // rule handling
            DataStructureRuleSet ruleSet = input.RuleSetId.IsDefault()
                ? null
                : persistenceService.SchemaListProvider.RetrieveInstance<DataStructureRuleSet>(
                    input.RuleSetId
                );
            if (container is IDataDocument dataDoc)
            {
                if (dataDoc.DataSet.HasErrors == false && ruleSet != null)
                {
                    RuleEngine re = RuleEngine.Create(null, null);
                    re.ProcessRules(dataDoc, ruleSet, null);
                }
            }

            result.Xml = GetFormattedXml(container.Xml);
            return result;
        }
        catch (Exception ex)
        {
            ErrorMessage(ex, result);
            return result;
        }
        finally
        {
            ResourceMonitor.Rollback(transactionId);
        }
    }

    private string GetFormattedXml(XmlNode node)
    {
        if (node == null)
        {
            return "";
        }

        string resultText = "";
        StringWriter swriter = new StringWriter();
        XmlTextWriter xwriter = new XmlTextWriter(swriter);
        try
        {
            xwriter.Formatting = Formatting.Indented;
            node.WriteTo(xwriter);
            resultText = swriter.ToString();
        }
        finally
        {
            xwriter.Close();
            swriter.Close();
        }
        return resultText;
    }

    private ParametersResult GetParameterList(string xsl)
    {
        var result = new ParametersResult(ParameterTypes);
        XmlDocument xsltDoc = LoadXslt(xsl, result);
        if (xsltDoc == null)
        {
            return result;
        }

        IDictionary<string, string> aliasToNameSpace = GetNamespaceDictionary(xsltDoc);
        IList<XmlElement> newParamNodes = XmlTools.ResolveTransformationParameterElements(xsltDoc);
        result.Parameters = newParamNodes
            .Select(x => NodeToParamData(x, aliasToNameSpace))
            .ToList();

        return result;
    }

    private IDictionary<string, string> GetNamespaceDictionary(XmlDocument xsltDoc)
    {
        XPathDocument x = new XPathDocument(new StringReader(xsltDoc.InnerXml));
        XPathNavigator navigator = x.CreateNavigator();
        navigator.MoveToFollowing(XPathNodeType.Element);
        return navigator.GetNamespacesInScope(XmlNamespaceScope.All).Invert();
    }

    private ParameterData NodeToParamData(
        XmlNode parameterNode,
        IDictionary<string, string> aliasToNameSpace
    )
    {
        string name = parameterNode.Attributes["name"].Value;
        try
        {
            string asPrefix = aliasToNameSpace[XmlTools.AsNameSpace];
            string typeAttribute = $"{asPrefix}:DataType";
            string type = parameterNode.Attributes[typeAttribute]?.Value;
            return new ParameterData(name, type);
        }
        catch (KeyNotFoundException)
        {
            return new ParameterData(name, null);
        }
    }

    private void ErrorMessage(Exception ex, IResult result)
    {
        result.AddToOutput(ex.Message);
        result.AddToOutput(Environment.NewLine);
        if (ex.InnerException != null)
        {
            ErrorMessage(ex.InnerException, result);
        }
    }

    private static List<string> GetParameterTypes()
    {
        return Enum.GetValues(typeof(OrigamDataType))
            .Cast<object>()
            .Select(x => x.ToString())
            .ToList();
    }

    public IEnumerable<ShemaItemInfo> GetRuleSets(Guid dataStructureId)
    {
        DataStructure structure =
            persistenceService.SchemaListProvider.RetrieveInstance<DataStructure>(dataStructureId);
        return structure
            .ChildItemsByType<DataStructureRuleSet>(DataStructureRuleSet.CategoryConst)
            .Select(rule => new ShemaItemInfo { Name = rule.Name, SchemaItemId = rule.Id })
            .OrderBy(x => x.Name);
    }
}