using System;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;
using Origam.DA;
using Origam.DA.Service;
using Microsoft.Practices.EnterpriseLibrary.Configuration;

namespace Origam.Workbench.Services
{
	/// <summary>
	/// Summary description for PersistenceService.
	/// </summary>
	public class PersistenceService : IPersistenceService //OrigamPersistenceProvider, IPersistenceService
	{
		public PersistenceService() : base()
		{
		}

		#region IService Members

		public void InitializeService()
		{
			/// ******************************************************************** ///
			// We initialize new data service - which is MsSqlDataService
			/// ******************************************************************** ///

			// Now we initialize our IDataService, with which we will access Architect data
			
			// We read the configuration for a connection string
			OrigamSettings settings = ConfigurationManager.GetConfiguration("OrigamSettings") as OrigamSettings;

			if(settings == null)
			{
				// store new default config
				settings = new OrigamSettings();
				settings.SchemaConnectionString = "--PLACE SCHEMA CONNECTION STRING HERE--";
				settings.DataConnectionString = "--PLACE DATA CONNECTION STRING HERE--";
				settings.SchemaDataService = "--PLACE ASSEMBLY REFERENCE HERE--";
				settings.DataDataService =  "--PLACE ASSEMBLY REFERENCE HERE--";
				ConfigurationManager.WriteConfiguration("OrigamSettings", settings);

				throw new System.Configuration.ConfigurationException("Default ORIGAM configuration created. Check the configuration file.");
			}

			string assembly = settings.SchemaDataService.Split(",".ToCharArray())[0].Trim();
			string classname = settings.SchemaDataService.Split(",".ToCharArray())[1].Trim();

			IDataService architectService = Reflector.InvokeObject(assembly, classname) as IDataService; //new Origam.DA.Service.MsSqlDataService(settings.SchemaConnectionString);
			architectService.ConnectionString = settings.SchemaConnectionString;

			if(architectService is AbstractDataService)
			{
				// AbstractDataService needs its own persistence provider, we use OrigamPersistenceProvider
				IPersistenceProvider metadataPersistence = new OrigamPersistenceProvider();

				// The persistence provider needs its own data service - we use the simplest possible one
				metadataPersistence.DataService = new SerializerDataService();
				metadataPersistence.DataService.ConnectionString = "origamMetadata.xml";

				// There is no really query, we read the whole xml file as it is
				metadataPersistence.DataStructureQuery = new DataStructureQuery();
			
				// We load up our metadata persistence provider
				metadataPersistence.Refresh();

				(architectService as AbstractDataService).PersistenceProvider = metadataPersistence;
			}

			this.SchemaProvider.DataService = architectService;

			OnInitialize(EventArgs.Empty);
		}

		public void LoadSchema(Guid schemaVersion, Guid schemaExtension)
		{
			OrigamSettings settings = ConfigurationManager.GetConfiguration("OrigamSettings") as OrigamSettings;
			DataStructureQuery q = new DataStructureQuery(settings.SchemaDataStructureId, settings.SchemaDataStructureSchemaVersionId);
			this.SchemaProvider.DataStructureQuery = q;
			
			// We load our metadata
			this.SchemaProvider.Refresh();
		}

		IPersistenceProvider _schemaProvider = new OrigamPersistenceProvider();
		public IPersistenceProvider SchemaProvider
		{
			get
			{
				return _schemaProvider;
			}
		}

		IPersistenceProvider _schemaListProvider = new OrigamPersistenceProvider();
		public IPersistenceProvider SchemaListProvider
		{
			get
			{
				return _schemaListProvider;
			}
		}

		public void UnloadService()
		{
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
	}
}
