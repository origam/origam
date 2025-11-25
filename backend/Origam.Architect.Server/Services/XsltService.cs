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
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using Origam.DA;
using Origam.DA.Service;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class XsltService(
    EditorService editorService,
    IBusinessServicesService businessServicesService
)
{
    private List<ParameterData> parameterList = new();
    private ParameterData selectedParameter = null;

    public Result Validate(Guid schemaItemId)
    {
        EditorData editorData = editorService.OpenDefaultEditor(schemaItemId);
        ISchemaItem item = editorData.Item;
        if (item is not XslTransformation transformation)
        {
            throw new Exception("Not a XslTransformation");
        }

        return ValidateXslt(transformation);
    }

    public Result Transform(Guid schemaItemId)
    {
        EditorData editorData = editorService.OpenDefaultEditor(schemaItemId);
        ISchemaItem item = editorData.Item;
        if (item is not XslTransformation transformation)
        {
            throw new Exception("Not a XslTransformation");
        }

        return ValidateXslt(transformation);
    }

    private Result ValidateXslt(XslTransformation transformation)
    {
        var result = new Result();
        if (
            LoadXslt(transformation.TextStore, result) == null
            || Transform(transformation.TextStore, "<ROOT/>", true, result) == null
        )
        {
            result.Text = Strings.XsltValidationFailed;
            return result;
        }

        result.AddToOutput(Strings.XsltValidationSuccess);
        result.Text = Strings.XsltValidationSuccess;
        return result;
    }

    private XmlDocument LoadXslt(string xslt, Result result)
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

    private Result Transform(string xslt, string sourceXml, bool validateOnly, Result result)
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
            var doc = new XmlContainer(sourceXml);
            transformer.MethodName = "TransformText";
            transformer.Parameters.Add("XslScript", xslt);
            transformer.Parameters.Add("Data", doc);
            transformer.Parameters.Add("ValidateOnly", validateOnly);
            transformer.TransactionId = transactionId;
            //
            // transformer.OutputStructure = cboDataStructure.SelectedItem as IDataStructure;

            // resolve transformation input parameters and try to put an empty xml document to each just
            // in case it expects a node set as a parameter
            var xsltParams = XmlTools.ResolveTransformationParameters(xslt);
            RefreshParameterList();
            LoadDisplayedParameterData();
            Hashtable parameterValues = GetParameterValues(xsltParams);
            transformer.Parameters.Add("Parameters", parameterValues);
            transformer.Run();
            IXmlContainer container = transformer.Result as IXmlContainer;
            if (container == null)
            {
                return result;
            }
            // rule handling
            DataStructureRuleSet ruleSet = null; //cboRuleSet.SelectedItem as DataStructureRuleSet;
            IDataDocument dataDoc = container as IDataDocument;
            if (dataDoc != null)
            {
                if (dataDoc.DataSet.HasErrors == false && ruleSet != null)
                {
                    RuleEngine re = RuleEngine.Create(null, null);
                    re.ProcessRules(dataDoc, ruleSet, null);
                }
            }
            result.SetXml(container);
            return result;
        }
        catch (Exception ex)
        {
            ErrorMessage(ex, result);
            return null;
        }
        finally
        {
            ResourceMonitor.Rollback(transactionId);
        }
    }

    private void RefreshParameterList()
    {
        // XmlDocument xsltDoc = LoadXslt();
        // parameterListUpdater.Refresh(xsltDoc);
    }

    private void LoadDisplayedParameterData()
    {
        UpdateParameterData(selectedParameter);
    }

    private Hashtable GetParameterValues(IList<string> paramNames)
    {
        var parHashtable = new Hashtable();
        foreach (var paramName in paramNames)
        {
            ParameterData correspondingData =
                parameterList.FirstOrDefault(parData => parData.Name == paramName)
                ?? throw new ArgumentException(string.Format(Strings.ParameterNotFound, paramName));

            parHashtable.Add(paramName, correspondingData.Value);
        }
        return parHashtable;
    }

    private void UpdateParameterData(ParameterData parData)
    {
        if (parData == null)
        {
            return;
        }

        // parData.Type = (OrigamDataType)parameterTypeComboBox.SelectedItem;
        // parData.Text = paremeterEditor.Text;
    }

    private void ErrorMessage(Exception ex, Result result)
    {
        result.AddToOutput(ex.Message);
        result.AddToOutput(Environment.NewLine);
        if (ex.InnerException != null)
        {
            ErrorMessage(ex.InnerException, result);
        }
    }
}

internal class ParameterData
{
    public string Name { get; }
    public string Text { get; set; } = "";
    public OrigamDataType Type { get; set; } = OrigamDataType.String;

    public ParameterData(string name, string type)
    {
        this.Name = name;
        Type = StringTypeToParameterDataType(type);
    }

    private OrigamDataType StringTypeToParameterDataType(string type)
    {
        if (type == null)
        {
            return OrigamDataType.String;
        }

        return Enum.GetValues(typeof(OrigamDataType))
                .Cast<OrigamDataType?>()
                .FirstOrDefault(origamType => origamType.ToString() == type)
            ?? throw new ArgumentException(string.Format(Strings.WrongParameterType, type));
    }

    public object Value
    {
        get
        {
            if (Type == OrigamDataType.Xml)
            {
                return new XmlContainer(Text);
            }
            Type systemType = DatasetGenerator.ConvertDataType(Type);

            return DatasetTools.ConvertValue(Text, systemType);
        }
    }

    public override string ToString() => Name;
}

public class Result
{
    [JsonIgnore]
    private readonly StringBuilder output = new();
    public string Title { get; init; }
    public string Text { get; set; }
    public string Output => output.ToString();
    public string ResultXml { get; set; }

    public Result()
    {
        Title = Strings.ValidationResultTitle;
        Text = String.Empty;
    }

    public void SetXml(IXmlContainer container)
    {
        ResultXml = container.ToString();
    }

    public void AddToOutput(string text)
    {
        output.AppendLine(text);
    }
}
