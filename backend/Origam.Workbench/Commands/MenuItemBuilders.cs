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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Origam.Git;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Workbench.Commands;

public class SchemaItemEditorsMenuBuilder : ISubmenuBuilder
{
    public SchemaItemEditorsMenuBuilder() { }

    public SchemaItemEditorsMenuBuilder(bool showDialog)
    {
        this.showDialog = showDialog;
    }

    private readonly bool showDialog;

    #region ISubmenuBuilder Members
    public bool LateBound
    {
        get { return true; }
    }

    public bool HasItems()
    {
        WorkbenchSchemaService sch =
            ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
            as WorkbenchSchemaService;
        if (!(sch.IsSchemaLoaded && sch.ActiveNode is ISchemaItemFactory))
        {
            return false;
        }

        ISchemaItemFactory factory = sch.ActiveNode as ISchemaItemFactory;
        if (factory.NewItemTypes == null)
        {
            return false;
        }

        if (factory.NewItemTypes.Length == 0)
        {
            return false;
        }

        return true;
    }

    public AsMenuCommand[] BuildSubmenu(object owner)
    {
        WorkbenchSchemaService sch =
            ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
            as WorkbenchSchemaService;
        object activeNode = owner ?? sch.ActiveNode;
        if (activeNode == null)
        {
            return new AsMenuCommand[0];
        }

        ISchemaItemFactory factory = (ISchemaItemFactory)activeNode;
        NonpersistentSchemaItemNode nonpersistentNode = activeNode as NonpersistentSchemaItemNode;
        ISchemaItem activeItem = activeNode as ISchemaItem;
        if (nonpersistentNode != null)
        {
            activeItem = nonpersistentNode.ParentNode as ISchemaItem;
        }
        var items = new List<AsMenuCommand>();
        if (factory.NewItemTypes != null)
        {
            var names = new List<string>();
            // filter out only the names that do not exist already in the children collection
            foreach (string name in factory.NewTypeNames)
            {
                bool found = false;
                foreach (ISchemaItem existing in activeItem.ChildItems)
                {
                    if (existing.Name == name)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    names.Add(item: name);
                }
            }
            var nameableTypes = new List<Type>();
            foreach (Type type in factory.NewItemTypes)
            {
                if (names.Count == 0 || !IsNameableType(factory: factory, type: type))
                {
                    AddNewItem(
                        sch: sch,
                        type: type,
                        parentElement: factory,
                        newItemName: null,
                        items: items,
                        showDialog: showDialog
                    );
                }
                else
                {
                    nameableTypes.Add(item: type);
                }
            }
            // populate a submenu builder for nameable types
            foreach (string name in names)
            {
                SchemaItemEditorNamesBuilder builder = new SchemaItemEditorNamesBuilder(
                    types: nameableTypes,
                    name: name,
                    parentElement: factory,
                    showDialog: showDialog
                );
                AddNewSubmenu(name: name, builder: builder, items: items);
            }
        }
        return items.ToArray();
    }

    private static bool IsNameableType(ISchemaItemFactory factory, Type type)
    {
        foreach (Type nameableType in factory.NameableTypes)
        {
            if (nameableType.Equals(o: type))
            {
                return true;
            }
        }
        return false;
    }

    public static void AddNewItem(
        WorkbenchSchemaService sch,
        Type type,
        ISchemaItemFactory parentElement,
        string newItemName,
        List<AsMenuCommand> items,
        bool showDialog
    )
    {
        AddNewSchemaItem cmd = new AddNewSchemaItem(showDialog: showDialog);
        cmd.ParentElement = parentElement;
        cmd.Owner = type;
        cmd.Name = newItemName;
        SchemaItemDescriptionAttribute attr = type.SchemaItemDescription();
        Image image = null;
        string name = type.Name;
        if (attr != null)
        {
            name = attr.Name;
            int imageIndex = -1;
            if (attr.Icon is string)
            {
                imageIndex = sch.SchemaBrowser.ImageList.Images.IndexOfKey(key: (string)attr.Icon);
            }
            else
            {
                imageIndex = (int)attr.Icon;
            }
            image = sch.SchemaBrowser.ImageList.Images[index: imageIndex];
        }
        AsMenuCommand menu = new AsMenuCommand(label: name, menuCommand: cmd);
        menu.Image = image;
        items.Add(item: menu);
        menu.Click += new EventHandler(EditNewItem);
    }

    private static void AddNewSubmenu(
        string name,
        ISubmenuBuilder builder,
        List<AsMenuCommand> items
    )
    {
        AsMenuCommand result = new AsMenuCommand(label: name, caller: builder);
        result.SubItems.Add(item: builder);
        items.Add(item: result);
    }
    #endregion
    private static void EditNewItem(object sender, EventArgs e)
    {
        try
        {
            (sender as AsMenuCommand).Command.Run();
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: WorkbenchSingleton.Workbench as Form,
                text: ex.Message,
                caption: ResourceUtils.GetString(key: "ErrorTitle"),
                exception: ex
            );
        }
    }
}

public class SchemaItemConvertMenuBuilder : ISubmenuBuilder
{
    #region ISubmenuBuilder Members
    public bool LateBound
    {
        get { return false; }
    }

    public bool HasItems()
    {
        WorkbenchSchemaService sch =
            ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
            as WorkbenchSchemaService;
        if (!sch.CanEditItem(item: sch.ActiveNode))
        {
            return false;
        }

        ISchemaItemFactory factory = ParentFactory(item: sch.ActiveNode);
        if (factory == null)
        {
            return false;
        }

        if (factory.NewItemTypes == null)
        {
            return false;
        }

        return true;
    }

    public AsMenuCommand[] BuildSubmenu(object owner)
    {
        if (!HasItems())
        {
            return new AsMenuCommand[0];
        }
        WorkbenchSchemaService sch =
            ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
            as WorkbenchSchemaService;
        ISchemaItemFactory factory = ParentFactory(item: sch.ActiveNode);
        var items = new List<AsMenuCommand>();
        for (int i = 0; i < factory.NewItemTypes.Length; ++i)
        {
            Type type = factory.NewItemTypes[i];
            if ((sch.ActiveNode as ISchemaItemConvertible).CanConvertTo(type: type))
            {
                ConvertSchemaItem cmd = new ConvertSchemaItem();
                cmd.Owner = type;
                SchemaItemDescriptionAttribute attr = type.SchemaItemDescription();
                string name;
                if (attr != null && attr.Name != null)
                {
                    name = attr.Name;
                }
                else
                {
                    name = type.Name;
                }
                AsMenuCommand menu = new AsMenuCommand(label: name, menuCommand: cmd);
                int imageIndex = -1;
                if (attr.Icon is string)
                {
                    imageIndex = sch.SchemaBrowser.ImageList.Images.IndexOfKey(
                        key: (string)attr.Icon
                    );
                }
                else
                {
                    imageIndex = (int)attr.Icon;
                }
                menu.Image = sch.SchemaBrowser.ImageList.Images[index: imageIndex];
                menu.Click += new EventHandler(ConvertItem);
                items.Add(item: menu);
            }
        }
        return items.ToArray();
    }
    #endregion
    private ISchemaItemFactory ParentFactory(object item)
    {
        if (item is ISchemaItem)
        {
            ISchemaItem schemaItem = (item as ISchemaItem);
            if (schemaItem.ParentItem == null)
            {
                return schemaItem.RootProvider;
            }

            return schemaItem.ParentItem;
        }
        return null;
    }

    private void ConvertItem(object sender, EventArgs e)
    {
        try
        {
            (sender as AsMenuCommand).Command.Run();
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: WorkbenchSingleton.Workbench as Form,
                text: ex.Message,
                caption: ResourceUtils.GetString(key: "ErrorTitle"),
                exception: ex
            );
        }
    }
}

public class SchemaItemEditorNamesBuilder : ISubmenuBuilder
{
    private List<Type> _types;
    private string _name;
    private ISchemaItemFactory _parentElement;

    public SchemaItemEditorNamesBuilder(
        List<Type> types,
        string name,
        ISchemaItemFactory parentElement,
        bool showDialog
    )
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
        get { return true; }
    }

    public bool HasItems()
    {
        return _types.Count > 0;
    }

    public AsMenuCommand[] BuildSubmenu(object owner)
    {
        WorkbenchSchemaService sch =
            ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
            as WorkbenchSchemaService;
        var items = new List<AsMenuCommand>();
        foreach (Type type in _types)
        {
            SchemaItemEditorsMenuBuilder.AddNewItem(
                sch: sch,
                type: type,
                parentElement: _parentElement,
                newItemName: _name,
                items: items,
                showDialog: ShowDialog
            );
        }
        return items.ToArray();
    }
    #endregion
}

public class GitMenuBuilder : ISubmenuBuilder
{
    readonly OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();

    AsMenuCommand[] items = new AsMenuCommand[1];
    public bool LateBound
    {
        get { return true; }
    }

    public AsMenuCommand[] BuildSubmenu(object owner)
    {
        AsMenuCommand menu = new AsMenuCommand(
            label: "Diff with previous version",
            menuCommand: new ShowFileDiffXml()
        );
        menu.Click += new EventHandler(ExeItem);
        items[0] = menu;
        return items;
    }

    public bool HasItems()
    {
        return items.Length > 0
            && GitManager.IsValid(modelSourceControlLocation: settings.ModelSourceControlLocation);
    }

    private void ExeItem(object sender, EventArgs e)
    {
        try
        {
            (sender as AsMenuCommand).Command.Run();
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: WorkbenchSingleton.Workbench as Form,
                text: ex.Message,
                caption: ResourceUtils.GetString(key: "ErrorTitle"),
                exception: ex
            );
        }
    }
}
