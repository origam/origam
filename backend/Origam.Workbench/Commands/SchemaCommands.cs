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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using LibGit2Sharp;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Git;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.Windows.Editor.GIT;
using Origam.Workbench.Editors;
using Origam.Workbench.Services;
using WeifenLuo.WinFormsUI.Docking;

namespace Origam.Workbench.Commands;

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
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return _parentElement != null; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }
    public ISchemaItemFactory ParentElement
    {
        get { return _parentElement; }
        set { _parentElement = value; }
    }
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
    public event EventHandler<ISchemaItem> ItemCreated;

    public override void Run()
    {
        var newItemMethodInfo = typeof(ISchemaItemFactory).GetMethod(name: "NewItem");
        var newItemGenericMethodInfo = newItemMethodInfo.MakeGenericMethod(
            typeArguments: Owner as Type
        );
        var item = newItemGenericMethodInfo.Invoke(
            obj: ParentElement,
            parameters: new object[] { _schema.ActiveSchemaExtensionId, null }
        );
        var abstractSchemaItem = (ISchemaItem)item;
        if (_name != null)
        {
            abstractSchemaItem.Name = _name;
        }
        // set abstract, if parent is abstract
        if ((abstractSchemaItem.ParentItem != null) && abstractSchemaItem.ParentItem.IsAbstract)
        {
            abstractSchemaItem.IsAbstract = true;
        }
        EditSchemaItem cmd = new EditSchemaItem(showDialog: ShowDialog);
        cmd.Owner = item;
        _schema.LastAddedNodeParent = ParentElement;
        _schema.LastAddedType = this.Owner as Type;
        ItemCreated?.Invoke(sender: this, e: abstractSchemaItem);
        cmd.Run();
    }

    public override void Dispose()
    {
        _schema = null;
    }
}

/// <summary>
/// Creates a new schema item of same type and under the same parent as last added schema item and displays it in editor.
/// </summary>
public class AddRepeatingSchemaItem : AbstractMenuCommand
{
    SchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
    public override bool IsEnabled
    {
        get { return _schema.LastAddedNodeParent != null; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        var newItemMethodInfo = typeof(ISchemaItemFactory).GetMethod(name: "NewItem");
        var newItemGenericMethodInfo = newItemMethodInfo.MakeGenericMethod(
            typeArguments: _schema.LastAddedType
        );
        var item = newItemGenericMethodInfo.Invoke(
            obj: _schema.LastAddedNodeParent,
            parameters: new object[] { _schema.ActiveSchemaExtensionId, null }
        );
        var abstractSchemaItem = (ISchemaItem)item;
        // set abstract, if parent is abstract
        if ((abstractSchemaItem.ParentItem != null) && abstractSchemaItem.ParentItem.IsAbstract)
        {
            abstractSchemaItem.IsAbstract = true;
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
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return _schema.ActiveNode is ISchemaItemConvertible; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        ISchemaItemConvertible activeItem = _schema.ActiveNode as ISchemaItemConvertible;
        ISchemaItem converted = activeItem.ConvertTo(type: this.Owner as Type);
        //_schema.UpdateBrowser();
        EditSchemaItem cmd = new EditSchemaItem();
        cmd.Owner = converted;
        cmd.Run();
    }

    public override void Dispose()
    {
        _schema = null;
        base.Dispose();
    }
}

/// <summary>
/// Creates a new group and displays it in editor.
/// </summary>
public class AddNewGroup : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get
        {
            return _schema.ActiveNode is AbstractSchemaItemProvider
                || _schema.ActiveNode is SchemaItemGroup;
        }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        SchemaItemGroup item = (_schema.ActiveNode as ISchemaItemFactory).NewGroup(
            schemaExtensionId: _schema.ActiveSchemaExtensionId
        );
        _schema.SchemaBrowser.EbrSchemaBrowser.RefreshActiveNode();
    }

    public override void Dispose()
    {
        _schema = null;
        base.Dispose();
    }
}

/// <summary>
/// Edits an active schema item in a diagram editor.
/// </summary>
public class EditDiagramActiveSchemaItem : AbstractCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    IPersistenceService _persistence =
        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
        as IPersistenceService;

    public override void Run()
    {
        ISchemaItem item = this.Owner as ISchemaItem;
        // First we test, if the item is not opened already
        foreach (IViewContent content in WorkbenchSingleton.Workbench.ViewContentCollection)
        {
            if (
                content.DisplayedItemId == item.Id
                && content.GetType().ToString() == "Origam.Workbench.Editors.DiagramEditor"
            )
            {
                (content as DockContent).Activate();
                return;
            }
        }
        Assembly a = Assembly.Load(assemblyString: "Origam.Workbench.Diagram");
        IViewContent editor =
            a.CreateInstance(typeName: "Origam.Workbench.Editors.DiagramEditor") as IViewContent;
        // Set editor to dirty, if object has not been persisted, yet (new item)
        if (!item.IsPersisted)
        {
            editor.IsDirty = true;
        }
        else
        {
            // Get a copy of the item to edit (no cache usage => we get a fresh copy)
            ISchemaItem freshItem =
                _persistence.SchemaProvider.RetrieveInstance(
                    type: item.GetType(),
                    primaryKey: item.PrimaryKey,
                    useCache: false
                ) as ISchemaItem;
            freshItem.ParentItem = item.ParentItem;
            item = freshItem;
        }
        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
        editor.LoadObject(objectToLoad: item);
        editor.TitleName = item.Name;
        editor.DisplayedItemId = item.Id;
        WorkbenchSingleton.Workbench.ShowView(content: editor);
        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
    }
}

/// <summary>
/// Edits an active schema item in an editor.
/// </summary>
public class EditActiveSchemaItem : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get
        {
            if (_schema.IsSchemaLoaded)
            {
                return _schema.CanEditItem(item: _schema.ActiveNode);
            }

            return false;
        }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
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
        base.Dispose();
    }
}

public class ExpandAllActiveSchemaItem : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return _schema.ActiveNode is ISchemaItem; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        if (_schema.ActiveNode.HasChildNodes)
        {
            ExpressionBrowser schemaBrowser = _schema.SchemaBrowser.EbrSchemaBrowser;
            schemaBrowser.ExpandAllChildNodes(browserNode: _schema.ActiveNode);
        }
    }

    public override void Dispose()
    {
        _schema = null;
        base.Dispose();
    }
}

/// <summary>
/// Edits a schema item in an editor. Schema item is passed as Owner.
/// </summary>
public class EditSchemaItem : AbstractCommand
{
    public EditSchemaItem() { }

    public EditSchemaItem(bool showDialog)
    {
        ShowDialog = showDialog;
    }

    public bool ShowDialog { get; set; }
    public bool ShowDiagramEditorAfterSave { get; set; }
    IPersistenceService _persistence =
        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
        as IPersistenceService;
    WorkbenchSchemaService _schemaService =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;

    //		private IParameterService _parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
    public override void Run()
    {
        // First we test, if the item is not opened already
        foreach (IViewContent content in WorkbenchSingleton.Workbench.ViewContentCollection)
        {
            if (content.DisplayedItemId == (Owner as IPersistent).Id && content is AbstractEditor)
            {
                (content as DockContent).Activate();
                return;
            }
        }
        IViewContent editor;
        IPersistent item;
        if (Owner is ISchemaItem || Owner is Package)
        {
            item = this.Owner as IPersistent;
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                paramName: "Owner",
                actualValue: this.Owner,
                message: ResourceUtils.GetString(key: "ErrorEditObject")
            );
        }
        string itemType = item.GetType().ToString();
        if (item is Package)
        {
            editor = new Origam.Workbench.Editors.PackageEditor();
        }
        else if (
            itemType == "Origam.Schema.GuiModel.FormControlSet"
            || itemType == "Origam.Schema.GuiModel.PanelControlSet"
            || itemType == "Origam.Schema.GuiModel.ControlSetItem"
        )
        {
            System.Reflection.Assembly a = Assembly.Load(assemblyString: "Origam.Gui.Designer");
            editor =
                a.CreateInstance(typeName: "Origam.Gui.Designer.ControlSetEditor") as IViewContent;
            if (editor == null)
            {
                throw new Exception(message: ResourceUtils.GetString(key: "ErrorLoadEditorFailed"));
            }
        }
        else if (
            itemType == "Origam.Schema.EntityModel.XslTransformation"
            || itemType == "Origam.Schema.RuleModel.XslRule"
            || itemType == "Origam.Schema.RuleModel.EndRule"
            || itemType == "Origam.Schema.RuleModel.ComplexDataRule"
        )
        {
            System.Reflection.Assembly a = Assembly.Load(assemblyString: "Origam.Workbench");
            editor =
                a.CreateInstance(typeName: "Origam.Workbench.Editors.XslEditor") as IViewContent;
            if (editor == null)
            {
                throw new Exception(message: ResourceUtils.GetString(key: "ErrorLoadEditorFailed"));
            }
        }
        else if (itemType == "Origam.Schema.EntityModel.XsdDataStructure")
        {
            System.Reflection.Assembly a = Assembly.Load(
                assemblyString: "Origam.Schema.EntityModel.UI"
            );
            editor =
                a.CreateInstance(typeName: "Origam.Schema.EntityModel.UI.XsdEditor")
                as IViewContent;
            if (editor == null)
            {
                throw new Exception(message: ResourceUtils.GetString(key: "ErrorLoadEditorFailed"));
            }
        }
        else if (itemType == "Origam.Schema.DeploymentModel.ServiceCommandUpdateScriptActivity")
        {
            System.Reflection.Assembly a = Assembly.Load(
                assemblyString: "Origam.Schema.DeploymentModel.UI"
            );
            editor =
                a.CreateInstance(
                    typeName: "Origam.Schema.DeploymentModel.ServiceScriptCommandEditor"
                ) as IViewContent;
            if (editor == null)
            {
                throw new Exception(message: ResourceUtils.GetString(key: "ErrorLoadEditorFailed"));
            }
        }
        else if (item is EntityUIAction)
        {
            editor = new UiActionEditor(closeOnLinkClick: ShowDialog);
        }
        else if (itemType == "Origam.Schema.WorkflowModel.Workflow" && !ShowDialog)
        {
            if (item.IsPersisted)
            {
                var diagramAction = new EditDiagramActiveSchemaItem();
                diagramAction.Owner = this.Owner;
                diagramAction.Run();
                return;
            }
            this.ShowDialog = true;
            editor = new PropertyGridEditor(closeOnLinkClick: ShowDialog);

            this.ShowDiagramEditorAfterSave = true;
        }
        else
        {
            editor = new PropertyGridEditor(closeOnLinkClick: ShowDialog);
        }
        // Set editor to dirty, if object has not been persisted, yet (new item)
        if (!item.IsPersisted)
        {
            editor.IsDirty = true;
        }
        else
        {
            // Get a copy of the item to edit (no cache usage => we get a fresh copy)
            item = item.GetFreshItem();
        }
        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
        if (!_schemaService.CanEditItem(item: item))
        {
            editor.IsReadOnly = true;
        }
        editor.LoadObject(objectToLoad: item);
        editor.DisplayedItemId = item.Id;
        if (item is ISchemaItem)
        {
            editor.TitleName = (item as ISchemaItem).Name;
            if ((item as ISchemaItem).NodeImage == null)
            {
                (editor as Form).Icon = System.Drawing.Icon.FromHandle(
                    handle: (
                        (System.Drawing.Bitmap)
                            _schemaService.SchemaBrowser.ImageList.Images[
                                index: _schemaService.SchemaBrowser.ImageIndex(
                                    icon: (item as ISchemaItem).Icon
                                )
                            ]
                    ).GetHicon()
                );
            }
            else
            {
                (editor as Form).Icon = System.Drawing.Icon.FromHandle(
                    handle: (item as ISchemaItem).NodeImage.ToBitmap().GetHicon()
                );
            }
        }
        else if (item is Package)
        {
            editor.TitleName = (item as Package).Name;
        }
        if (ShowDialog)
        {
            var result = (editor as Form).ShowDialog(
                owner: WorkbenchSingleton.Workbench as IWin32Window
            );
            if (result == DialogResult.OK && ShowDiagramEditorAfterSave)
            {
                this.ShowDialog = false;
                ShowDiagramEditorAfterSave = false;
                Run();
            }
        }
        else
        {
            WorkbenchSingleton.Workbench.ShowView(content: editor);
        }
        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
    }
}

/// <summary>
/// Delets the currently selected node.
/// </summary>
public class DeleteActiveNode : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public event EventHandler BeforeDelete;
    public event EventHandler AfterDelete;
    public override bool IsEnabled
    {
        get
        {
            if (_schema.IsSchemaLoaded && _schema.ActiveNode != null)
            {
                if (!(_schema.CanDeleteItem(item: _schema.ActiveNode)))
                {
                    return false;
                }
                return _schema.ActiveNode.CanDelete;
            }

            return false;
        }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        if (
            (
                _schema.ActiveNode is ISchemaItem
                && (_schema.ActiveNode as ISchemaItem).SchemaExtensionId
                    != _schema.ActiveSchemaExtensionId
            )
            | (
                _schema.ActiveNode is SchemaItemGroup
                && (_schema.ActiveNode as SchemaItemGroup).SchemaExtensionId
                    != _schema.ActiveSchemaExtensionId
            )
        )
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorDeleteItemNotActiveExtension")
            );
        }
        if (
            MessageBox.Show(
                text: ResourceUtils.GetString(
                    key: "DoYouWishDelete",
                    args: _schema.ActiveNode.NodeText
                ),
                caption: ResourceUtils.GetString(key: "DeleteTile"),
                buttons: MessageBoxButtons.YesNo,
                icon: MessageBoxIcon.Warning
            ) == DialogResult.Yes
        )
        {
            // first close an open editor
            foreach (
                IViewContent content in WorkbenchSingleton.Workbench.ViewContentCollection.ToArray<IViewContent>()
            )
            {
                if (content.DisplayedItemId == (_schema.ActiveNode as IPersistent).Id)
                {
                    content.IsDirty = false;
                    (content as DockContent).Close();
                }
            }
            IPersistenceProvider persistenceProvider = ServiceManager
                .Services.GetService<IPersistenceService>()
                .SchemaProvider;
            // then delete from the model
            BeforeDelete?.Invoke(sender: this, e: EventArgs.Empty);
            try
            {
                persistenceProvider.BeginTransaction();
                _schema.ActiveNode.Delete();
                AfterDelete?.Invoke(sender: this, e: EventArgs.Empty);
            }
            catch
            {
                // it might fail because of references
                persistenceProvider.EndTransactionDontSave();
                throw;
            }
            persistenceProvider.EndTransaction();
        }
    }

    public override void Dispose()
    {
        _schema = null;
    }
}

public class MoveToAnotherPackage : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get
        {
            if (
                (_schema.ActiveNode is not ISchemaItem && _schema.ActiveNode is not SchemaItemGroup)
                || !_schema.IsSchemaLoaded
                || _schema.LoadedPackages.Count < 2
                || _schema.ActiveExtension.IncludedPackages.Count == 0
            )
            {
                return false;
            }
            return true;
        }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        var selectForm = new MoveToPackageForm();
        selectForm.ShowDialog(owner: WorkbenchSingleton.Workbench as IWin32Window);
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
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return _schema.ActiveSchemaItem != null; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        Pads.DocumentationPad pad =
            WorkbenchSingleton.Workbench.GetPad(type: typeof(Pads.DocumentationPad))
            as Pads.DocumentationPad;
        pad.ShowDocumentation(schemaItemId: _schema.ActiveSchemaItem.Id);
    }

    public override void Dispose()
    {
        _schema = null;
        base.Dispose();
    }
}

/// <summary>
/// Displays list of items on which currently selected item is dependent
/// </summary>
public class ShowDependencies : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return _schema.ActiveSchemaItem != null; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        Pads.FindSchemaItemResultsPad pad =
            WorkbenchSingleton.Workbench.GetPad(type: typeof(Pads.FindSchemaItemResultsPad))
            as Pads.FindSchemaItemResultsPad;
        var dependencies = _schema
            .ActiveSchemaItem.GetDependencies(ignoreErrors: false)
            .Cast<ISchemaItem>()
            .Where(predicate: x => x != null)
            .ToArray();
        pad.DisplayResults(results: dependencies);
        ViewFindSchemaItemResultsPad cmd = new ViewFindSchemaItemResultsPad();
        cmd.Run();
        cmd.Dispose();
    }

    public override void Dispose()
    {
        _schema = null;
        base.Dispose();
    }
}

/// <summary>
/// Displays list of items on which currently selected item is dependent
/// </summary>
public class ShowUsage : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return _schema.ActiveSchemaItem != null; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        Pads.FindSchemaItemResultsPad pad =
            WorkbenchSingleton.Workbench.GetPad(type: typeof(Pads.FindSchemaItemResultsPad))
            as Pads.FindSchemaItemResultsPad;
        var referenceList = _schema.ActiveSchemaItem.GetUsage();
        if (referenceList != null)
        {
            pad.DisplayResults(results: referenceList.ToArray());
        }
        ViewFindSchemaItemResultsPad cmd = new ViewFindSchemaItemResultsPad();
        cmd.Run();
        cmd.Dispose();
    }

    public override void Dispose()
    {
        _schema = null;
        base.Dispose();
    }
}

/// <summary>
/// Show file in directory in explorer.
/// </summary>
public class ShowExplorerXml : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return _schema.ActiveSchemaItem != null; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        foreach (string file in _schema.ActiveSchemaItem.Files)
        {
            string filePath = Path.Combine(
                path1: settings.ModelSourceControlLocation.Replace(oldValue: "/", newValue: "\\"),
                path2: file
            );
            if (File.Exists(path: filePath))
            {
                Process.Start(fileName: "explorer.exe", arguments: "/select," + filePath);
            }
            break;
        }
    }

    public override void Dispose()
    {
        _schema = null;
        base.Dispose();
    }
}

/// <summary>
/// Show file in directory in explorer.
/// </summary>
public class ShowConsoleXml : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return _schema.ActiveSchemaItem != null; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        foreach (string file in _schema.ActiveSchemaItem.Files)
        {
            string filePath = Path.Combine(path1: settings.ModelSourceControlLocation, path2: file);
            if (File.Exists(path: filePath))
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true,
                    NewLineOnAttributes = true,
                };
                XmlDocument xml = new XmlDocument();
                XmlViewer viewer = new XmlViewer
                {
                    Text = file.Replace(oldValue: "\\", newValue: "/")
                        .Split(separator: '/')
                        .LastOrDefault(),
                };
                try
                {
                    xml.Load(filename: filePath);
                    viewer.Content = xml.ToBeautifulString(xmlWriterSettings: xmlWriterSettings);
                }
                catch
                {
                    viewer.Content = new StreamReader(path: filePath).ReadToEnd();
                }
                WorkbenchSingleton.Workbench.ShowView(content: viewer);
            }
        }
    }

    public override void Dispose()
    {
        _schema = null;
        base.Dispose();
    }
}

/// <summary>
/// Show file in directory in explorer.
/// </summary>
public class ShowFileDiffXml : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;

    public override bool IsEnabled
    {
        get { return _schema.ActiveSchemaItem != null; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        string activefile = Path.Combine(
            path1: settings.ModelSourceControlLocation,
            path2: _schema.ActiveSchemaItem.RootItem.RelativeFilePath
        );
        var provider = (FilePersistenceProvider)_schema.ActiveSchemaItem.PersistenceProvider;
        bool hasChange = false;
        if (provider != null)
        {
            GitManager gitManager = new GitManager(path: settings.ModelSourceControlLocation);
            foreach (string file in _schema.ActiveSchemaItem.Files)
            {
                string fileName = Path.Combine(
                    path1: settings.ModelSourceControlLocation,
                    path2: file
                );
                if (File.Exists(path: fileName))
                {
                    gitManager.SetFile(file: fileName);
                    Commit lastCommit = gitManager.GetLastCommit();
                    string text = gitManager.GetModifiedChanges();
                    if (!string.IsNullOrEmpty(value: text))
                    {
                        GitDiferenceView gitDiferenceView = new GitDiferenceView
                        {
                            Text = gitManager.getCompareFileName(),
                        };
                        text = Regex.Replace(
                            input: text,
                            pattern: @"^.*\ No newline at end of file.*\n",
                            replacement: "",
                            options: RegexOptions.Multiline
                        );
                        gitDiferenceView.ShowDiff(
                            oldfile: fileName + " " + lastCommit.Sha,
                            newfile: fileName,
                            text: text
                        );
                        WorkbenchSingleton.Workbench.ShowView(content: gitDiferenceView);
                        hasChange = true;
                    }
                }
            }
        }
        if (!hasChange)
        {
            MessageBox.Show(
                text: "Found no changes in " + activefile,
                caption: "Git Diff",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Information
            );
        }
    }

    public override void Dispose()
    {
        _schema = null;
        base.Dispose();
    }
}
