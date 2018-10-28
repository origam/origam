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
using System.Collections;
using System.Data;
using System.Windows.Forms;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;
using Origam.DA.ObjectPersistence;
using Origam.DA;

namespace Origam.Workbench.Commands
{
	/// <summary>
	/// Connect to the workbench repository
	/// </summary>
	public class ConnectRepository : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return ! WorkbenchSingleton.Workbench.IsConnected;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			WorkbenchSingleton.Workbench.Connect();
		}		
	}

	/// <summary>
	/// Disconnects from the workbench repository
	/// </summary>
	public class DisconnectRepository : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return WorkbenchSingleton.Workbench.IsConnected;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			WorkbenchSingleton.Workbench.Disconnect();
		}		
	}

	/// <summary>
	/// Shows window for editing OrigamSettings.config
	/// </summary>
	public class EditConfiguration : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			OrigamSettingsEditor editor = new OrigamSettingsEditor();

			editor.LoadObject(ConfigurationManager.GetAllConfigurations());
			WorkbenchSingleton.Workbench.ShowView(editor);
		}		
	}

	public class FlushSchemaCache : AbstractMenuCommand
	{
		IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		SchemaService _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

		public override bool IsEnabled
		{
			get
			{
				return _schema.IsSchemaLoaded;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			_persistence.SchemaProvider.FlushCache();
		}

		public override void Dispose()
		{
			_persistence = null;
			_schema = null;
		}

	}

	/// <summary>
	/// Persists schema to a file, allowing user to select the file name and location.
	/// </summary>
	public class PersistSchemaToFile : AbstractMenuCommand
	{
		IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		SchemaService _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

		public override bool IsEnabled
		{
			get
			{
				return _schema.IsSchemaLoaded && _schema.IsSchemaChanged;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
            IDatabasePersistenceProvider dbProvider = _persistence.SchemaProvider as IDatabasePersistenceProvider;
            if (dbProvider == null)
            {
                throw new Exception("The configured model persistence provider does not support this command.");
            }
            
            using (System.Windows.Forms.SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.CreatePrompt = true;
				dialog.DefaultExt = ".xml";
				dialog.Filter = ResourceUtils.GetString("XmlFilesFilter");
				dialog.OverwritePrompt = true;
				dialog.Title = ResourceUtils.GetString("SaveMetadataTitle");
			
				if(dialog.ShowDialog() == DialogResult.OK)
					dbProvider.PersistToFile(dialog.FileName, false);
			}
		}

		public override void Dispose()
		{
			_schema = null;
			_persistence = null;
		}

	}

	/// <summary>
	/// Exports schema to an XML file.
	/// </summary>
	public class ExportSchemaToFile : AbstractMenuCommand
	{
		private SchemaService _schema =
		    ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

        public override bool IsEnabled
        {
            get
            {
                bool modelPersistedToFileSystem =
                    ServiceManager.Services.GetService<IPersistenceService>() is FilePersistenceService;
                return _schema.IsSchemaLoaded && !modelPersistedToFileSystem;
            }
            set => base.IsEnabled = value;
	    }

	    public override void Run()
		{
			if(_schema.IsSchemaChanged) throw new Exception("Model not saved. Save the model before exporting.");

			using(System.Windows.Forms.SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.CreatePrompt = true;
				dialog.DefaultExt = ".xml";
				dialog.Filter = ResourceUtils.GetString("XmlFilesFilter");
				dialog.OverwritePrompt = true;
				dialog.Title = ResourceUtils.GetString("SaveMetadataTitle");
			
				if(dialog.ShowDialog() == DialogResult.OK)
				{
					SchemaExtension extension = _schema.ActiveExtension;
				
					IPersistenceService persistence = ServiceManager.Services.GetService<IPersistenceService>()
					    .Clone() as IPersistenceService;
					persistence.InitializeService();
					persistence.LoadSchemaList();

					persistence.LoadSchema((Guid)extension.PrimaryKey["Id"], true, true, null);

					(persistence.SchemaProvider as IDatabasePersistenceProvider).PersistToFile(dialog.FileName, true);
				}
			}
		}

		public override void Dispose()
		{
			_schema = null;
		}

	}

	/// <summary>
	/// Exports a single package to an XML file.
	/// </summary>
	public class ExportPackageToFile : AbstractMenuCommand
	{
		SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

        public override bool IsEnabled
        {
            get
            {
                bool modelPersistedToFileSystem =
                    ServiceManager.Services.GetService<IPersistenceService>() is FilePersistenceService;
                return schema.IsSchemaLoaded && !modelPersistedToFileSystem;
            }
            set => base.IsEnabled = value;
	    }

	    public override void Run()
		{
			if(schema.IsSchemaChanged) throw new Exception("Model not saved. Save the model before exporting.");

			IPersistenceService mainPersistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;

			using(System.Windows.Forms.SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.CreatePrompt = true;
				dialog.DefaultExt = ".xml";
				dialog.Filter = ResourceUtils.GetString("XmlFilesFilter");
				dialog.OverwritePrompt = true;
				dialog.Title = ResourceUtils.GetString("SaveMetadataTitle");
			
				if(dialog.ShowDialog() == DialogResult.OK)
				{
					SchemaExtension extension = schema.ActiveExtension;
				
					IPersistenceService persistence = mainPersistence;
					persistence.ExportPackage((Guid)extension.PrimaryKey["Id"], dialog.FileName);
				}
			}
		}

		public override void Dispose()
		{
			schema = null;
		}

	}

	/// <summary>
	/// Imports schema from XML file.
	/// </summary>
	public class ImportSchemaFromFile : AbstractMenuCommand
	{
		SchemaService _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

		public override bool IsEnabled
		{
			get
			{
				Pads.ExtensionPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(Pads.ExtensionPad)) as Pads.ExtensionPad;
			    IPersistenceService persistenceService =
			        ServiceManager.Services.GetService<IPersistenceService>();

                return _schema.IsSchemaLoaded && 
				       pad.SelectedExtension != null && 
                       !(persistenceService is FilePersistenceService);
			}
			set => base.IsEnabled = value;
		}

		public override void Run()
		{
			ImportSchemaFromFileIntoExtension cmd = new ImportSchemaFromFileIntoExtension();

			Pads.ExtensionPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(Pads.ExtensionPad)) as Pads.ExtensionPad;

			cmd.Owner = pad.SelectedExtension;
			cmd.Run();
		}

		public override void Dispose()
		{
			_schema = null;
		}

	}

    public class ImportPackagesFromRepository : AbstractMenuCommand
	{
        public override bool IsEnabled
        {
            get
            {
                bool modelPersistedToFileSystem =
                    ServiceManager.Services.GetService<IPersistenceService>() is FilePersistenceService;
                return WorkbenchSingleton.Workbench.IsConnected && !(modelPersistedToFileSystem);
            }
            set => base.IsEnabled = value;
	    }

	    public override void Run()
		{
            SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            if (schema.UnloadSchema())
            {
                ViewLogPad viewLog = new ViewLogPad();
                viewLog.Run();
                ModelImport.ModelImport modelImport = new ModelImport.ModelImport();
                modelImport.Import();
                Pads.ExtensionPad pad = WorkbenchSingleton.Workbench.GetPad(
                    typeof(Pads.ExtensionPad)) as Pads.ExtensionPad;
                pad.LoadPackages();
            }
		}
	}

	/// <summary>
	/// Imports schema from XML file.
	/// </summary>
	public class ImportPackageFromFile : AbstractMenuCommand
	{
		SchemaService schema =
		    ServiceManager.Services.GetService<SchemaService>();

        public override bool IsEnabled
		{
			get
			{
				Pads.ExtensionPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(Pads.ExtensionPad)) as Pads.ExtensionPad;
			    bool modelPersistedToFileSystem =
			        ServiceManager.Services.GetService<IPersistenceService>() is FilePersistenceService;
                return schema.IsSchemaLoaded &&
                       pad.SelectedExtension != null &&
                       !(modelPersistedToFileSystem);
			}
			set => base.IsEnabled = value;
		}

		public override void Run()
		{
			ImportPackageFromFileIntoExtension cmd = new ImportPackageFromFileIntoExtension();

			Pads.ExtensionPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(Pads.ExtensionPad)) as Pads.ExtensionPad;

			cmd.Owner = pad.SelectedExtension;
			cmd.Run();
		}

		public override void Dispose()
		{
			schema = null;
		}

	}

	/// <summary>
	/// Imports schema from XML file.
	/// </summary>
	public class ImportSchemaFromFileIntoExtension : AbstractCommand
	{
		public override void Run()
		{	
			IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

			if(schema.IsSchemaChanged) throw new Exception("Model not saved. Save the model before importing.");

			Schema.SchemaExtension extension = this.Owner as Schema.SchemaExtension;
				
			using(OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.DefaultExt = ".xml";
				dialog.Filter = ResourceUtils.GetString("XmlFilesFilter");
				dialog.Title = ResourceUtils.GetString("LoadSchemaTitle");
			
				if(dialog.ShowDialog() == DialogResult.OK)
				{
                    DataSet data = new DataSet();
					try
					{
						try
						{
                            data.ReadXml(dialog.FileName);
						} 
						catch
						{								
							throw new ConstraintException(String.Format("Constraint exception while loading data into dataset: `{0}'",
								DatasetTools.GetDatasetErrors(data)));

						}						
						ArrayList extensionIds = new ArrayList();
						foreach(DataRow row in data.Tables["SchemaExtension"].Rows)
						{
							extensionIds.Add(row["Id"]);
						}
						if(schema.LoadSchema(extensionIds, (Guid)extension.PrimaryKey["Id"], true, true))
						{
							schema.MergeSchema(data);
						}
					}
					finally
					{
						data.Dispose();
					}
				}
				else
				{
					throw new Exception(ResourceUtils.GetString("ErrorUserCanceled"));
				}
			}
		}
	}
	
	/// <summary>
	/// Imports a single package from XML file.
	/// </summary>
	public class ImportPackageFromFileIntoExtension : AbstractCommand
	{
		public override void Run()
		{	
			IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

			if(schema.IsSchemaChanged) throw new Exception("Model not saved. Save the model before importing.");

			Schema.SchemaExtension extension = this.Owner as Schema.SchemaExtension;
				
			using(OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.DefaultExt = ".xml";
				dialog.Filter = ResourceUtils.GetString("XmlFilesFilter");
				dialog.Title = ResourceUtils.GetString("LoadSchemaTitle");
			
				if(dialog.ShowDialog() == DialogResult.OK)
				{
                    DataSet data = new DataSet();
                    try
                    {
                        data.EnforceConstraints = false;
                        data.ReadXml(dialog.FileName);
                        data.EnforceConstraints = false;
						ArrayList extensionIds = new ArrayList();
						foreach(DataRow row in data.Tables["SchemaExtension"].Rows)
						{
							extensionIds.Add(row["Id"]);
						}
						persistence.MergePackage((Guid)extensionIds[0], data, null);
					}
					finally
					{
						data.Dispose();
					}
				}
				else
				{
					throw new Exception(ResourceUtils.GetString("ErrorUserCanceled"));
				}
			}
		}
	}
	

	/// <summary>
	/// Saves the active content window.
	/// </summary>
	public class SaveContent : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				// we cannot use IsDirty, because some forms get IsDirty after loosing focus, so 
				// Save button would be never enabled
				return WorkbenchSingleton.Workbench.ActiveDocument != null && WorkbenchSingleton.Workbench.ActiveDocument.IsViewOnly == false;  // && WorkbenchSingleton.Workbench.ActiveDocument.IsDirty;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			try
			{
				WorkbenchSingleton.Workbench.ActiveDocument.SaveObject();
				WorkbenchSingleton.Workbench.ActiveDocument.IsDirty = false;
                (ServiceManager.Services.GetService<IPersistenceService>() 
                    as FilePersistenceService)?.PersistIndex();
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(WorkbenchSingleton.Workbench as IWin32Window, ex.Message, "Chyba pøi ukládání dat formuláøe '" + WorkbenchSingleton.Workbench.ActiveDocument.TitleName + "'", ex);
			}
		}
	}

	/// <summary>
	/// Refreshes the active content window.
	/// </summary>
	public class RefreshContent : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return WorkbenchSingleton.Workbench.ActiveDocument != null && WorkbenchSingleton.Workbench.ActiveDocument.CanRefreshContent;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			try
			{
				WorkbenchSingleton.Workbench.ActiveDocument.RefreshContent();
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(WorkbenchSingleton.Workbench as IWin32Window, ex.Message, ResourceUtils.GetString("ErrorWhenRefreshForm", WorkbenchSingleton.Workbench.ActiveDocument.TitleName), ex);
			}
		}
	}

	/// <summary>
	/// Saves the active content window.
	/// </summary>
	public class TestForm : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return WorkbenchSingleton.Workbench.ActiveDocument != null;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			WorkbenchSingleton.Workbench.ActiveDocument.Test();
		}
	}

	/// <summary>
	/// Saves the active content window.
	/// </summary>
	public class ExitWorkbench : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			WorkbenchSingleton.Workbench.ExitWorkbench();
		}
	}

	/// <summary>
	/// Executes all the update scripts in order to update to the current model version
	/// </summary>
	public class DeployVersion : AbstractMenuCommand
	{
		SchemaService _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

		public override bool IsEnabled
		{
			get
			{
				if(_schema.IsSchemaLoaded)
				{
					try
					{
						IDeploymentService deployment = ServiceManager.Services.GetService(typeof(IDeploymentService)) as IDeploymentService;
						foreach(SchemaExtension extension in _schema.LoadedPackages)
						{
							if(deployment.CanUpdate(extension))
							{
								return true;
							}
						}
					}
                    catch (DatabaseTableNotFoundException ex)
                    {
                        return ex.TableName == "OrigamModelVersion";
                    }
					catch
					{
						return false;
					}
					return false;
				}
				else
				{
					return false;
				}
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
            Origam.Workbench.Commands.ViewLogPad logPad =
                new Origam.Workbench.Commands.ViewLogPad();
            logPad.Run();
            IDeploymentService deployment = ServiceManager.Services.GetService(typeof(IDeploymentService)) as IDeploymentService;
			deployment.Deploy();
		}

		public override void Dispose()
		{
			_schema = null;
			base.Dispose ();
		}

	}
}
