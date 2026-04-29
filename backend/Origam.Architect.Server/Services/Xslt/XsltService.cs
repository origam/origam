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
        string xsl = GetXsl(schemaItemId: input.SchemaItemId);
        return ValidateXslt(xsl: xsl, input: input);
    }

    public TransformationResult Transform(TransformationInput input)
    {
        string xsl = GetXsl(schemaItemId: input.SchemaItemId);
        var result = new TransformationResult();
        return Transform(input: input, xslt: xsl, validateOnly: true, result: result);
    }

    public ParametersResult GetParameters(Guid schemaItemId)
    {
        string xsl = GetXsl(schemaItemId: schemaItemId);
        return GetParameterList(xsl: xsl);
    }

    public IEnumerable<ShemaItemInfo> GetSettings()
    {
        ISchemaService schema = ServiceManager.Services.GetService<ISchemaService>();
        DataStructureSchemaItemProvider structures =
            schema.GetProvider<DataStructureSchemaItemProvider>();

        return structures
            .ChildItems.Select(selector: item => new ShemaItemInfo
            {
                Name = item.Name,
                SchemaItemId = item.Id,
            })
            .OrderBy(keySelector: x => x.Name);
    }

    private string GetXsl(Guid schemaItemId)
    {
        EditorData editorData = editorService.OpenDefaultEditor(schemaItemId: schemaItemId);
        ISchemaItem item = editorData.Item;
        if (item is XslTransformation transformation)
        {
            return transformation.TextStore;
        }
        if (item is XslRule xslRule)
        {
            return xslRule.Xsl;
        }
        throw new Exception(message: "Not a XslTransformation or XslRule");
    }

    private ValidationResult ValidateXslt(string xsl, TransformationInput input)
    {
        var result = new ValidationResult();
        if (
            LoadXslt(xslt: xsl, result: result) == null
            || Transform(input: input, xslt: xsl, validateOnly: true, result: result) == null
        )
        {
            result.Text = Strings.XsltValidationFailed;
            return result;
        }

        result.AddToOutput(text: Strings.XsltValidationSuccess);
        result.Text = Strings.XsltValidationSuccess;
        return result;
    }

    private XmlDocument LoadXslt(string xslt, IResult result)
    {
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml: xslt);
            return doc;
        }
        catch (Exception ex)
        {
            result.AddToOutput(text: ex.Message);
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
        result.AddToOutput(text: "");
        string transactionId = Guid.NewGuid().ToString();
        try
        {
            IServiceAgent transformer = businessServicesService.GetAgent(
                serviceType: "DataTransformationService",
                ruleEngine: RuleEngine.Create(contextStores: new Hashtable(), transactionId: null),
                workflowEngine: null
            );

            var doc = new XmlContainer(xmlString: input.InputXml);
            transformer.MethodName = "TransformText";
            transformer.Parameters.Add(key: "XslScript", value: xslt);
            transformer.Parameters.Add(key: "Data", value: doc);
            transformer.Parameters.Add(key: "ValidateOnly", value: validateOnly);
            transformer.TransactionId = transactionId;
            transformer.OutputStructure = input.TargetDataStructureId.IsDefault()
                ? null
                : persistenceService.SchemaListProvider.RetrieveInstance<DataStructure>(
                    instanceId: input.TargetDataStructureId
                );

            // resolve transformation input parameters and try to put an empty xml document to each just
            // in case it expects a node set as a parameter
            Hashtable parameterValues = new Hashtable();
            foreach (var parameter in input.Parameters)
            {
                parameterValues.Add(key: parameter.Name, value: parameter.Value);
            }
            transformer.Parameters.Add(key: "Parameters", value: parameterValues);
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
                    instanceId: input.RuleSetId
                );
            if (container is IDataDocument dataDoc)
            {
                if (dataDoc.DataSet.HasErrors == false && ruleSet != null)
                {
                    RuleEngine re = RuleEngine.Create(contextStores: null, transactionId: null);
                    re.ProcessRules(data: dataDoc, ruleSet: ruleSet, contextRow: null);
                }
            }

            result.Xml = GetFormattedXml(node: container.Xml);
            return result;
        }
        catch (Exception ex)
        {
            ErrorMessage(ex: ex, result: result);
            return result;
        }
        finally
        {
            ResourceMonitor.Rollback(transactionId: transactionId);
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
        XmlTextWriter xwriter = new XmlTextWriter(w: swriter);
        try
        {
            xwriter.Formatting = Formatting.Indented;
            node.WriteTo(w: xwriter);
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
        var result = new ParametersResult(parameterTypes: ParameterTypes);
        XmlDocument xsltDoc = LoadXslt(xslt: xsl, result: result);
        if (xsltDoc == null)
        {
            return result;
        }

        IDictionary<string, string> aliasToNameSpace = GetNamespaceDictionary(xsltDoc: xsltDoc);
        IList<XmlElement> newParamNodes = XmlTools.ResolveTransformationParameterElements(
            doc: xsltDoc
        );
        result.Parameters = newParamNodes
            .Select(selector: x =>
                NodeToParamData(parameterNode: x, aliasToNameSpace: aliasToNameSpace)
            )
            .ToList();

        return result;
    }

    private IDictionary<string, string> GetNamespaceDictionary(XmlDocument xsltDoc)
    {
        XPathDocument x = new XPathDocument(textReader: new StringReader(s: xsltDoc.InnerXml));
        XPathNavigator navigator = x.CreateNavigator();
        navigator.MoveToFollowing(type: XPathNodeType.Element);
        return navigator.GetNamespacesInScope(scope: XmlNamespaceScope.All).Invert();
    }

    private ParameterData NodeToParamData(
        XmlNode parameterNode,
        IDictionary<string, string> aliasToNameSpace
    )
    {
        string name = parameterNode.Attributes[name: "name"].Value;
        try
        {
            string asPrefix = aliasToNameSpace[key: XmlTools.AsNameSpace];
            string typeAttribute = $"{asPrefix}:DataType";
            string type = parameterNode.Attributes[name: typeAttribute]?.Value;
            return new ParameterData(name: name, type: type);
        }
        catch (KeyNotFoundException)
        {
            return new ParameterData(name: name, type: null);
        }
    }

    private void ErrorMessage(Exception ex, IResult result)
    {
        result.AddToOutput(text: ex.Message);
        result.AddToOutput(text: Environment.NewLine);
        if (ex.InnerException != null)
        {
            ErrorMessage(ex: ex.InnerException, result: result);
        }
    }

    private static List<string> GetParameterTypes()
    {
        return Enum.GetValues(enumType: typeof(OrigamDataType))
            .Cast<object>()
            .Select(selector: x => x.ToString())
            .ToList();
    }

    public IEnumerable<ShemaItemInfo> GetRuleSets(Guid dataStructureId)
    {
        DataStructure structure =
            persistenceService.SchemaListProvider.RetrieveInstance<DataStructure>(
                instanceId: dataStructureId
            );
        return structure
            .ChildItemsByType<DataStructureRuleSet>(itemType: DataStructureRuleSet.CategoryConst)
            .Select(selector: rule => new ShemaItemInfo
            {
                Name = rule.Name,
                SchemaItemId = rule.Id,
            })
            .OrderBy(keySelector: x => x.Name);
    }
}
