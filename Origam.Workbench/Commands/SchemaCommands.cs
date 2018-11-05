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
using System.Windows.Forms;
using System.Reflection;

using WeifenLuo.WinFormsUI.Docking;

using Origam.UI;
using Origam.Workbench.Services;
using Origam.Schema;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.Workbench.Commands
{
	/// <summary>
	/// Creates a new schema item and displays it in editor.
	/// </summary>
	public class AddNewSchemaItem : AbstractMenuCommand
	{
        public AddNewSchemaItem(bool showDialog)
        {
            ShowDialog = showDialog;
        }

        public bool ShowDialog { get; set; }
        string _name = null;
        ISchemaItemFactory _parentElement = null;
        WorkbenchSchemaService _schema = ServiceManager.Services.GetService(
            typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				return _parentElement != null;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

        public ISchemaItemFactory ParentElement
        {
            get
            {
                return _parentElement;
            }
            set
            {
                _parentElement = value;
            }
        }

        public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public override void Run()
		{
			AbstractSchemaItem item = ParentElement.NewItem(this.Owner as Type, _schema.ActiveSchemaExtensionId, null);
			if(_name != null)
			{
				item.Name = _name;
			}

			// set abstract, if parent is abstract
			if(item.ParentItem != null && item.ParentItem.IsAbstract)
			{
				item.IsAbstract = true;
			}

			//_schema.SchemaBrowser.ebrSchemaBrowser.RefreshActiveNode();

			EditSchemaItem cmd = new EditSchemaItem(ShowDialog);
			cmd.Owner = item;

			_schema.LastAddedNodeParent = ParentElement;
			_schema.LastAddedType = this.Owner as Type;

			cmd.Run();
		}
	
		public override void Dispose()
		{
			_schema= null;
		}

	}

	/// <summary>
	/// Creates a new schema item of same type and under the same parent as last added schema item and displays it in editor.
	/// </summary>
	public class AddRepeatingSchemaItem : AbstractMenuCommand
	{
		SchemaService _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

		public override bool IsEnabled
		{
			get
			{
				return _schema.LastAddedNodeParent != null;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			AbstractSchemaItem item = _schema.LastAddedNodeParent.NewItem(
                _schema.LastAddedType, _schema.ActiveSchemaExtensionId, null);
			// set abstract, if parent is abstract
			if(item.ParentItem != null && item.ParentItem.IsAbstract)
			{
				item.IsAbstract = true;
			}
			EditSchemaItem cmd = new EditSchemaItem();
			cmd.Owner = item;
			cmd.Run();
		}

		public override void Dispose()
		{
			_schema = null;
		}

	}

	/// <summary>
	/// Converts existing schema item to a new type and displays it in editor. Before it delets the existing item.
	/// </summary>
	public class ConvertSchemaItem : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				return _schema.ActiveNode is ISchemaItemConvertible;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			ISchemaItemConvertible activeItem = _schema.ActiveNode as ISchemaItemConvertible;

			ISchemaItem converted = activeItem.ConvertTo(this.Owner as Type);

			//_schema.UpdateBrowser();
			EditSchemaItem cmd = new EditSchemaItem();
			cmd.Owner = converted;

			cmd.Run();
		}

		public override void Dispose()
		{
			_schema = null;

			base.Dispose ();
		}

	}

	/// <summary>
	/// Moves an existing schema item to a different schema extension.
	/// </summary>
	public class MoveToExtension : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				return true; //_schema.CanEditItem(_schema.ActiveNode);
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			if(!(this.Owner is SchemaExtension))
			{
				throw new ArgumentOutOfRangeException("Owner", this.Owner, ResourceUtils.GetString("ErrorNotSchemaExtension"));
			}

			if(_schema.ActiveNode is SchemaItemGroup)
			{
				SchemaItemGroup activeItem = _schema.ActiveNode as SchemaItemGroup;

				activeItem.SchemaExtension = this.Owner as SchemaExtension;
			
				activeItem.Persist();
			}
			else if(_schema.ActiveNode is ISchemaItem)
			{
				AbstractSchemaItem activeItem = _schema.ActiveNode as AbstractSchemaItem;

				activeItem.SetExtensionRecursive(this.Owner as SchemaExtension);

				activeItem.Persist();
			}
		}

		public override void Dispose()
		{
			_schema = null;

			base.Dispose ();
		}

	}

	/// <summary>
	/// Creates a new group and displays it in editor.
	/// </summary>
	public class AddNewGroup : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				return _schema.ActiveNode is AbstractSchemaItemProvider || _schema.ActiveNode is SchemaItemGroup;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			SchemaItemGroup item = (_schema.ActiveNode as ISchemaItemFactory).NewGroup(_schema.ActiveSchemaExtensionId);
			_schema.SchemaBrowser.EbrSchemaBrowser.RefreshActiveNode();
		}

		public override void Dispose()
		{
			_schema = null;

			base.Dispose ();
		}

	}

	/// <summary>
	/// Edits an active schema item in a diagram editor.
	/// </summary>
	public class EditDiagramActiveSchemaItem : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
		IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;

		public override bool IsEnabled
		{
			get
			{
				return _schema.ActiveNode is AbstractSchemaItem;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			AbstractSchemaItem item = _schema.ActiveSchemaItem;

			// First we test, if the item is not opened already
			foreach(IViewContent content in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				if(content.DisplayedItemId == item.Id)
				{
					(content as DockContent).Activate();

					return;
				}
			}

			System.Reflection.Assembly a = Assembly.LoadWithPartialName("Origam.Workbench.Diagram");
			IViewContent editor = a.CreateInstance("Origam.Workbench.Editors.DiagramEditor") as IViewContent;

			// Set editor to dirty, if object has not been persisted, yet (new item)
			if(!item.IsPersisted)
				editor.IsDirty = true;
			else
			{
				// Get a copy of the item to edit (no cache usage => we get a fresh copy)
				AbstractSchemaItem freshItem = _persistence.SchemaProvider.RetrieveInstance(item.GetType(), item.PrimaryKey, false) as AbstractSchemaItem;
				freshItem.ParentItem = item.ParentItem;
				item = freshItem;
			}

			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			editor.LoadObject(item);
			editor.TitleName = item.Name;
			editor.DisplayedItemId = item.Id;

			WorkbenchSingleton.Workbench.ShowView(editor);
			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

		}

		public override void Dispose()
		{
			_schema = null;
			_persistence = null;
		}

	}

	/// <summary>
	/// Edits an active schema item in an editor.
	/// </summary>
	public class EditActiveSchemaItem : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				if(_schema.IsSchemaLoaded)
				{
					return _schema.CanEditItem(_schema.ActiveNode);
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
			EditSchemaItem cmd = new EditSchemaItem();
			cmd.Owner = _schema.ActiveNode;
			cmd.Run();
		}

		public override void Dispose()
		{
			_schema = null;

			base.Dispose ();
		}

	}

    /// <summary>
    /// Edits a schema item in an editor. Schema item is passed as Owner.
    /// </summary>
    public class EditSchemaItem : AbstractCommand
    {
        public EditSchemaItem()
        {
            
        }

        public EditSchemaItem(bool showDialog)
        {
            ShowDialog = showDialog;
        }

        public bool ShowDialog { get; set; }
        IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		WorkbenchSchemaService _schemaService = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
//		private IParameterService _parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

		public override void Run()
		{	
			// First we test, if the item is not opened already
			foreach(IViewContent content in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				if(content.DisplayedItemId == (Owner as IPersistent).Id)
				{
					(content as DockContent).Activate();

					return;
				}
			}
			IViewContent editor;
			IPersistent item;
			if(Owner is AbstractSchemaItem || Owner is SchemaExtension)
			{
				item = this.Owner as IPersistent;
			}
			else
			{
				throw new ArgumentOutOfRangeException("Owner", this.Owner, ResourceUtils.GetString("ErrorEditObject"));
			}
            string itemType = item.GetType().ToString();
            if (item is SchemaExtension)
			{
				editor = new Origam.Workbench.Editors.PackageEditor();
			}
			else if(itemType == "Origam.Schema.GuiModel.FormControlSet" 
				|| itemType == "Origam.Schema.GuiModel.PanelControlSet"
				|| itemType == "Origam.Schema.GuiModel.ControlSetItem")
			{
				System.Reflection.Assembly a = Assembly.LoadWithPartialName("Origam.Gui.Designer");
				editor = a.CreateInstance("Origam.Gui.Designer.ControlSetEditor") as IViewContent;
				if(editor == null)
					throw new Exception(ResourceUtils.GetString("ErrorLoadEditorFailed"));
			}
			else if(itemType == "Origam.Schema.EntityModel.XslTransformation"
                || itemType == "Origam.Schema.RuleModel.XslRule"
                || itemType == "Origam.Schema.RuleModel.EndRule"
                || itemType == "Origam.Schema.RuleModel.ComplexDataRule")
			{
				System.Reflection.Assembly a = Assembly.LoadWithPartialName("Origam.Workbench");
				editor = a.CreateInstance("Origam.Workbench.Editors.XslEditor") as IViewContent;
				if(editor == null)
					throw new Exception(ResourceUtils.GetString("ErrorLoadEditorFailed"));
			}
			else if(itemType == "Origam.Schema.EntityModel.XsdDataStructure")
			{
				System.Reflection.Assembly a = Assembly.LoadWithPartialName("Origam.Schema.EntityModel.UI");
				editor = a.CreateInstance("Origam.Schema.EntityModel.XsdEditor") as IViewContent;
				if(editor == null)
					throw new Exception(ResourceUtils.GetString("ErrorLoadEditorFailed"));
			}
			else if(itemType == "Origam.Schema.DeploymentModel.ServiceCommandUpdateScriptActivity")
			{
				System.Reflection.Assembly a = Assembly.LoadWithPartialName("Origam.Schema.DeploymentModel");
				editor = a.CreateInstance("Origam.Schema.DeploymentModel.ServiceScriptCommandEditor") as IViewContent;
				if(editor == null)
					throw new Exception(ResourceUtils.GetString("ErrorLoadEditorFailed"));
			}
			else
			{
				editor = new Origam.Workbench.Editors.PropertyGridEditor();
			}

			// Set editor to dirty, if object has not been persisted, yet (new item)
			if(!item.IsPersisted)
			{
				editor.IsDirty = true;
			}
			else
			{
				// Get a copy of the item to edit (no cache usage => we get a fresh copy)
				item = item.GetFreshItem();
			}

			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			if(! _schemaService.CanEditItem(item))
			{
				editor.IsReadOnly = true;
			}

			editor.LoadObject(item);
			editor.DisplayedItemId = item.Id;

			if(item is AbstractSchemaItem)
			{
				editor.TitleName = (item as AbstractSchemaItem).Name;
				if((item as AbstractSchemaItem).NodeImage == null)
				{
					(editor as Form).Icon = System.Drawing.Icon.FromHandle(((System.Drawing.Bitmap)_schemaService.SchemaBrowser.ImageList.Images[Convert.ToInt32((item as AbstractSchemaItem).Icon)]).GetHicon());
				}
				else
				{
					(editor as Form).Icon = System.Drawing.Icon.FromHandle((item as AbstractSchemaItem)
						.NodeImage.ToBitmap()
						.GetHicon());
				}
			}
			else if(item is SchemaExtension)
			{
				editor.TitleName = (item as SchemaExtension).Name;
			}

            if (ShowDialog)
            {
                (editor as Form).ShowDialog(WorkbenchSingleton.Workbench as IWin32Window);
            }
            else
            {
                WorkbenchSingleton.Workbench.ShowView(editor);
            }
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
		}
	}

	/// <summary>
	/// Delets the currently selected node.
	/// </summary>
	public class DeleteActiveNode : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				if(_schema.IsSchemaLoaded && _schema.ActiveNode != null)
				{
					if(! (_schema.CanDeleteItem(_schema.ActiveNode)))
					{
						return false;
					}

					return _schema.ActiveNode.CanDelete;
				}
				
				return false;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			if((_schema.ActiveNode is AbstractSchemaItem && (_schema.ActiveNode as AbstractSchemaItem).SchemaExtensionId != _schema.ActiveSchemaExtensionId)
				| (_schema.ActiveNode is SchemaItemGroup && (_schema.ActiveNode as SchemaItemGroup).SchemaExtensionId != _schema.ActiveSchemaExtensionId))
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErrorDeleteItemNotActiveExtension"));
			}

			if(MessageBox.Show(ResourceUtils.GetString("DoYouWishDelete", _schema.ActiveNode.NodeText), ResourceUtils.GetString("DeleteTile"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				// first close an open editor
				foreach(IViewContent content in WorkbenchSingleton.Workbench.ViewContentCollection)
				{
					if(content.DisplayedItemId == (_schema.ActiveNode as IPersistent).Id)
					{
						(content as IViewContent).IsDirty = false;
						(content as DockContent).Close();
						break;
					}
				}
                ServiceManager.Services.GetService<IPersistenceService>()
                    .SchemaProvider.BeginTransaction();
				// then delete from the model
				_schema.ActiveNode.Delete();
                ServiceManager.Services.GetService<IPersistenceService>()
                    .SchemaProvider.EndTransaction();
            }
		}

		public override void Dispose()
		{
			_schema = null;
		}

	}

	/// <summary>
	/// Displays the documentation for the active schema item in the documentation pad
	/// </summary>
	public class ShowDocumentation : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				return _schema.ActiveSchemaItem != null;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			Pads.DocumentationPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(Pads.DocumentationPad)) as Pads.DocumentationPad;
			pad.ShowDocumentation(_schema.ActiveSchemaItem.Id);
		}

		public override void Dispose()
		{
			_schema = null;

			base.Dispose ();
		}

	}

	/// <summary>
	/// Displays list of items on which currently selected item is dependent
	/// </summary>
	public class ShowDependencies : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				return _schema.ActiveSchemaItem != null;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			Pads.FindSchemaItemResultsPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(Pads.FindSchemaItemResultsPad)) as Pads.FindSchemaItemResultsPad;

			ArrayList dependencies =_schema.ActiveSchemaItem.GetDependencies(false).ToArrayList();

			pad.DisplayResults((AbstractSchemaItem[])dependencies.ToArray(typeof(AbstractSchemaItem)));

			ViewFindSchemaItemResultsPad cmd = new ViewFindSchemaItemResultsPad();
			cmd.Run();
			cmd.Dispose();
		}

		public override void Dispose()
		{
			_schema = null;

			base.Dispose ();
		}

	}

	/// <summary>
	/// Displays list of items on which currently selected item is dependent
	/// </summary>
	public class ShowUsage : AbstractMenuCommand
	{
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override bool IsEnabled
		{
			get
			{
				return _schema.ActiveSchemaItem != null;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

		public override void Run()
		{
			Pads.FindSchemaItemResultsPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(Pads.FindSchemaItemResultsPad)) as Pads.FindSchemaItemResultsPad;

			pad.DisplayResults((AbstractSchemaItem[])_schema.ActiveSchemaItem.GetUsage(false).ToArray(typeof(AbstractSchemaItem)));

			ViewFindSchemaItemResultsPad cmd = new ViewFindSchemaItemResultsPad();
			cmd.Run();
			cmd.Dispose();
		}

		public override void Dispose()
		{
			_schema = null;

			base.Dispose ();
		}

	}
}
