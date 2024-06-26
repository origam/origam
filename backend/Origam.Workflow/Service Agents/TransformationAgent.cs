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
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;
using Origam.DA;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services.CoreServices;
using log4net;
using Origam.Workbench.Services;
using System.Collections.Generic;
using Origam.Rule.Xslt;
using Origam.Service.Core;

namespace Origam.Workflow;
/// <summary>
/// Summary description for TransformationAgent.
/// </summary>
public class TransformationAgent : AbstractServiceAgent
{
	private static readonly ILog log = LogManager.GetLogger(
        MethodBase.GetCurrentMethod().DeclaringType);
    IXsltEngine _transformer = null;
	public TransformationAgent()
	{
		this.PersistenceProviderChanged += new EventHandler(TransformationAgent_PersistenceProviderChanged);
	}
	#region IServiceAgent Members
	private object _result;
	public override object Result
	{
		get
		{
			return _result;
		}
	}
	public override void Run()
	{
		switch(this.MethodName)
		{
			case "Transform":
                bool validateOnly = false;
				if(this.Parameters.Contains("ValidateOnly") && (bool)this.Parameters["ValidateOnly"] == true)
				{
					validateOnly = true;
				}
				// Check input parameters
				if(! (this.Parameters["Data"] is IXmlContainer))
					throw new InvalidCastException(ResourceUtils.GetString("ErrorNotXmlDocument"));
				if(! (this.Parameters["XslScript"] is Guid))
					throw new InvalidCastException(ResourceUtils.GetString("ErrorXslScriptNotGuid"));
				if(! (this.Parameters["Parameters"] == null || this.Parameters["Parameters"] is Hashtable))
					throw new InvalidCastException(ResourceUtils.GetString("ErrorNotHashtable"));
                InitializeTransformer((Guid)Parameters["XslScript"]);
				_result = 
					_transformer.Transform(this.Parameters["Data"] as IXmlContainer, 
					(Guid)this.Parameters["XslScript"],
					this.Parameters["Parameters"] as Hashtable,
					(RuleEngine as RuleEngine).TransactionId,
					this.OutputStructure as AbstractDataStructure,
					validateOnly);
				break;
			case "TransformText":
                TransformText();
				break;
            case "TransformData":
                TransformData();
                break;
			default:
				throw new ArgumentOutOfRangeException(
                    "MethodName", this.MethodName, 
                    ResourceUtils.GetString("InvalidMethodName"));
		}
	}
    private void TransformText()
    {
        ValidateTransformTextParameters();
        if(Parameters.Contains("XsltEngineType") 
        && (Parameters["XsltEngineType"] is int))
        {
            InitializeTransformer(
                (XsltEngineType)Parameters["XsltEngineType"]);
        }
        else
        {
            InitializeTransformer(Guid.Empty);
        }
        bool validateOnly = false;
        if(Parameters.Contains("ValidateOnly") 
        && ((bool)Parameters["ValidateOnly"] == true))
        {
            validateOnly = true;
        }
        _result = _transformer.Transform(
            Parameters["Data"] as IXmlContainer, 
            (string)Parameters["XslScript"],
            Parameters["Parameters"] as Hashtable,
            (RuleEngine as RuleEngine).TransactionId,
            OutputStructure as AbstractDataStructure,
            validateOnly);
    }
    private void TransformData()
    {
        IDataReader dataReader = null;
        Stream output = null;
        if(log.IsDebugEnabled)
        {
            log.Debug("Validating parameters...");
        }
        ValidateTransformDataParameters();
        try
        {
            if(log.IsDebugEnabled)
            {
                log.DebugFormat("Opening output file {0}...", 
                    Parameters.ContainsKey("OutputFile") ? Parameters["OutputFile"] : "");
            }
            output = File.Open(Parameters["OutputFile"] as string,
                FileMode.Create, FileAccess.Write, FileShare.None);
            if(log.IsDebugEnabled)
            {
                log.Debug("Aquiring DataReader...");
            }
            dataReader = GetDataReader();
            DataReaderXPathNavigator navigator 
                = new DataReaderXPathNavigator(
                    dataReader, Parameters["EntityName"] as string, -1, 
                    false);
            if(log.IsDebugEnabled)
            {
                log.Debug("Initializing transformation...");
            }
            InitializeTransformer((Guid)Parameters["XslScript"]);
            _transformer.Transform(navigator, (Guid)Parameters["XslScript"],
                    Parameters["XslParameters"] as Hashtable,
                    (RuleEngine as RuleEngine).TransactionId,
                    output);
            if(log.IsDebugEnabled)
            {
                log.Debug("Transformation finished...");
            }
        }
        finally
        {
            if(output != null)
            {
                output.Close();
            }
            if(dataReader != null)
            {
                dataReader.Close();
            }
        }
        _result = null;
    }
    private IDataReader GetDataReader()
    {
        DataStructureReference dataStructureReference;
        dataStructureReference = Parameters["DataStructure"] 
            as DataStructureReference;
        DataStructureQuery query = new DataStructureQuery(
            dataStructureReference.DataStructureId, 
            dataStructureReference.DataStructureMethodId, 
            Guid.Empty, 
            dataStructureReference.DataStructureSortSetId);
        Hashtable parameters = Parameters["DataParameters"] as Hashtable;
        if(parameters != null)
        {
            foreach(DictionaryEntry entry in parameters)
            {
                query.Parameters.Add(new QueryParameter(
                    entry.Key as string, entry.Value));
            }
        }
        IDataService dataService = DataServiceFactory.GetDataService();
        return dataService.ExecuteDataReader(
            query, SecurityManager.CurrentPrincipal, TransactionId);
    }
    private void ValidateTransformTextParameters()
    {
        if(!(Parameters["Data"] is IXmlContainer))
        {
            throw new InvalidCastException(
                ResourceUtils.GetString("ErrorNotXmlDocument"));
        }
        if(!(Parameters["XslScript"] is string))
            throw new InvalidCastException(
                ResourceUtils.GetString("ErrorXslScriptNotGuid"));
        if(!((Parameters["Parameters"] == null)
        || (Parameters["Parameters"] is Hashtable)))
            throw new InvalidCastException(
                ResourceUtils.GetString("ErrorNotHashtable"));
    }
    private void ValidateTransformDataParameters()
    {
        DataStructureReference dataStructureReference;
        dataStructureReference = Parameters["DataStructure"] 
            as DataStructureReference;
        if(dataStructureReference == null)
        {
            throw new InvalidCastException(ResourceUtils.GetString(
                "ErrorNotDataStructureReference"));
        }
        if(!((Parameters["DataParameters"] == null) 
        || (Parameters["DataParameters"] is Hashtable)))
        {
            throw new InvalidCastException(ResourceUtils.GetString(
                "ErrorDataParametersNotHashtable"));
        }
        if(!(Parameters["EntityName"] is string))
        {
            throw new InvalidCastException(ResourceUtils.GetString(
                "ErrorEntityNameNotString"));
        }
        if(!(Parameters["XslScript"] is Guid))
        {
            throw new InvalidCastException(ResourceUtils.GetString(
                "ErrorXslScriptNotGuid"));
        }
        if(!((Parameters["XslParameters"] == null) 
        || (Parameters["XslParameters"] is Hashtable)))
        {
            throw new InvalidCastException(ResourceUtils.GetString(
                "ErrorXslParametersNotHashtable"));
        }
        if(!(Parameters["OutputFile"] is string))
        {
            throw new InvalidCastException(ResourceUtils.GetString(
                "ErrorOutputFileNotString"));
        }
    }
	public override IList<string> ExpectedParameterNames(AbstractSchemaItem item, string method, string parameter)
	{
		IList<string> result = new List<string>();
		XslTransformation transformation = null;
		ServiceMethodCallTask task = item as ServiceMethodCallTask;
		if(task != null)
		{
			transformation = ResolveServiceMethodCallTask(task);
		}
		if(transformation != null && method == "Transform" && parameter == "Parameters")
		{
			string transformationText = transformation.TextStore;
			result = XmlTools.ResolveTransformationParameters (transformationText);
		}
		return result;
	}
	private XslTransformation ResolveServiceMethodCallTask(ServiceMethodCallTask task)
	{
		AbstractSchemaItem tParam = task.GetChildByName("XslScript");
		if(tParam.ChildItems.Count == 1)
		{
			TransformationReference tfRef = tParam.ChildItems[0] as TransformationReference;
			if(tfRef != null)
			{
				return tfRef.Transformation as XslTransformation;
			}
		}
		return null;
	}
	#endregion
    private void InitializeTransformer(XsltEngineType xsltEngineType)
    {
        IPersistenceService persistence 
            = ServiceManager.Services.GetService(
                typeof(IPersistenceService)) as IPersistenceService;
        _transformer = AsTransform.GetXsltEngine(
            xsltEngineType, persistence.SchemaProvider);
		_transformer.Trace = this.Trace;
		_transformer.TraceStepName = this.TraceStepName;
		_transformer.TraceWorkflowId = this.TraceWorkflowId;
		_transformer.TraceStepId = this.TraceStepId;
    }
    private void InitializeTransformer(Guid transformationId)
    {
        IPersistenceService persistence 
            = ServiceManager.Services.GetService(
                typeof(IPersistenceService)) as IPersistenceService;
        if(transformationId == Guid.Empty)
        {
            _transformer = AsTransform.GetXsltEngine(
                XsltEngineType.XslTransform, persistence.SchemaProvider);
        }
        else
        {
            _transformer = AsTransform.GetXsltEngine(
                persistence.SchemaProvider, transformationId);
        }
		_transformer.Trace = this.Trace;
		_transformer.TraceStepName = this.TraceStepName;
		_transformer.TraceWorkflowId = this.TraceWorkflowId;
		_transformer.TraceStepId = this.TraceStepId;
    }
	private void TransformationAgent_PersistenceProviderChanged(object sender, EventArgs e)
	{
        if(_transformer != null)
        {
            _transformer.PersistenceProvider = this.PersistenceProvider;
        }
	}
}
