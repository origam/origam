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
using System.Reflection;
using Origam.OrigamEngine;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using System.IO;
using System.Text;
using Origam.Extensions;

namespace Origam.Workbench.Services
{
	/// <summary>
	/// Summary description for PersistenceService.
	/// </summary>
	public class PersistenceService : IPersistenceService
	{
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);
        
        private const string SchemaLoadDataStructureId = "84dc90cb-c18b-467b-af23-ab50a5505343";
		public const string SchemaDataStructureId = "0f9daa67-4fe0-4ffb-9d8a-70204dc1ed78";
		private const string SchemaSelectionDataStructureId = "fc7c663e-e782-4e37-a24b-54e3d7d04f45";
		private const string SchemaFilterId_Client = "094166f1-de1e-4ace-9e75-1c62c27ddff7";
		private const string SchemaFilterId = "2e6feb49-7c0a-44c4-ba40-33f4c372f928";
		private const string SchemaSortId = "5fcd96dd-c700-4955-9876-eb8eaa96ce54";

		DatabasePersistenceProvider _helperProvider;
        DatabasePersistenceProvider _schemaProvider;
        DatabasePersistenceProvider _schemaListProvider;

		private bool _isInitialized = false;
		private bool _isConnected = false;
        private bool _autoConnect = true;

		public PersistenceService() : base()
		{
            _helperProvider = new DatabasePersistenceProvider();
            _schemaProvider = new DatabasePersistenceProvider();
            _schemaListProvider = new DatabasePersistenceProvider();
        }

        public PersistenceService(DatabasePersistenceProvider helperProvider,
            DatabasePersistenceProvider schemaProvider,
            DatabasePersistenceProvider schemaListProvider,
            bool autoConnect)
            : base()
        {
            _helperProvider = helperProvider;
            _schemaProvider = schemaProvider;
            _schemaListProvider = schemaListProvider;
            _autoConnect = autoConnect;
        }
        
        #region IService Members

		public void InitializeService()
		{
			/// ******************************************************************** ///
			// We initialize new data service - which is MsSqlDataService
			/// ******************************************************************** ///

			// Now we initialize our IDataService, with which we will access Architect data
			
			// We read the configuration for a connection string
			if(! _isInitialized)
			{
				if(_autoConnect && ! _isConnected) Connect();

				// CHECK DATABASE SCHEMA VERSION
//				if(! IsRepositoryVersionCompatible())
//				{
//					throw new Exception("Model database version different than expected. Unable to read models from this database. Please upgrade the model repository.");
//				}

				OnInitialize(EventArgs.Empty);

				_isInitialized = true;
			}
		}

		public void UpdateRepository()
		{
			if(! _isConnected) Connect();

			UpdateRepository(_schemaProvider.DataService.DatabaseSchemaVersion());
		}

		private Version SystemVersion() => VersionProvider.CurrentModelMeta;

		public bool IsRepositoryVersionCompatible()
		{
			if(! _isConnected) Connect();

			string[] version = VersionProvider.CurrentModelMetaVersion.Split(".".ToCharArray());
			string[] dbVersion = _schemaProvider.DataService.DatabaseSchemaVersion().Split(".".ToCharArray());
		
			return !((version.Length < 2 | dbVersion.Length < 2) || (version[0] != dbVersion[0] | version[1] != dbVersion[1]));
		}

		public bool CanUpdateRepository()
		{
			if(! _isConnected) Connect();

			string dbVersion = _schemaProvider.DataService.DatabaseSchemaVersion();

			Version v = new Version(dbVersion);
			Version basicVersion = SystemVersion();

			return basicVersion.CompareTo(v) >= 0;
		}

		public Package LoadSchema(
			Guid schemaExtensionId, bool loadDocumentation, bool loadDeploymentScripts, 
			string transactionId)
		{
			return LoadSchema(schemaExtensionId, Guid.Empty, loadDocumentation, 
				loadDeploymentScripts, transactionId);
		}

		public Package LoadSchema(Guid schemaExtensionId, Guid extraExtensionId, 
			bool loadDocumentation, bool loadDeploymentScripts, string transactionId)
		{
			IStatusBarService statusBar = ServiceManager.Services.GetService<IStatusBarService>();
			Package extension1;
			Package extension2 = null;
			try
			{
				extension1 = SchemaListProvider.RetrieveInstance(
					typeof(Package), new ModelElementKey(schemaExtensionId)) as Package;
			}
			catch
			{
				throw new Exception(ResourceUtils.GetString("ErrorPackageLoadFailed"));
			}
			if(extraExtensionId != Guid.Empty)
			{
				try
				{
					extension2 = SchemaListProvider.RetrieveInstance(
						typeof(Package), new ModelElementKey(extraExtensionId)) as Package;
				}
				catch
				{
					// do not do anything for extra extension - if it does not exist, we just don't load it
				}
			}
			IList<Package> packages = extension1.IncludedPackages;
			packages.Add(extension1);
			if(extension2 != null) 
			{
				IList<Package> packages2 = extension2.IncludedPackages;
                foreach (Package ext2 in packages2)
                {
                    if(!packages.Contains(ext2))
                    {
                        packages.Add(ext2);
                    }
                }
				packages.Add(extension2);
			}
			if(statusBar != null) statusBar.SetStatusText(ResourceUtils.GetString("Loading", extension1.Name));
			ArrayList extensionIds = new ArrayList(packages.Count);
			for(int i = 0; i < packages.Count; i++)
			{
				Package loadingExtension = packages[i] as Package;
				extensionIds.Add(loadingExtension.PrimaryKey["Id"]);
				bool append = i > 24;
				if(i % 24 == 0 & i > 0)
				{
					LoadSchema(extensionIds, append, loadDocumentation, 
						loadDeploymentScripts, transactionId);
					extensionIds.Clear();
				}
			}
			if(extensionIds.Count > 0)
			{
				LoadSchema(extensionIds, packages.Count > 25, 
					loadDocumentation, loadDeploymentScripts, transactionId);
			}
			_schemaProvider.EnforceConstraints();
			if(statusBar != null) statusBar.SetStatusText("");
			return extension1;
		}

		public void LoadSchema(ArrayList extensions, bool append, 
			bool loadDocumentation, bool loadDeploymentScripts, 
			string transactionId)
		{
			LoadSchema(_schemaProvider, extensions, append, loadDocumentation, 
				loadDeploymentScripts, transactionId);
		}

        public void MergeSchema(DataSet schema, Key activePackage)
        {
            #region Check Compatibility
            DatabasePersistenceProvider cloned = this.SchemaProvider.Clone() as DatabasePersistenceProvider;
            cloned.Init(schema);

            List<Package> list = cloned.RetrieveList<Package>( null);
            List<Package> list2 = this.SchemaProvider.RetrieveList<Package>(null);

            // only import the model if it contains the main extension and it is the main extension there as well
            bool foundActiveExtension = false;
            foreach (Package imported in list)
            {
                if (imported.PrimaryKey.Equals(activePackage)) // && imported.ChildExtensions.Count == 0)
                {
                    foundActiveExtension = true;
                    break;
                }
            }

            if (!foundActiveExtension)
            {
                throw new Exception(ResourceUtils.GetString("ErrorDifferentPackage"));
            }

#if ORIGAM_CLIENT
			// if the imported packages are the same, they must be the same versions or higher
			foreach(Package current in list2)
			{
				foreach(Package imported in list)
				{
					if(imported.PrimaryKey.Equals(current.PrimaryKey))
					{
						Version n = null; 
						Version o = null;
						
						if(imported.Version != null) n = new Version(imported.Version);
						if(current.Version != null) o = new Version(current.Version);

						if(! ((current.Version == null & imported.Version == null) | current.Version == null & imported.Version != null))
						{
							if((current.Version != null & imported.Version == null)
								||
								o.CompareTo(n) > 0)
							{
								throw new InvalidOperationException(ResourceUtils.GetString("ErrorOlderModel"));
							}
						}
					}
				}
			}
#endif
            cloned.FlushCache();
            #endregion

            _schemaProvider.MergeData(schema);
            _schemaProvider.FlushCache();
        }

        public void InitializeRepository()
        {
            log.Info("Initializing repository...");
            string transactionId = Guid.NewGuid().ToString();
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader reader = new StreamReader(
                       assembly.GetManifestResourceStream(
                        assembly.FullName.Split(",".ToCharArray())[0] + ".create_model_db.sql")))
            {
                try
                {
                    string line;
                    StringBuilder command = new StringBuilder();
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line != "GO")
                        {
                            command.Append(line);
                            command.Append("\n");
                        }
                        else
                        {
                            _schemaProvider.DataService
                                .ExecuteUpdate(command.ToString(), transactionId);
                            command = new StringBuilder();
                        }
                    }
                    string lastCommand = command.ToString();
                    if (lastCommand.Length > 0)
                    {
                        _schemaProvider.DataService
                            .ExecuteUpdate(lastCommand, transactionId);
                    }
                    ResourceMonitor.Commit(transactionId);
                    log.Info("Initialized repository...");
                }
                catch (Exception ex)
                {
                    log.LogOrigamError("Failed to initialize repository...", ex);
                    ResourceMonitor.Rollback(transactionId);
                }
            }
        }

        public void ExportPackage(Guid extensionId, string fileName)
		{
			ArrayList extensions = new ArrayList();
			extensions.Add(extensionId);
			LoadSchema(_helperProvider, extensions, false, true, true, null);
			_helperProvider.PersistToFile(fileName, true);
			extensions.Clear();
			extensions.Add(Guid.Empty);
			// empty the helper provider
			LoadSchema(_helperProvider, extensions, false, false, false, null);
		}

		public void MergePackage(
			Guid extensionId, DataSet data, string transactionId)
		{
			ArrayList extensions = new ArrayList();
			extensions.Add(extensionId);
			LoadSchema(_helperProvider, extensions, false, true, true, 
				transactionId);
			_helperProvider.MergeData(data);
			_helperProvider.Update(transactionId);
		}

		private void LoadSchema(
			DatabasePersistenceProvider provider, ArrayList extensions, bool append, 
			bool loadDocumentation, bool loadDeploymentScripts, 
			string transactionId)
		{
			if(extensions.Count > 25) throw new ArgumentOutOfRangeException("extensions count", extensions.Count, ResourceUtils.GetString("ErrorTooMuchExtensions"));
 
			// we set the main model's data structure
			provider.DataStructureId = new Guid(SchemaDataStructureId);
			
			Guid filterId;
			Guid sortId = Guid.Empty;
#if ORIGAM_CLIENT
				if(loadDeploymentScripts)
				{
					filterId = new Guid(SchemaFilterId);
				}
				else
				{
					filterId = new Guid(SchemaFilterId_Client);
				}
#else
			filterId = new Guid(SchemaFilterId);
			sortId = new Guid(SchemaSortId);
#endif
			DataStructureQuery q = new DataStructureQuery(
				new Guid(SchemaLoadDataStructureId), filterId, Guid.Empty, sortId);
			q.EnforceConstraints = false;
			for(int i = 1; i <= extensions.Count; i++)
			{
				object schemaExtensionId = extensions[i-1];
				string counter = i.ToString();
				q.Parameters.Add(new QueryParameter("SchemaExtension_parId" + counter, schemaExtensionId));
				q.Parameters.Add(new QueryParameter("SchemaItemGroup_parSchemaExtensionId" + counter, schemaExtensionId));
			}
			q.Parameters.Add(new QueryParameter("Documentation_parLoadDocumentation", loadDocumentation ? 1 : 0));
			q.LoadByIdentity = false;
			provider.DataStructureQuery = q;
			// We load our metadata
			provider.Refresh(append, transactionId);
		}

		public void LoadSchemaList()
		{
			OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
			DataStructureQuery q = new DataStructureQuery(new Guid(SchemaSelectionDataStructureId));
			q.LoadByIdentity = false;
			_schemaListProvider.DataStructureQuery = q;
            _schemaListProvider.DataStructureId = q.DataSourceId;

			// We load our metadata
			try
			{
				_schemaListProvider.Refresh(false, null);
			}						
			catch(Exception ex)
			{
				throw new Exception(ResourceUtils.GetString("ErrorSchemaListFailed", ex.Message), ex);
			}
		}

		public IPersistenceProvider SchemaProvider
		{
			get
			{
				return _schemaProvider;
			}
		}

		public IPersistenceProvider SchemaListProvider
		{
			get
			{
				return _schemaListProvider;
			}
		}

		public void UnloadService()
		{
			_schemaListProvider.DataService.Dispose();

			_schemaListProvider.Dispose();
			_schemaProvider.Dispose();
			_isConnected = false;
			

			OnUnload(EventArgs.Empty);
		}
		
		protected void OnInitialize(EventArgs e)
		{
			if (Initialize != null) 
			{
				Initialize(this, e);
			}
		}
		
		protected void OnUnload(EventArgs e)
		{
			if (Unload != null) 
			{
				Unload(this, e);
			}
		}
		
		public event EventHandler Initialize;
		public event EventHandler Unload;

		#endregion

		#region ICloneable Members

		public object Clone()
		{
			return new PersistenceService();
		}

		#endregion

		#region Private Methods
		private void UpdateRepository(string startVersion)
		{
			IDataService service = _schemaProvider.DataService;
			string transactionId = Guid.NewGuid().ToString();

			try
			{
				switch(startVersion)
				{
					case "1.7":
						UpdateTo_1_8(service, transactionId);
						goto case "1.8";

					case "1.8":
						UpdateTo_1_9(service, transactionId);
						goto case "1.9";

					case "1.9":
						UpdateTo_1_10(service, transactionId);
						goto case "1.10";

					case "1.10":
						UpdateTo_1_11(service, transactionId);
						goto case "1.11";

					case "1.11":
						UpdateTo_1_12(service, transactionId);
						goto case "1.12";

					case "1.12":
						UpdateTo_1_13(service, transactionId);
						goto case "1.13";

					case "1.13":
						UpdateTo_1_14(service, transactionId);
						goto case "1.14";

					case "1.14":
						UpdateTo_1_15(service, transactionId);
						goto case "1.15";

					case "1.15":
						UpdateTo_1_16(service, transactionId);
						goto case "1.16";

					case "1.16":
						UpdateTo_1_17(service, transactionId);
						goto case "1.17";

					case "1.17":
						UpdateTo_1_18(service, transactionId);
						goto case "1.18";

					case "1.18":
						goto case "4.0";
					case "4.0":
						goto case "4.5";
					case "4.5":
						UpdateTo_4_6(service, transactionId);
						goto case "4.6";
					case "4.6":
						UpdateTo_4_7(service, transactionId);
						goto case "4.7";
					case "4.7":
						UpdateTo_4_8(service, transactionId);
                        goto case "4.8";
                    case "4.8":
                        UpdateTo_4_9(service, transactionId);
                        goto case "4.9";
                    case "4.9":
                        UpdateTo_4_10(service, transactionId);
                        goto case "4.10";
                    case "4.10":
                        UpdateTo_4_11(service, transactionId);
                        goto case "4.11.0.0";
                    case "4.11.0.0":
						UpdateTo_5_0(service, transactionId);
						goto case "5.0";
					case "5.0":
                        // todo - next version
						break;
                }

                service.UpdateDatabaseSchemaVersion(VersionProvider.CurrentModelMetaVersion, transactionId);
			}
			catch(Exception ex)
			{
				ResourceMonitor.Rollback(transactionId);
				throw new Exception(ResourceUtils.GetString("ErrorRepositoryUpdateFailed", Environment.NewLine + ex.Message), ex);
			}

			ResourceMonitor.Commit(transactionId);
		}

		private void UpdateTo_1_8(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 42; i++)
			{
				string scriptCode = "1_8_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception("Error occured while running update script '" + scriptCode + "'" + Environment.NewLine + ex.Message, ex);
				}
			}
		}

		private void UpdateTo_1_9(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 6; i++)
			{
				string scriptCode = "1_9_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_1_10(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 5; i++)
			{
				string scriptCode = "1_10_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_1_11(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 3; i++)
			{
				string scriptCode = "1_11_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_1_12(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 1; i++)
			{
				string scriptCode = "1_12_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_1_13(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 1; i++)
			{
				string scriptCode = "1_13_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_1_14(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 4; i++)
			{
				string scriptCode = "1_14_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_1_15(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 2; i++)
			{
				string scriptCode = "1_15_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_1_16(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 4; i++)
			{
				string scriptCode = "1_16_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_1_17(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 1; i++)
			{
				string scriptCode = "1_17_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_1_18(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 1; i++)
			{
				string scriptCode = "1_18_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_4_6(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 1; i++)
			{
				string scriptCode = "4_6_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_4_7(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 2; i++)
			{
				string scriptCode = "4_7_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

		private void UpdateTo_4_8(IDataService service, string transactionId)
		{
			for(int i = 1; i <= 1; i++)
			{
				string scriptCode = "4_8_" + i.ToString("0000");
				try
				{
					service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
				}
				catch(Exception ex)
				{
					throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
				}
			}
		}

        private void UpdateTo_4_9(IDataService service, string transactionId)
        {
            for (int i = 1; i <= 1; i++)
            {
                string scriptCode = "4_9_" + i.ToString("0000");
                try
                {
                    service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
                }
                catch (Exception ex)
                {
                    throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
                }
            }
        }

        private void UpdateTo_4_10(IDataService service, string transactionId)
        {
            for (int i = 1; i <= 19; i++)
            {
                string scriptCode = "4_10_" + i.ToString("0000");
                try
                {
                    service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
                }
                catch (Exception ex)
                {
                    throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
                }
            }
        }
        private void UpdateTo_4_11(IDataService service, string transactionId)
        {
            for (int i = 1; i <= 1; i++)
            {
                string scriptCode = "4_11_" + i.ToString("0000");
                try
                {
                    service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
                }
                catch (Exception ex)
                {
                    throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
                }
            }
        }
		private void UpdateTo_5_0(IDataService service, string transactionId)
		{
			string scriptCode = "5_0"; 
			try
			{
				service.ExecuteUpdate(UpdateScriptHelper.GetScript(scriptCode), transactionId);
			}
			catch (Exception ex)
			{
				throw new Exception(ResourceUtils.GetString("ErrorWhenUpdateScript", scriptCode, Environment.NewLine + ex.Message), ex);
			}
		}

        private void Connect()
		{
			OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;

			if(settings == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorSettingsNotFound"));

			string assembly = settings.SchemaDataService.Split(",".ToCharArray())[0].Trim();
			string classname = settings.SchemaDataService.Split(",".ToCharArray())[1].Trim();

			IDataService architectService = Reflector.InvokeObject(assembly, classname) as IDataService;
			architectService.ConnectionString = settings.SchemaConnectionString;
            architectService.BulkInsertThreshold = settings.ModelBulkInsertThreshold;
            architectService.UpdateBatchSize = settings.ModelUpdateBatchSize;

			if(architectService is AbstractDataService)
			{
				// AbstractDataService needs its own persistence provider, we use OrigamPersistenceProvider
				DatabasePersistenceProvider metadataPersistence = new DatabasePersistenceProvider();

				// The persistence provider needs its own data service - we use the simplest possible one
				metadataPersistence.DataService = new OrigamMetadataDataService();

				// There is no really query, we read the whole xml file as it is
				metadataPersistence.DataStructureQuery = new DataStructureQuery();
			
				// We load up our metadata persistence provider
				metadataPersistence.Refresh(false, null);

				(architectService as AbstractDataService).PersistenceProvider = metadataPersistence;
			}

			_schemaProvider.DataService = architectService;
			_schemaListProvider.DataService = architectService;
			_helperProvider.DataService = architectService;

			_isConnected = true;
		}
		#endregion

	    public void Dispose()
	    {
	        _helperProvider?.Dispose();
	        _schemaProvider?.Dispose();
	        _schemaListProvider?.Dispose();
	    }
	}
}
