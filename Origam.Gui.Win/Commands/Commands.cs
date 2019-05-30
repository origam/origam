using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Gui.Win.Commands
{

    public class ShowDataStructureFilterSetSql : AbstractMenuCommand
    {
        private WorkbenchSchemaService _schema =
            ServiceManager.Services.GetService(
                typeof(SchemaService)) as WorkbenchSchemaService;

        public override bool IsEnabled
        {
            get
            {
                return Owner is DataStructureFilterSet;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        public override void Run()
        {
            AbstractSqlDataService abstractSqlDataService = DataService.GetDataService() as AbstractSqlDataService;
            AbstractSqlCommandGenerator generator = (AbstractSqlCommandGenerator)abstractSqlDataService.DbDataAdapterFactory.Clone();
            DataStructureFilterSet filterSet = Owner as DataStructureFilterSet;
            generator.PrettyFormat = true;
            generator.generateConsoleUseSyntax = true;
            StringBuilder builder = new StringBuilder();
            DataStructure ds = filterSet.RootItem as DataStructure;
            builder.AppendFormat("-- SQL statements for data structure: {0}\r\n", ds.Name);
            // parameter declarations
            builder.AppendLine(
                generator.SelectParameterDeclarationsSql(
                filterSet, false, null));

            foreach (DataStructureEntity entity in ds.Entities)
            {
                if (entity.Columns.Count > 0)
                {
                    builder.AppendLine(generator.CreateOutputTableSql());
                    builder.AppendLine("-----------------------------------------------------------------");
                    builder.AppendLine("-- " + entity.Name);
                    builder.AppendLine("-----------------------------------------------------------------");
                    builder.AppendLine(
                        generator.SelectSql(ds,
                        entity,
                        filterSet,
                        null,
                        null,
                        new Hashtable(),
                        null,
                        false
                        )
                        );
                    builder.AppendLine(generator.CreateDataStructureFooterSql());
                }
            }
            new ShowSqlConsole(builder.ToString()).Run();
        }
    }

    public class ShowDataStructureSortSetSql : AbstractMenuCommand
    {
        private WorkbenchSchemaService _schema =
            ServiceManager.Services.GetService(
                typeof(SchemaService)) as WorkbenchSchemaService;

        public override bool IsEnabled
        {
            get
            {
                return Owner is DataStructureSortSet;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        public override void Run()
        {
            StringBuilder builder = new StringBuilder();
            AbstractSqlDataService abstractSqlDataService = DataService.GetDataService() as AbstractSqlDataService;
            AbstractSqlCommandGenerator generator = (AbstractSqlCommandGenerator)abstractSqlDataService.DbDataAdapterFactory.Clone();
            generator.PrettyFormat = true;
            generator.generateConsoleUseSyntax = true;
            bool displayPagingParameters = true;
            DataStructure ds = (Owner as ISchemaItem).RootItem as DataStructure;
            builder.AppendLine("-- SQL statements for data structure: " + ds.Name);
            foreach (DataStructureEntity entity in ds.Entities)
            {
                if (entity.Columns.Count > 0)
                {
                    builder.AppendLine("-----------------------------------------------------------------");
                    builder.AppendLine("-- " + entity.Name);
                    builder.AppendLine("-----------------------------------------------------------------");
                    // parameter declarations
                    builder.AppendLine(
                        generator.SelectParameterDeclarationsSql(
                        ds, entity, Owner as DataStructureSortSet,
                        displayPagingParameters, null));
                    builder.AppendLine(generator.CreateOutputTableSql());
                    builder.AppendLine(
                        generator.SelectSql(ds,
                        entity,
                        null,
                        Owner as DataStructureSortSet,
                        null,
                        new Hashtable(),
                        new Hashtable(),
                        displayPagingParameters
                        )
                        );
                    builder.AppendLine(generator.CreateDataStructureFooterSql());
                }
            }
            new ShowSqlConsole(builder.ToString()).Run();
        }
    }

    public class ShowSqlConsole : AbstractMenuCommand
    {
        WorkbenchSchemaService _schemaService =
            ServiceManager.Services.GetService(typeof(WorkbenchSchemaService))
                as WorkbenchSchemaService;

        public ShowSqlConsole(object owner)
        {
            Owner = owner;
        }

        public override bool IsEnabled
        {
            get { return _schemaService.IsSchemaLoaded; }
            set
            {
                throw new ArgumentException("Cannot set this property",
                    "IsEnabled");
            }
        }

        public override void Run()
        {
            SqlViewer viewer = new SqlViewer();
            viewer.LoadObject(Owner as string);
            WorkbenchSingleton.Workbench.ShowView(viewer);
        }
    }
    
     public class ShowDataStructureEntitySql : AbstractMenuCommand
    {
        private WorkbenchSchemaService _schema =
            ServiceManager.Services.GetService(
                typeof(SchemaService)) as WorkbenchSchemaService;

        public override bool IsEnabled
        {
            get
            {
                return Owner is DataStructureEntity;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        public override void Run()
        {
            AbstractSqlDataService abstractSqlDataService = DataService.GetDataService() as AbstractSqlDataService;
            AbstractSqlCommandGenerator generator = (AbstractSqlCommandGenerator) abstractSqlDataService.DbDataAdapterFactory.Clone();
            DataStructureEntity entity = Owner as DataStructureEntity;
            StringBuilder builder = new StringBuilder();
            if (entity.Columns.Count > 0)
            {
                DataStructure ds = (Owner as ISchemaItem).RootItem as DataStructure;
                builder.AppendLine("-- SQL statements for data structure: " + ds.Name);
                generator.PrettyFormat = true;
                generator.generateConsoleUseSyntax = true;
                // parameter declarations
                builder.AppendLine(
                    generator.SelectParameterDeclarationsSql(
                    ds,
                    Owner as DataStructureEntity,
                    (DataStructureFilterSet)null, false, null)
                    );
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine("-- " + (Owner as DataStructureEntity).Name);
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine(
                    generator.SelectSql(
                        ds: ds,
                        entity: Owner as DataStructureEntity,
                        filter: null,
                        sortSet: null,
                        scalarColumn: null,
                        parameters: new Hashtable(),
                        selectParameterReferences: null,
                        forceDatabaseCalculation: false
                        )
                    );
                builder.AppendLine();
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine("-- Load Record After Update SQL");
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine(
                    generator.SelectRowSql(
                    Owner as DataStructureEntity,
                    new Hashtable(),
                    null,
                    true
                    )
                    );
                builder.AppendLine();
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine("-- INSERT SQL");
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine(
                    generator.InsertSql(
                    ds,
                    Owner as DataStructureEntity
                    )
                    );
                builder.AppendLine();
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine("-- UPDATE SQL");
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine(
                    generator.UpdateSql(
                    ds,
                    Owner as DataStructureEntity
                    )
                    );
                builder.AppendLine();
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine("-- DELETE SQL");
                builder.AppendLine("-----------------------------------------------------------------");
                builder.AppendLine(
                    generator.DeleteSql(
                    ds,
                    Owner as DataStructureEntity
                    )
                    );
            }
            else
            {
                builder.AppendLine("No SQL command generated for this entity. No columns selected.");
            }
            new ShowSqlConsole(builder.ToString()).Run();
        }
    }
     
     public class GenerateDataStructureEntityColumns : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				return Owner is DataStructureEntity;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
			DataStructureEntity entity = Owner as DataStructureEntity;
			entity.AllFields = false;

			foreach(DataStructureColumn col in entity.Columns)
			{
				col.IsDeleted = true;
				col.Persist();
			}

			foreach(IDataEntityColumn column in entity.EntityDefinition.EntityColumns)
			{
				DataStructureColumn newColumn = entity.NewItem(typeof(DataStructureColumn), 
                    _schema.ActiveSchemaExtensionId, null) as DataStructureColumn;
				newColumn.Field = column;
				newColumn.Name = column.Name;
				newColumn.Persist();
			}
			entity.Persist();
		}

		public override void Dispose()
		{
			_schema = null;
		}

	}


	public class GenerateWorkQueueClassEntityMappings : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return Owner is WorkQueueClass;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
			Origam.Schema.WorkflowModel.WorkflowHelper.GenerateWorkQueueClassEntityMappings(
                Owner as WorkQueueClass);
		}
		}

	public class SaveDataFromDataStructure : AbstractMenuCommand
	{
		WorkbenchSchemaService _schemaService = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
		IServiceAgent _dataServiceAgent;

		public override bool IsEnabled
		{
			get
			{
				return Owner is DataStructure 
					||  Owner is DataStructureMethod;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
			DataStructure structure;
			DataStructureMethod method = null;

			if(Owner is DataStructureMethod)
			{
				structure = (Owner as DataStructureMethod).ParentItem as DataStructure;
				method = Owner as DataStructureMethod;
			}
			else
			{
				structure = Owner as DataStructure;
			}
			

			_dataServiceAgent = (ServiceManager.Services.GetService(
                typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);

			DataStructureQuery query = new DataStructureQuery(structure.Id);
			if(method != null)	
			{
				query.MethodId = method.Id;
			}

			SaveFileDialog dialog = null;

			try
			{
				dialog = new SaveFileDialog();

				dialog.AddExtension = true;
				dialog.DefaultExt = "xml";
				dialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*" ;
				dialog.FilterIndex = 1;

				dialog.Title = strings.SaveXmlResult_Title;

				if(dialog.ShowDialog() == DialogResult.OK)
				{
					LoadData(query).WriteXml(dialog.FileName, XmlWriteMode.WriteSchema);
				} 
			}
			finally
			{
				if(dialog != null) dialog.Dispose();
			}
		}

		private DataSet LoadData(DataStructureQuery query)
		{
			_dataServiceAgent.MethodName = "LoadDataByQuery";
			_dataServiceAgent.Parameters.Clear();
			_dataServiceAgent.Parameters.Add("Query", query);

			_dataServiceAgent.Run();

			return _dataServiceAgent.Result as DataSet;
		}

		public override void Dispose()
		{
			_schemaService = null;
			_dataServiceAgent = null;
		}

	}
	
	/// <summary>
	/// Makes the selected version the current version of the package
	/// </summary>
	public class MakeActiveVersionCurrent : AbstractMenuCommand
	{
		WorkbenchSchemaService _schemaService = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				return Owner is DeploymentVersion 
				       && (Owner as DeploymentVersion).IsCurrentVersion == false 
				       & (Owner as DeploymentVersion).SchemaExtension.PrimaryKey.Equals(
					       _schemaService.ActiveExtension.PrimaryKey);
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");
			}
		}

		public override void Run()
		{
			MakeVersionCurrent cmd = new MakeVersionCurrent();
			cmd.Owner = Owner as DeploymentVersion;
			cmd.Run();
		}

		public override void Dispose()
		{
			_schemaService = null;
		}
	}
	public class MakeVersionCurrent : AbstractCommand
	{
		public override void Run()
		{
			WorkbenchSchemaService _schemaService = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
			IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;

			if (_schemaService.IsSchemaChanged &&
			    _persistence is PersistenceService)
			{
				throw new Exception(
					"Model not saved. Please, save the model before setting the version.");
			}

			DeploymentVersion version = this.Owner as DeploymentVersion;

			SchemaExtension ext = _persistence.SchemaProvider.RetrieveInstance(typeof(SchemaExtension), _schemaService.ActiveExtension.PrimaryKey) as SchemaExtension;

			ext.VersionString = version.VersionString;
			ext.Persist();
			_schemaService.ActiveExtension.Refresh();

			IDatabasePersistenceProvider dbProvider = _persistence.SchemaProvider as IDatabasePersistenceProvider;
			IDatabasePersistenceProvider dbListProvider = _persistence.SchemaListProvider as IDatabasePersistenceProvider;
			if(dbProvider != null)
			{
				dbProvider.Update(null);
			}
			if(dbListProvider != null)
			{
				dbListProvider.Refresh(false, null);
			}

			_schemaService.SchemaBrowser.EbrSchemaBrowser.RefreshItem(version.RootProvider);
			_schemaService.SchemaBrowser.EbrSchemaBrowser.SelectItem(version);
			Origam.Workbench.Commands.DeployVersion cmd3 = new Origam.Workbench.Commands.DeployVersion();
			cmd3.Run();
		}
	}
}