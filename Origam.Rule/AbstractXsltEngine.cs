#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Xml;
using System.Collections;
using System.Data;
using Origam.Schema;
using Origam.Schema.RuleModel;
using Origam.DA.Service;
using Origam.Schema.EntityModel;
using Origam.DA.ObjectPersistence;

using Origam.Workbench.Services;
using System.IO;
using System.Xml.XPath;

namespace Origam.Rule
{
	/// <summary>
	/// Summary description for AsXslTransform.
	/// </summary>
	public abstract class AbstractXsltEngine : IXsltEngine
	{
		private ITracingService _tracingService = ServiceManager.Services.GetService(typeof(ITracingService)) as ITracingService;

#if ORIGAM_CLIENT
		private static Hashtable _transformationCache = new Hashtable();
		private readonly object _lock = new object();
#endif

		#region Constructors
        public AbstractXsltEngine()
		{
		}

        public AbstractXsltEngine(IPersistenceProvider persistence)
		{
			_persistence = persistence;
		}
		#endregion

		#region Properties
        public ITracingService TracingService
        {
            get
            {
                return _tracingService;
            }
        }

		IPersistenceProvider _persistence;
		public IPersistenceProvider PersistenceProvider
		{
			get
			{
				return _persistence;
			}
			set
			{
				_persistence = value;
			}
		}

		string _traceStepName;
		public string TraceStepName
		{
			get
			{
				return _traceStepName;
			}
			set
			{
				_traceStepName = value;
			}
		}
		
		Guid _traceStepId;
		public Guid TraceStepId
		{
			get
			{
				return _traceStepId;
			}
			set
			{
				_traceStepId = value;
			}
		}

		Guid _traceWorkflowId;
		public Guid TraceWorkflowId
		{
			get
			{
				return _traceWorkflowId;
			}
			set
			{
				_traceWorkflowId = value;
			}
		}

		bool _trace;
		public bool Trace
		{
			get
			{
				return _trace;
			}
			set
			{
				_trace = value;
			}
		}
        #endregion

        #region Public Methods
        public IXmlContainer Transform(IXmlContainer data, Guid transformationId, Hashtable parameters, RuleEngine ruleEngine, IDataStructure outputStructure, bool validateOnly)
		{
			return this.Transform(data, transformationId, Guid.Empty, parameters, new Hashtable(), ruleEngine, outputStructure, validateOnly);
		}

        public IXmlContainer Transform(IXmlContainer data, Guid transformationId, Guid retransformationId, Hashtable parameters, Hashtable retransformationParameters, RuleEngine ruleEngine, IDataStructure outputStructure, bool validateOnly)
		{
			object xsltEngine;

#if ORIGAM_CLIENT
			if(_transformationCache.Contains(transformationId))
			{
				xsltEngine = _transformationCache[transformationId];
			}
			else
			{
#endif
				AbstractSchemaItem transf = this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(transformationId)) as AbstractSchemaItem;

				string xsl;

				if(transf is XslTransformation)
				{
					xsl = (transf as XslTransformation).TextStore;
				}
				else if (transf is XslRule)
				{
					xsl = (transf as XslRule).Xsl;
				}
				else
				{
					throw new ArgumentOutOfRangeException("transformationId", transformationId, "Invalid transformation id.");
				}

				xsltEngine = GetTransform(xsl, retransformationId, retransformationParameters, ruleEngine);
#if ORIGAM_CLIENT
				lock(_lock)
				{
					_transformationCache[transformationId] = xsltEngine;
				}
			}
#endif

            return Transform(data, xsltEngine, parameters, ruleEngine, outputStructure, validateOnly);
		}
        public void Transform(IXPathNavigable input, Guid transformationId, Hashtable parameters, RuleEngine ruleEngine, Stream output)
        {
			object xsltEngine;
#if ORIGAM_CLIENT
			if(_transformationCache.Contains(transformationId))
			{
				xsltEngine = _transformationCache[transformationId];
			}
			else
			{
#endif
				AbstractSchemaItem transf = PersistenceProvider.RetrieveInstance(
                    typeof(AbstractSchemaItem), 
                    new ModelElementKey(transformationId)) 
                    as AbstractSchemaItem;
				string xsl;
				if(transf is XslTransformation)
				{
					xsl = (transf as XslTransformation).TextStore;
				}
				else if (transf is XslRule)
				{
					xsl = (transf as XslRule).Xsl;
				}
				else
				{
					throw new ArgumentOutOfRangeException("transformationId", 
                        transformationId, "Invalid transformation id.");
				}

				xsltEngine = GetTransform(xsl, Guid.Empty, new Hashtable(), ruleEngine);
#if ORIGAM_CLIENT
				lock(_lock)
				{
					_transformationCache[transformationId] = xsltEngine;
				}
			}
#endif
            Transform(input, xsltEngine, parameters, ruleEngine, output);
        }

        public object GetTransform(string xsl, Guid retransformTemplateId, Hashtable retransformationParameters, RuleEngine ruleEngine)
        {
            if (retransformTemplateId == Guid.Empty)
            {
                return GetTransform(xsl);
            }
            else
            {
                IXmlContainer oldXslt = new XmlContainer(xsl);
                IXmlContainer newXslt = Transform(oldXslt, retransformTemplateId, retransformationParameters, ruleEngine, null, false);
                return GetTransform(newXslt);
            }

        }

        internal abstract object GetTransform(IXmlContainer xslt);
        internal abstract object GetTransform(string xsl);

        public IXmlContainer Transform(IXmlContainer data, string xsl, Hashtable parameters, RuleEngine ruleEngine, IDataStructure outputStructure, bool validateOnly)
		{
			object xsltEngine = GetTransform(xsl, Guid.Empty, null, ruleEngine);

            return Transform(data, xsltEngine, parameters, ruleEngine, outputStructure, validateOnly);
		}

		internal abstract IXmlContainer Transform(IXmlContainer data, object xsltEngine, Hashtable parameters, RuleEngine ruleEngine, IDataStructure outputStructure, bool validateOnly);
        internal abstract void Transform(IXPathNavigable input, object xstlEngine, Hashtable parameters, RuleEngine ruleEngine, Stream output);

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
			DatasetGenerator gen = new DatasetGenerator(true);
			DataStructure ds = this.PersistenceProvider.RetrieveInstance(typeof(DataStructure), dataStructureKey) as DataStructure;

			return gen.CreateDataSet(ds);
		}
        #endregion
    }
}
