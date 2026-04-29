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
using System.Xml.XPath;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using StackExchange.Profiling;

namespace Origam.Rule.Xslt;

/// <summary>
/// Summary description for AsXslTransform.
/// </summary>
public abstract class AbstractXsltEngine : IXsltEngine
{
    private ITracingService _tracingService =
        ServiceManager.Services.GetService(serviceType: typeof(ITracingService)) as ITracingService;
#if ORIGAM_CLIENT
    private readonly object _lock = new object();
#endif
    #region Constructors

    public AbstractXsltEngine() { }

    public AbstractXsltEngine(IPersistenceProvider persistence)
    {
        _persistence = persistence;
    }
    #endregion
    #region Properties
    public ITracingService TracingService
    {
        get { return _tracingService; }
    }
    IPersistenceProvider _persistence;
    public IPersistenceProvider PersistenceProvider
    {
        get { return _persistence; }
        set { _persistence = value; }
    }
    string _traceStepName;
    public string TraceStepName
    {
        get { return _traceStepName; }
        set { _traceStepName = value; }
    }

    Guid _traceStepId;
    public Guid TraceStepId
    {
        get { return _traceStepId; }
        set { _traceStepId = value; }
    }
    Guid _traceWorkflowId;
    public Guid TraceWorkflowId
    {
        get { return _traceWorkflowId; }
        set { _traceWorkflowId = value; }
    }
    bool _trace;
    public bool Trace
    {
        get { return _trace; }
        set { _trace = value; }
    }
    #endregion
    #region Public Methods
    public IXmlContainer Transform(
        IXmlContainer data,
        Guid transformationId,
        Hashtable parameters,
        string transactionId,
        IDataStructure outputStructure,
        bool validateOnly
    )
    {
        return this.Transform(
            data: data,
            transformationId: transformationId,
            retransformationId: Guid.Empty,
            parameters: parameters,
            transactionId: transactionId,
            retransformationParameters: new Hashtable(),
            outputStructure: outputStructure,
            validateOnly: validateOnly
        );
    }

    public IXmlContainer Transform(
        IXmlContainer data,
        Guid transformationId,
        Guid retransformationId,
        Hashtable parameters,
        string transactionId,
        Hashtable retransformationParameters,
        IDataStructure outputStructure,
        bool validateOnly
    )
    {
        object xsltEngine;
#if ORIGAM_CLIENT
        if (IsTransformationCached(transformationId: transformationId))
        {
            xsltEngine = GetCachedTransformation(tranformationId: transformationId);
        }
        else
        {
#endif
            ISchemaItem transf =
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: transformationId)
                ) as ISchemaItem;
            string xsl;
            if (transf is XslTransformation)
            {
                xsl = (transf as XslTransformation).TextStore;
            }
            else if (transf is XslRule)
            {
                xsl = (transf as XslRule).Xsl;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "transformationId",
                    actualValue: transformationId,
                    message: "Invalid transformation id."
                );
            }
            xsltEngine = GetTransform(
                xsl: xsl,
                retransformTemplateId: retransformationId,
                retransformationParameters: retransformationParameters,
                transactionId: transactionId
            );
#if ORIGAM_CLIENT
            lock (_lock)
            {
                PutTransformationToCache(
                    transformationId: transformationId,
                    transformation: xsltEngine
                );
            }
        }
#endif
        using (
            MiniProfiler.Current.CustomTiming(
                category: "xsl",
                commandString: "transformationId: " + transformationId,
                executeType: "XslTransform"
            )
        )
        {
            return Transform(
                data: data,
                xsltEngine: xsltEngine,
                parameters: parameters,
                transactionId: transactionId,
                outputStructure: outputStructure,
                validateOnly: validateOnly
            );
        }
    }

    public void Transform(
        IXPathNavigable input,
        Guid transformationId,
        Hashtable parameters,
        string transactionId,
        Stream output
    )
    {
        object xsltEngine;
#if ORIGAM_CLIENT
        if (IsTransformationCached(transformationId: transformationId))
        {
            xsltEngine = GetCachedTransformation(tranformationId: transformationId);
        }
        else
        {
#endif
            ISchemaItem transf =
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: transformationId)
                ) as ISchemaItem;
            string xsl;
            if (transf is XslTransformation)
            {
                xsl = (transf as XslTransformation).TextStore;
            }
            else if (transf is XslRule)
            {
                xsl = (transf as XslRule).Xsl;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "transformationId",
                    actualValue: transformationId,
                    message: "Invalid transformation id."
                );
            }
            xsltEngine = GetTransform(
                xsl: xsl,
                retransformTemplateId: Guid.Empty,
                retransformationParameters: new Hashtable(),
                transactionId: transactionId
            );
#if ORIGAM_CLIENT
            lock (_lock)
            {
                PutTransformationToCache(
                    transformationId: transformationId,
                    transformation: xsltEngine
                );
            }
        }
#endif
        Transform(
            input: input,
            xstlEngine: xsltEngine,
            parameters: parameters,
            transactionId: transactionId,
            output: output
        );
    }

    public object GetTransform(
        string xsl,
        Guid retransformTemplateId,
        Hashtable retransformationParameters,
        string transactionId
    )
    {
        if (retransformTemplateId == Guid.Empty)
        {
            return GetTransform(xsl: xsl);
        }
        IXmlContainer oldXslt = new XmlContainer(xmlString: xsl);
        IXmlContainer newXslt = Transform(
            data: oldXslt,
            transformationId: retransformTemplateId,
            parameters: retransformationParameters,
            transactionId: transactionId,
            outputStructure: null,
            validateOnly: false
        );

        return GetTransform(xslt: newXslt);
    }

    internal abstract object GetTransform(IXmlContainer xslt);
    internal abstract object GetTransform(string xsl);

    public IXmlContainer Transform(
        IXmlContainer data,
        string xsl,
        Hashtable parameters,
        string transactionId,
        IDataStructure outputStructure,
        bool validateOnly
    )
    {
        object xsltEngine = GetTransform(
            xsl: xsl,
            retransformTemplateId: Guid.Empty,
            retransformationParameters: null,
            transactionId: transactionId
        );
        return Transform(
            data: data,
            xsltEngine: xsltEngine,
            parameters: parameters,
            transactionId: transactionId,
            outputStructure: outputStructure,
            validateOnly: validateOnly
        );
    }

    internal abstract IXmlContainer Transform(
        IXmlContainer data,
        object xsltEngine,
        Hashtable parameters,
        string transactionId,
        IDataStructure outputStructure,
        bool validateOnly
    );
    internal abstract void Transform(
        IXPathNavigable input,
        object xstlEngine,
        Hashtable parameters,
        string transactionId,
        Stream output
    );

    public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo)
    {
        if (traceTaskInfo != null)
        {
            Trace = traceTaskInfo.Trace;
            TraceStepId = traceTaskInfo.TraceStepId;
            TraceWorkflowId = traceTaskInfo.TraceWorkflowId;
            TraceStepName = traceTaskInfo.TraceStepName;
        }
    }
    #endregion
    #region Private Methods
    internal DataSet GetEmptyData(Key dataStructureKey)
    {
        DatasetGenerator gen = new DatasetGenerator(userDefinedParameters: true);
        DataStructure ds =
            this.PersistenceProvider.RetrieveInstance(
                type: typeof(DataStructure),
                primaryKey: dataStructureKey
            ) as DataStructure;
        return gen.CreateDataSet(ds: ds);
    }
    #endregion
    #region Transformation Cache
    protected virtual bool IsTransformationCached(Guid transformationId)
    {
        return false;
    }

    protected virtual object GetCachedTransformation(Guid tranformationId)
    {
        return null;
    }

    protected virtual void PutTransformationToCache(
        Guid transformationId,
        object transformation
    ) { }
    #endregion
}
