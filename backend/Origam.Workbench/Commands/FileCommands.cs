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
using System.Linq;
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

			editor.LoadObject(ConfigurationManager.GetAllUserHomeConfigurations());
			WorkbenchSingleton.Workbench.ShowView(editor);
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
			}
			catch(Exception ex)
			{
                AsMessageBox.ShowError(WorkbenchSingleton.Workbench as IWin32Window, ex.Message, ResourceUtils.GetString("ErrorWhenSaving", WorkbenchSingleton.Workbench.ActiveDocument.TitleName), ex);
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
						return _schema.ActiveExtension
							.IncludedPackages
							.Append(_schema.ActiveExtension)
							.Any(deployment.CanUpdate);
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
