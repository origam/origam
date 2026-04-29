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
using System.Data;
using System.IO;
using System.Reflection;
using log4net;
using Origam.DA;
using Origam.Rule;
using Origam.Rule.Xslt;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Workflow;

/// <summary>
/// Summary description for TransformationAgent.
/// </summary>
public class TransformationAgent : AbstractServiceAgent
{
    private static readonly ILog log = LogManager.GetLogger(
        type: MethodBase.GetCurrentMethod().DeclaringType
    );
    IXsltEngine _transformer = null;

    public TransformationAgent()
    {
        this.PersistenceProviderChanged += new EventHandler(
            TransformationAgent_PersistenceProviderChanged
        );
    }

    #region IServiceAgent Members
    private object _result;
    public override object Result
    {
        get { return _result; }
    }

    public override void Run()
    {
        switch (this.MethodName)
        {
            case "Transform":
            {
                bool validateOnly = false;
                if (
                    this.Parameters.Contains(key: "ValidateOnly")
                    && (bool)this.Parameters[key: "ValidateOnly"] == true
                )
                {
                    validateOnly = true;
                }
                // Check input parameters
                if (!(this.Parameters[key: "Data"] is IXmlContainer))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNotXmlDocument")
                    );
                }

                if (!(this.Parameters[key: "XslScript"] is Guid))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorXslScriptNotGuid")
                    );
                }

                if (
                    !(
                        this.Parameters[key: "Parameters"] == null
                        || this.Parameters[key: "Parameters"] is Hashtable
                    )
                )
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNotHashtable")
                    );
                }

                InitializeTransformer();
                _result = _transformer.Transform(
                    data: this.Parameters[key: "Data"] as IXmlContainer,
                    transformationId: (Guid)this.Parameters[key: "XslScript"],
                    parameters: this.Parameters[key: "Parameters"] as Hashtable,
                    transactionId: (RuleEngine as RuleEngine).TransactionId,
                    outputStructure: this.OutputStructure as AbstractDataStructure,
                    validateOnly: validateOnly
                );
                break;
            }

            case "TransformText":
            {
                TransformText();
                break;
            }

            case "TransformData":
            {
                TransformData();
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "MethodName",
                    actualValue: this.MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }

    private void TransformText()
    {
        ValidateTransformTextParameters();
        InitializeTransformer();
        bool validateOnly = false;
        if (
            Parameters.Contains(key: "ValidateOnly")
            && ((bool)Parameters[key: "ValidateOnly"] == true)
        )
        {
            validateOnly = true;
        }
        _result = _transformer.Transform(
            data: Parameters[key: "Data"] as IXmlContainer,
            xsl: (string)Parameters[key: "XslScript"],
            parameters: Parameters[key: "Parameters"] as Hashtable,
            transactionId: (RuleEngine as RuleEngine).TransactionId,
            outputStructure: OutputStructure as AbstractDataStructure,
            validateOnly: validateOnly
        );
    }

    private void TransformData()
    {
        IDataReader dataReader = null;
        Stream output = null;
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Validating parameters...");
        }
        ValidateTransformDataParameters();
        try
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    format: "Opening output file {0}...",
                    arg0: Parameters.ContainsKey(key: "OutputFile")
                        ? Parameters[key: "OutputFile"]
                        : ""
                );
            }
            output = File.Open(
                path: Parameters[key: "OutputFile"] as string,
                mode: FileMode.Create,
                access: FileAccess.Write,
                share: FileShare.None
            );
            if (log.IsDebugEnabled)
            {
                log.Debug(message: "Aquiring DataReader...");
            }
            dataReader = GetDataReader();
            DataReaderXPathNavigator navigator = new DataReaderXPathNavigator(
                dataReader: dataReader,
                entityName: Parameters[key: "EntityName"] as string,
                position: -1,
                wasMoveToFirstChildInvoked: false
            );
            if (log.IsDebugEnabled)
            {
                log.Debug(message: "Initializing transformation...");
            }
            InitializeTransformer();
            _transformer.Transform(
                input: navigator,
                transformationId: (Guid)Parameters[key: "XslScript"],
                parameters: Parameters[key: "XslParameters"] as Hashtable,
                transactionId: (RuleEngine as RuleEngine).TransactionId,
                output: output
            );
            if (log.IsDebugEnabled)
            {
                log.Debug(message: "Transformation finished...");
            }
        }
        finally
        {
            if (output != null)
            {
                output.Close();
            }
            if (dataReader != null)
            {
                dataReader.Close();
            }
        }
        _result = null;
    }

    private IDataReader GetDataReader()
    {
        DataStructureReference dataStructureReference;
        dataStructureReference = Parameters[key: "DataStructure"] as DataStructureReference;
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: dataStructureReference.DataStructureId,
            methodId: dataStructureReference.DataStructureMethodId,
            defaultSetId: Guid.Empty,
            sortSetId: dataStructureReference.DataStructureSortSetId
        );
        Hashtable parameters = Parameters[key: "DataParameters"] as Hashtable;
        if (parameters != null)
        {
            foreach (DictionaryEntry entry in parameters)
            {
                query.Parameters.Add(
                    value: new QueryParameter(
                        _parameterName: entry.Key as string,
                        value: entry.Value
                    )
                );
            }
        }
        IDataService dataService = DataServiceFactory.GetDataService();
        return dataService.ExecuteDataReader(
            dataStructureQuery: query,
            userProfile: SecurityManager.CurrentPrincipal,
            transactionId: TransactionId
        );
    }

    private void ValidateTransformTextParameters()
    {
        if (!(Parameters[key: "Data"] is IXmlContainer))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorNotXmlDocument")
            );
        }
        if (!(Parameters[key: "XslScript"] is string))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorXslScriptNotGuid")
            );
        }

        if (
            !(
                (Parameters[key: "Parameters"] == null)
                || (Parameters[key: "Parameters"] is Hashtable)
            )
        )
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorNotHashtable")
            );
        }
    }

    private void ValidateTransformDataParameters()
    {
        DataStructureReference dataStructureReference;
        dataStructureReference = Parameters[key: "DataStructure"] as DataStructureReference;
        if (dataStructureReference == null)
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorNotDataStructureReference")
            );
        }
        if (
            !(
                (Parameters[key: "DataParameters"] == null)
                || (Parameters[key: "DataParameters"] is Hashtable)
            )
        )
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorDataParametersNotHashtable")
            );
        }
        if (!(Parameters[key: "EntityName"] is string))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorEntityNameNotString")
            );
        }
        if (!(Parameters[key: "XslScript"] is Guid))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorXslScriptNotGuid")
            );
        }
        if (
            !(
                (Parameters[key: "XslParameters"] == null)
                || (Parameters[key: "XslParameters"] is Hashtable)
            )
        )
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorXslParametersNotHashtable")
            );
        }
        if (!(Parameters[key: "OutputFile"] is string))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorOutputFileNotString")
            );
        }
    }

    public override IList<string> ExpectedParameterNames(
        ISchemaItem item,
        string method,
        string parameter
    )
    {
        IList<string> result = new List<string>();
        XslTransformation transformation = null;
        ServiceMethodCallTask task = item as ServiceMethodCallTask;
        if (task != null)
        {
            transformation = ResolveServiceMethodCallTask(task: task);
        }
        if (transformation != null && method == "Transform" && parameter == "Parameters")
        {
            string transformationText = transformation.TextStore;
            result = XmlTools.ResolveTransformationParameters(
                transformationText: transformationText
            );
        }
        return result;
    }

    private XslTransformation ResolveServiceMethodCallTask(ServiceMethodCallTask task)
    {
        ISchemaItem tParam = task.GetChildByName(name: "XslScript");
        if (tParam.ChildItems.Count == 1)
        {
            TransformationReference tfRef = tParam.ChildItems[index: 0] as TransformationReference;
            if (tfRef != null)
            {
                return tfRef.Transformation as XslTransformation;
            }
        }
        return null;
    }
    #endregion
    private void InitializeTransformer()
    {
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        _transformer = new CompiledXsltEngine(persistence: persistence.SchemaProvider);
        _transformer.Trace = this.Trace;
        _transformer.TraceStepName = this.TraceStepName;
        _transformer.TraceWorkflowId = this.TraceWorkflowId;
        _transformer.TraceStepId = this.TraceStepId;
    }

    private void TransformationAgent_PersistenceProviderChanged(object sender, EventArgs e)
    {
        if (_transformer != null)
        {
            _transformer.PersistenceProvider = this.PersistenceProvider;
        }
    }
}
