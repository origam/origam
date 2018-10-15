using System;
using System.Data;
using System.Collections;

using Origam.DA;
using Origam.Workbench.Services;
using Origam.UI;
using Origam.Schema;
using Origam.Schema.LookupModel;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for LookupManager.
	/// </summary>
	public class LookupManager
	{
		private enum QueryType
		{
			List,
			Value
		}

		private Hashtable _controls = new Hashtable();
		private IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		private Hashtable _valueCache = new Hashtable();

		public LookupManager()
		{
		}

		#region Properties
		private IDataService _dataService;
		public IDataService DataService
		{
			get
			{
				return _dataService;
			}
			set
			{
				_dataService = value;
			}
		}
		#endregion
		
		#region Public Methods
		/// <summary>
		/// Adds control under lookup management
		/// </summary>
		/// <param name="lookupControl"></param>
		public void AddLookupControl(ILookupControl lookupControl)
		{
			_controls.Add(lookupControl, null);

			AbstractDataLookup lookup = GetLookup(lookupControl.LookupId, lookupControl.LookupSchemaVersionId);

			// set the EditMenu visibility, if user can or cannot run the Edit command
			lookupControl.LookupShowEditButton = (lookup.ListEditMenuItem != null);

			lookupControl.LookupListDisplayMember = lookup.ListDisplayMember;
			lookupControl.LookupListValueMember = lookup.ListValueMember;

			lookupControl.LookupDisplayTextRequested += new EventHandler(lookupControl_LookupDisplayTextRequested);
			lookupControl.LookupEditRequested += new EventHandler(lookupControl_LookupEditRequested);
			lookupControl.LookupListRefreshRequested += new EventHandler(lookupControl_LookupListRefreshRequested);
		}

		public string GetDisplayText(Guid lookupId, Guid lookupSchemaVersionId, object lookupValue)
		{
			if(lookupValue == DBNull.Value | lookupValue == null)
			{
				return "";
			}
			else
			{
				if(_valueCache.ContainsKey(lookupId))
				{
					Hashtable cache = _valueCache[lookupId] as Hashtable;
					if(cache.ContainsKey(lookupValue))
					{
						return (string)cache[lookupValue];
					}
				}
				else
				{
					_valueCache.Add(lookupId, new Hashtable());
				}

				DataServiceDataLookup lookup = GetLookup(lookupId, lookupSchemaVersionId) as DataServiceDataLookup;

				if(lookup.ValueDisplayMember.IndexOf(".") > -1) return "wrong display member";

				DataStructureQuery query = GetQuery(lookup, QueryType.Value);

				foreach(string parameterName in lookup.ValueFilterSet.ParameterReferences.Keys)
				{
					query.Parameters.Add(new QueryParameter(parameterName, lookupValue));
				}

				string val;

				try
				{
					object result = _dataService.GetScalarValue(query, lookup.ValueDisplayMember);
					if(result == null)
					{
						val = null;
					}
					else
					{
						val = result.ToString();
					}
				}
				catch(Exception ex)
				{
					val = ex.Message;
				}

				if(val == null) 
				{
					val = lookupValue.ToString();
				}

				(_valueCache[lookupId] as Hashtable).Add(lookupValue, val);

				return val;
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Returns lookup schema item
		/// </summary>
		/// <param name="lookupControl"></param>
		/// <returns></returns>
		private AbstractDataLookup GetLookup(Guid lookupId, Guid lookupSchemaVersionId)
		{
			SchemaVersionKey key = new SchemaVersionKey(lookupId, lookupSchemaVersionId);
			return _persistence.SchemaProvider.RetrieveInstance(typeof(AbstractDataLookup), key) as AbstractDataLookup;
		}

		private DataStructureQuery GetQuery(AbstractDataLookup lookup, QueryType queryType)
		{
			if(lookup is DataServiceDataLookup)
			{
				switch(queryType)
				{
					case QueryType.List:
						return new DataStructureQuery(lookup.ListDataStructureId, lookup.ListDataStructureSchemaVersionId, (lookup as DataServiceDataLookup).ListDataStructureFilterSetId);
					case QueryType.Value:
						return new DataStructureQuery(lookup.ValueDataStructureId, lookup.ValueDataStructureSchemaVersionId, (lookup as DataServiceDataLookup).ValueDataStructureFilterSetId);
				}
			}

			throw new ArgumentOutOfRangeException("Unknown lookup type");
		}
		#endregion

		#region Lookup Control Event Handlers
		private void lookupControl_LookupDisplayTextRequested(object sender, EventArgs e)
		{
			ILookupControl control = sender as ILookupControl;

			control.LookupDisplayText = GetDisplayText(control.LookupId, control.LookupSchemaVersionId, control.LookupValue);
		}

		private void lookupControl_LookupEditRequested(object sender, EventArgs e)
		{
			// execute the menu item
		}

		private void lookupControl_LookupListRefreshRequested(object sender, EventArgs e)
		{
			ILookupControl control = sender as ILookupControl;

			DataServiceDataLookup lookup = GetLookup(control.LookupId, control.LookupSchemaVersionId) as DataServiceDataLookup;

			DataStructureQuery query = GetQuery(lookup, QueryType.List);
			
			// set parameters
			foreach(DictionaryEntry entry in control.LookupListParameters)
			{
				query.Parameters.Add(new QueryParameter((string)entry.Key, entry.Value));
			}

			DataSet data = _dataService.LoadDataSet(query, null);

			DataView view = new DataView(data.Tables[0]);

			control.LookupList = view;

			_controls[control] = view;
		}
		#endregion
	}
}
