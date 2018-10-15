#define THREADING

using System;
using System.Data;
using System.Collections;
using System.Threading;

using Origam.DA;
using Origam.Schema.EntityModel;

namespace Origam.Gui.Win
{
	public class AsDataSetConsumerLoader : IDisposable
	{
		DataLoader _dataLoader = new DataLoader();
#if THREADING
		Thread _dataLoaderThread = null;
#endif

		#region Constructors
		public AsDataSetConsumerLoader()
		{
			_dataLoader.Parent = this;
		}

		public AsDataSetConsumerLoader(IDataService dataService, Guid schemaVersion) : this()
		{
			if(dataService !=null)
			{
				this.DataService = dataService;
			}
			else
			{
				throw new NullReferenceException("NULL reference to DataService exception");
			}

			_schemaVersion = schemaVersion;
			_query.DataStructureSchemaVersionId =schemaVersion;
		}
		#endregion

		#region Properties
		//Schemaversion for queries
		private bool _finishedAddingContexts = false;
		public bool FinishedAddingContexts
		{
			get
			{
				return _finishedAddingContexts;
			}
			set
			{
				_finishedAddingContexts = value;

#if THREADING
				if(value)
				{
					_dataLoaderThread.Join();
					if(_dataLoader.Exception != null)
					{
						throw _dataLoader.Exception;
					}

					//_dataLoader.Parent = null;
					//_dataLoader = null;
				}
#endif
			}
		}

		private Guid _schemaVersion;
		public Guid SchemaVersion
		{
			get{return _schemaVersion;}
			set{
				_schemaVersion = value;
				_query.DataStructureSchemaVersionId = value;
			}
		}

		//DataCache
		private Hashtable _contextCache = new Hashtable();
		public Hashtable ContextCache
		{
			get{return _contextCache;}

			set{_contextCache = value;}
		}

		//Reference to DataService
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

		private DataStructureQuery _query = new DataStructureQuery();
		public DataStructureQuery CurrentQuery
		{
			get{return _query;}
		}
		#endregion

		#region Public Functions
		/// <summary>
		/// Returns dataset either from the cache or it loads a new one.
		/// </summary>
		/// <param name="dataStructureId"></param>
		/// <returns></returns>
		public DataSet GetFullDataSet(Guid dataStructureId, Guid filterId)
		{
			string index = GetDataStructureIndex(dataStructureId, filterId);

			DataSet result;
			
			if(_contextCache.Contains(index))
			{
				// if cache contains the data, we return it
				result = ((AsDataViewContext)_contextCache[index]).Data;
			}
			else
			{
				// if not, we load the data
				result = GetDataViewContext(dataStructureId, filterId).Data;
			}

			if(result != null)
			{
				return result;
			}
			else
			{
				throw new ArgumentException("Can't load Data by provided datastructureId", "dataStructureId");
			}

		}

		private string GetDataStructureIndex(Guid dataStructureId, Guid filterId)
		{
			return dataStructureId.ToString() + "|" + filterId.ToString();
		}

		/// <summary>
		/// Refreshes a dataset in the cache.
		/// </summary>
		/// <param name="dataStructureId"></param>
		public void RefreshDataView(Guid dataStructureId, Guid filterId)
		{
			string index = GetDataStructureIndex(dataStructureId, filterId);

			if(_contextCache.Contains(index))
			{
				RefreshDataSet(dataStructureId, filterId,((AsDataViewContext)_contextCache[index]).Data);
			}
			else
			{
				throw new ArgumentException("ComboBox Refresh was called, but data for refresh was'n found", "dataStructureGuid");
			}
		}
        
		/// <summary>
		/// Gets data view either from cache or loads it from the data source.
		/// </summary>
		/// <param name="dataStructure"></param>
		/// <param name="dataTable"></param>
		/// <returns></returns>
		public DataView GetDataView(Guid dataStructure, Guid filterId, string dataTable)
		{
			return this.GetDataView(dataStructure, filterId, dataTable,0);
		}

		/// <summary>
		/// Gets data view, shifted by context number.
		/// </summary>
		/// <param name="dataStructureId"></param>
		/// <param name="dataTable"></param>
		/// <param name="contextValue"></param>
		/// <returns></returns>
		public DataView GetDataView(Guid dataStructureId, Guid filterId, string dataTable, int contextValue)
		{
			string index = GetDataStructureIndex(dataStructureId, filterId);

			if(_contextCache.Contains(index))
			{
				return ((AsDataViewContext)_contextCache[index]).GetDataView(dataTable, contextValue);
			}
			else
			{
				
				if(this._schemaVersion ==Guid.Empty || this.DataService == null)
				{
					throw new NullReferenceException("Null dataservice or schema version reference");
				}
				else
				{
					return GetDataViewContext(dataStructureId, filterId).GetDataView(dataTable, contextValue);
				}
			}
		}

		/// <summary>
		/// Adds provided AsDataViewContext to the internal cache.
		/// </summary>
		/// <param name="dataStructureId"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public bool AddContextToCache(Guid dataStructureId, Guid filterId, AsDataViewContext val)
		{
			string index = GetDataStructureIndex(dataStructureId, filterId);

			if(!_contextCache.Contains(index))
			{
				_contextCache.Add(index, val);
				return true;
			}
			else
			{
				return false;
			}
		}

		public void StartAddingContexts()
		{
			this.FinishedAddingContexts = false;

#if THREADING
			_dataLoaderThread = new Thread(new ThreadStart(_dataLoader.FillContexts));
			_dataLoaderThread.IsBackground = true;
			_dataLoaderThread.Start();
#endif
		}
		#endregion

		#region Private Functions
		/// <summary>
		/// Refreshes the provided dataset.
		/// </summary>
		/// <param name="dataStructure"></param>
		/// <param name="data"></param>
		private void RefreshDataSet(Guid dataStructureId, Guid filterId, DataSet data)
		{
			try
			{
				_query.DataStructureGuid = dataStructureId;
				_query.StoredFilterGuid = filterId;

				foreach(DataTable table in data.Tables)
				{
					table.BeginLoadData();
				}

				foreach(DataTable table in data.Tables)
				{
					table.Clear();
				}

				foreach(DataTable table in data.Tables)
				{
					table.EndLoadData();
				}

				_dataService.LoadDataSet(_query, null, data);
				
			}
			catch(Exception ex)
			{
				throw new System.Data.DataException("Can't read datastructure (" 
					+ dataStructureId.ToString() 
					+ ") from data service." 
					+ Environment.NewLine 
					+ "Error:" + 
					Environment.NewLine + 
					ex.Message
					, ex);
			}
		}

		/// <summary>
		/// Loads data from the data source and returns the AsDataViewContext. 
		/// It adds the context to the internal cache.
		/// </summary>
		/// <param name="dataStructureId"></param>
		/// <returns></returns>
		private AsDataViewContext GetDataViewContext(Guid dataStructureId, Guid filterId)
		{
			AsDataViewContext newItem = new AsDataViewContext();
			DataSet newData = LoadData(dataStructureId, filterId);
			
			if(newData == null || newData.Tables.Count <1)
			{
				throw new ArgumentException("Can't load Data by provided datastructureId", "dataStructureId");
			}

			newItem.Data = newData;

			// we add new data view context to cache
			AddContextToCache(dataStructureId, filterId, newItem);

			return newItem;
		}

		/// <summary>
		/// Adds empty dataset to the cache so it will be refreshed by the data loader thread.
		/// </summary>
		/// <param name="dataStructureId"></param>
		/// <returns></returns>
		private DataSet LoadData(Guid dataStructureId, Guid filterId)
		{
			AsDataViewContext newContext = new AsDataViewContext();
			newContext.Data = DataService.GetEmptyDataSet(dataStructureId, this.SchemaVersion);

			if(!AddContextToCache(dataStructureId, filterId, newContext))
			{				
				System.Diagnostics.Debug.WriteLine("Method AddContextToCache can't wride data to cache (" + dataStructureId.ToString() + ")");
			}

#if THREADING
#else
			_dataLoader.FillContexts();
#endif

			return newContext.Data;
		}
		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			this.DataService = null;

			foreach(AsDataViewContext ctxt in this.ContextCache.Values)
			{
				ctxt.Dispose();
			}

			_contextCache.Clear();
			_contextCache =  null;
			_dataLoader = null;
			_query = null;
		}

		#endregion

		public class DataLoader
		{
			public DataLoader()
			{
			}

			AsDataSetConsumerLoader _parent;
			public AsDataSetConsumerLoader Parent
			{
				get{return _parent;}
				set{_parent = value;}
			}

			Exception _exception;
			public Exception Exception
			{
				get{return _exception;}
				set{_exception = value;}
			}

			public void FillContexts()
			{
#if THREADING
				try
				{
					bool finishOnNextIteration = false;
					bool quit = false;

					while(! quit)
					{
#endif
						ArrayList keys = new ArrayList(Parent.ContextCache.Keys);
					
						foreach(string key in keys)
						{
							Guid dataStructureId = new Guid(key.Split("|".ToCharArray())[0]);
							Guid filterId = new Guid(key.Split("|".ToCharArray())[1]);

							AsDataViewContext context = Parent.ContextCache[key] as AsDataViewContext;

							if(!context.IsLoaded)
							{
								RefreshDataSet(dataStructureId, filterId, context.Data);
								context.IsLoaded = true;
							}
						}

#if THREADING
						Thread.Sleep(50);
					
						if(finishOnNextIteration)
						{
							quit = true;
						}

						if(Parent.FinishedAddingContexts)
						{
							finishOnNextIteration = true;
						}
					}
				}
				catch(Exception ex)
				{
					this.Exception = ex;
				}
#endif
			}

			/// <summary>
			/// Refreshes the provided dataset.
			/// </summary>
			/// <param name="dataStructure"></param>
			/// <param name="data"></param>
			private void RefreshDataSet(Guid dataStructureId, Guid filterId, DataSet data)
			{
				try
				{
					if(Parent.DataService != null)
					{
						DataStructureQuery query = new DataStructureQuery(dataStructureId, Parent.SchemaVersion, filterId);

						Parent.DataService.LoadDataSet(query, null, data);
					}
				}
				catch(Exception ex)
				{
					throw new System.Data.DataException("Can't read datastructure (" 
						+ dataStructureId.ToString() 
						+ ") from data service." 
						+ Environment.NewLine 
						+ "Error:" + 
						Environment.NewLine + 
						ex.Message
						, ex);
				}
			}
		}
	}
	
}
