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
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Origam.Gui.UI;
using Origam.UI;
using Origam.Workbench.Services;
using Origam.Schema;
using Origam.Workbench.Editors;

namespace Origam.Workbench.Commands
{
	public class SchemaItemEditorsMenuBuilder : ISubmenuBuilder
	{
        public SchemaItemEditorsMenuBuilder()
        {

        }
        public SchemaItemEditorsMenuBuilder(bool showDialog)
        {
            ShowDialog = showDialog;
        }

        public bool ShowDialog { get; set; }
        #region ISubmenuBuilder Members
        public bool LateBound
        {
            get
            {
                return true;
            }
        }
        public bool HasItems()
        {
            WorkbenchSchemaService sch = ServiceManager.Services.GetService(
                typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
            if (!(sch.IsSchemaLoaded && sch.ActiveNode is ISchemaItemFactory)) return false;
            ISchemaItemFactory factory = sch.ActiveNode as ISchemaItemFactory;
            if (factory.NewItemTypes == null) return false;
            if (factory.NewItemTypes.Length == 0) return false;
            return true;
        }
        public ToolStripMenuItem[] BuildSubmenu(object owner)
        {
            WorkbenchSchemaService sch = ServiceManager.Services.GetService(
                typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
            object activeNode = owner ?? sch.ActiveNode;
            if (activeNode == null) return new ToolStripMenuItem[0];

            ISchemaItemFactory factory = (ISchemaItemFactory)activeNode;
            NonpersistentSchemaItemNode nonpersistentNode = activeNode as NonpersistentSchemaItemNode;
            AbstractSchemaItem activeItem = activeNode as AbstractSchemaItem;
            if (nonpersistentNode != null)
            {
                activeItem = nonpersistentNode.ParentNode as AbstractSchemaItem;
            }
            Type[] validTypes = ValidTypes(factory);
            ArrayList items;
            items = new ArrayList();
            if (factory.NewItemTypes != null)
            {
                ArrayList names = new ArrayList();
                // filter out only the names that do not exist already in the children collection
                foreach (string name in factory.NewTypeNames)
                {
                    bool found = false;
                    foreach (AbstractSchemaItem existing in activeItem.ChildItems)
                    {
                        if (existing.Name == name)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        names.Add(name);
                    }
                }

                ArrayList nameableTypes = new ArrayList();
                foreach (Type type in validTypes)
                {
                    if (names.Count == 0 || !IsNameableType(factory, type))
                    {
                        AddNewItem(sch, type, factory, null, items, ShowDialog);
                    }
                    else
                    {
                        nameableTypes.Add(type);
                    }
                }

                // populate a submenu builder for nameable types
                foreach (string name in names)
                {
                    SchemaItemEditorNamesBuilder builder =
                        new SchemaItemEditorNamesBuilder(nameableTypes, name, factory, ShowDialog);
                    AddNewSubmenu(name, builder, items);
                }
            }

            return (ToolStripMenuItem[])items.ToArray(typeof(ToolStripMenuItem));
        }

		private static bool IsNameableType(ISchemaItemFactory factory, Type type)
		{
			foreach(Type nameableType in factory.NameableTypes)
			{
				if(nameableType.Equals(type))
				{
					return true;
				}
			}
			return false;
		}

		public static Type[] ValidTypes(ISchemaItemFactory factory)
		{
			ArrayList result = new ArrayList();
			foreach (Type type in factory.NewItemTypes) 
			{
				if(LicensePolicy.ModelElementPolicy(type.Name, ModelElementPolicyCommand.Create))
				{
					result.Add(type);
				}
			}

			return (Type[])result.ToArray(typeof(Type));
		}

		public static void AddNewItem(WorkbenchSchemaService sch, Type type,
            ISchemaItemFactory parentElement, string newItemName, ArrayList items,
            bool showDialog)
		{
			AddNewSchemaItem cmd = new AddNewSchemaItem(showDialog);
            cmd.ParentElement = parentElement;
			cmd.Owner = type;
			cmd.Name = newItemName;
			SchemaItemDescriptionAttribute attr = type.SchemaItemDescription();
			Image image = null;
			string name = type.Name;
			if(attr != null)
			{
				name = attr.Name;
                int imageIndex = -1;
                if (attr.Icon is string)
                {
                    imageIndex = sch.SchemaBrowser.ImageList.Images.IndexOfKey((string)attr.Icon);
                }
                else
                {
                    imageIndex = (int)attr.Icon;
                }
                image = sch.SchemaBrowser.ImageList.Images[imageIndex];
            }
			AsMenuCommand menu = new AsMenuCommand(name, cmd);
			menu.Image = image;
			items.Add(menu);
			menu.Click += new EventHandler(EditNewItem);
		}

		private static void AddNewSubmenu(string name, ISubmenuBuilder builder, ArrayList items)
		{
			AsMenuCommand result = new AsMenuCommand(name, builder);
			result.SubItems.Add(builder);
			items.Add(result);
		}
		#endregion

		private static void EditNewItem(object sender, EventArgs e)
		{
			try
			{
				(sender as AsMenuCommand).Command.Run();
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(WorkbenchSingleton.Workbench as Form, ex.Message, ResourceUtils.GetString("ErrorTitle"), ex);
			}
		}
	}

	public class SchemaItemConvertMenuBuilder : ISubmenuBuilder
	{
        #region ISubmenuBuilder Members
        public bool LateBound
        {
            get
            {
                return false;
            }
        }

        public bool HasItems()
        {
            WorkbenchSchemaService sch = ServiceManager.Services.GetService(
                typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

            if (!sch.CanEditItem(sch.ActiveNode)) return false;
            ISchemaItemFactory factory = ParentFactory(sch.ActiveNode);
            if (factory == null) return false;
            if (factory.NewItemTypes == null) return false;
            return true;
        }
        public ToolStripMenuItem[] BuildSubmenu(object owner)
		{
            if (! HasItems())
            {
                return new ToolStripMenuItem[0];
            }
			WorkbenchSchemaService sch = ServiceManager.Services.GetService(
                typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
			ISchemaItemFactory factory = ParentFactory(sch.ActiveNode);
			ArrayList items = new ArrayList();
			for (int i = 0; i < factory.NewItemTypes.Length; ++i) 
			{
				Type type = factory.NewItemTypes[i];
				if((sch.ActiveNode as ISchemaItemConvertible).CanConvertTo(type))
				{
					ConvertSchemaItem cmd = new ConvertSchemaItem();
					cmd.Owner = type;
                    SchemaItemDescriptionAttribute attr = type.SchemaItemDescription();
					string name;
					if(attr != null && attr.Name != null)
					{
						name = attr.Name;
					}
					else
					{
						name = type.Name;
					}
					AsMenuCommand menu = new AsMenuCommand(name, cmd);
                    int imageIndex = -1;
                    if (attr.Icon is string)
                    {
                        imageIndex = sch.SchemaBrowser.ImageList.Images.IndexOfKey((string)attr.Icon);
                    }
                    else
                    {
                        imageIndex = (int)attr.Icon;
                    }
                    menu.Image = sch.SchemaBrowser.ImageList.Images[imageIndex];
					menu.Click += new EventHandler(ConvertItem);
					items.Add(menu);
				}
			}
            return (ToolStripMenuItem[])items.ToArray(typeof(ToolStripMenuItem));
		}
		#endregion

		private ISchemaItemFactory ParentFactory(object item)
		{
			if(item is ISchemaItem)
			{
				ISchemaItem schemaItem = (item as ISchemaItem);

				if(schemaItem.ParentItem == null)
				{
					return schemaItem.RootProvider;
				}
				else
				{
					return schemaItem.ParentItem;
				}
			}

			return null;
		}

		private void ConvertItem(object sender, EventArgs e)
		{
			try
			{
				(sender as AsMenuCommand).Command.Run();
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(WorkbenchSingleton.Workbench as Form, ex.Message, ResourceUtils.GetString("ErrorTitle"), ex);
			}
		}
	}


	public class ExtensionMenuBuilder : ISubmenuBuilder
	{
		#region ISubmenuBuilder Members
        public bool LateBound
        {
            get
            {
                return true;
            }
        }
        public bool HasItems()
        {
            WorkbenchSchemaService sch = ServiceManager.Services.GetService(
                typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
            if (sch.ActiveSchemaItem != null)
            {
                if (!LicensePolicy.ModelElementPolicy(
                    sch.ActiveSchemaItem.GetType().Name,
                    ModelElementPolicyCommand.MoveToPackage))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            if (sch.IsSchemaLoaded == false) return false;
            return sch.LoadedPackages.Count > 1;
        }

        public ToolStripMenuItem[] BuildSubmenu(object owner)
		{
			ArrayList extArray = new ArrayList();
            WorkbenchSchemaService sch = ServiceManager.Services.GetService(
                typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
            AddExtensionsToList(extArray, sch.LoadedPackages);
			extArray.Sort();
            ToolStripMenuItem[] items = new ToolStripMenuItem[extArray.Count];
			int i = 0;
			foreach(SchemaExtension ext in extArray)
			{
				MoveToExtension cmd = new MoveToExtension();
				cmd.Owner = ext;
				AsMenuCommand menu = new AsMenuCommand(ext.Name, cmd);
				items[i] = menu;
				menu.Click += new EventHandler(MoveItem);

				i++;
			}
			return items;
		}

		#endregion

		private void MoveItem(object sender, EventArgs e)
		{
			try
			{
				(sender as AsMenuCommand).Command.Run();
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(WorkbenchSingleton.Workbench as Form, ex.Message, ResourceUtils.GetString("ErrorTitle"), ex);
			}
		}

		private void AddExtensionsToList(ArrayList list, ArrayList extensions)
		{
			foreach(SchemaExtension ext in extensions)
			{
				list.Add(ext);
				//AddExtensionsToList(list, ext.ChildExtensions);
			}
		}
	}

	public class SchemaItemEditorNamesBuilder : ISubmenuBuilder
	{
        private ArrayList _types;
		private string _name;
        private ISchemaItemFactory _parentElement;

        public SchemaItemEditorNamesBuilder (ArrayList types, string name,
            ISchemaItemFactory parentElement, bool showDialog)
		{
			_types = types;
			_name = name;
            _parentElement = parentElement;
            ShowDialog = showDialog;
		}

        public bool ShowDialog { get; set; }

        #region ISubmenuBuilder Members
        public bool LateBound
        {
            get
            {
                return true;
            }
        }
        public bool HasItems()
        {
            return _types.Count > 0;
        }
		public ToolStripMenuItem[] BuildSubmenu(object owner)
		{
			WorkbenchSchemaService sch = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
			ArrayList items = new ArrayList();
			foreach(Type type in _types)
			{
				SchemaItemEditorsMenuBuilder.AddNewItem(sch, type, _parentElement, _name, items, ShowDialog);
			}
            return items.ToArray(typeof(ToolStripMenuItem)) as ToolStripMenuItem[];
		}
		#endregion
	}
}
