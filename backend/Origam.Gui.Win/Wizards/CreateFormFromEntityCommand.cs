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
using System.Linq;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;

namespace Origam.Gui.Win.Wizards;

/// <summary>
/// Summary description for CreateFormFromEntityCommand.
/// </summary>
public class CreateFormFromEntityCommand : AbstractMenuCommand
{
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    ScreenWizardForm screenwizardForm;
    public override bool IsEnabled
    {
        get { return Owner is IDataEntity; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        List<string> listdsName = GetListDatastructure(itemTypeConst: DataStructure.CategoryConst);
        var list = new List<ListViewItem>();
        DataStructure dd = new DataStructure();
        PanelControlSet pp = new PanelControlSet();
        FormControlSet ff = new FormControlSet();
        list.Add(
            item: new ListViewItem(
                text: dd.GetType().SchemaItemDescription().Name,
                imageKey: dd.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: pp.GetType().SchemaItemDescription().Name,
                imageKey: pp.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: ff.GetType().SchemaItemDescription().Name,
                imageKey: ff.Icon
            )
        );

        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.ScreenForm);
        if (listdsName.Any(predicate: name => name == (Owner as IDataEntity).Name))
        {
            stackPage.Push(obj: PagesList.StructureNamePage);
        }
        stackPage.Push(obj: PagesList.StartPage);
        screenwizardForm = new ScreenWizardForm
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString(key: "ScreenWizardTitle"),

            Description = ResourceUtils.GetString(key: "ScreenWizardDescription"),
            Pages = stackPage,
            Entity = Owner as IDataEntity,
            IsRoleVisible = false,
            textColumnsOnly = false,
            StructureList = listdsName,
            NameOfEntity = (Owner as IDataEntity).Name,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
        };
        Wizard wiz = new Wizard(objectForm: screenwizardForm);
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        string groupName = null;
        if (screenwizardForm.Entity.Group != null)
        {
            groupName = screenwizardForm.Entity.Group.Name;
        }

        DataStructure dataStructure = EntityHelper.CreateDataStructure(
            entity: screenwizardForm.Entity,
            name: screenwizardForm.NameOfEntity,
            persist: true
        );
        GeneratedModelElements.Add(item: dataStructure);
        PanelControlSet panel = GuiHelper.CreatePanel(
            groupName: groupName,
            entity: screenwizardForm.Entity,
            fieldsToPopulate: screenwizardForm.SelectedFieldNames,
            name: screenwizardForm.NameOfEntity
        );
        GeneratedModelElements.Add(item: panel);
        FormControlSet form = GuiHelper.CreateForm(
            dataSource: dataStructure,
            groupName: groupName,
            defaultPanel: panel
        );
        GeneratedModelElements.Add(item: form);
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            "This Wizard will create a Screen from an Entity with these parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Datastructure: \t\t");
        richTextBoxSummary.AppendText(text: screenwizardForm.NameOfEntity);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        ShowListItems(
            richTextBoxSummary: richTextBoxSummary,
            selectedFieldNames: screenwizardForm.SelectedFieldNames
        );
    }
}

public class CreateCompleteUICommand : AbstractMenuCommand
{
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    ScreenWizardForm wizardForm;
    public override bool IsEnabled
    {
        get { return Owner is IDataEntity; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        IDataEntity entity = Owner as IDataEntity;
        List<string> listdsName = GetListDatastructure(itemTypeConst: DataStructure.CategoryConst);
        var list = new List<ListViewItem>();
        DataStructure ds = new DataStructure();
        PanelControlSet panel1 = new PanelControlSet();
        FormControlSet frmSet = new FormControlSet();
        FormReferenceMenuItem form1 = new FormReferenceMenuItem();
        ServiceCommandUpdateScriptActivity activity1 = new ServiceCommandUpdateScriptActivity();
        list.Add(
            item: new ListViewItem(
                text: ds.GetType().SchemaItemDescription().Name,
                imageKey: ds.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: panel1.GetType().SchemaItemDescription().Name,
                imageKey: panel1.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: frmSet.GetType().SchemaItemDescription().Name,
                imageKey: frmSet.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: form1.GetType().SchemaItemDescription().Name,
                imageKey: form1.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: activity1.GetType().SchemaItemDescription().Name,
                imageKey: activity1.Icon
            )
        );
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.ScreenForm);
        if (listdsName.Any(predicate: name => name == (Owner as IDataEntity).Name))
        {
            stackPage.Push(obj: PagesList.StructureNamePage);
        }
        stackPage.Push(obj: PagesList.StartPage);
        wizardForm = new ScreenWizardForm
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString(key: "CreateCompleteUIWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString(key: "CreateCompleteUIWizardDescription"),
            Pages = stackPage,
            Entity = Owner as IDataEntity,
            IsRoleVisible = true,
            textColumnsOnly = false,
            StructureList = listdsName,
            NameOfEntity = (Owner as IDataEntity).Name,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
            Role = entity.Name,
        };
        Wizard wiz = new Wizard(objectForm: wizardForm);
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        string groupName = null;
        if (wizardForm.Entity.Group != null)
        {
            groupName = wizardForm.Entity.Group.Name;
        }

        DataStructure dataStructure = EntityHelper.CreateDataStructure(
            entity: wizardForm.Entity,
            name: wizardForm.NameOfEntity,
            persist: true
        );
        PanelControlSet panel = GuiHelper.CreatePanel(
            groupName: groupName,
            entity: wizardForm.Entity,
            fieldsToPopulate: wizardForm.SelectedFieldNames,
            name: wizardForm.NameOfEntity
        );
        FormControlSet form = GuiHelper.CreateForm(
            dataSource: dataStructure,
            groupName: groupName,
            defaultPanel: panel
        );
        FormReferenceMenuItem menu = MenuHelper.CreateMenuItem(
            caption: !string.IsNullOrEmpty(value: wizardForm.Caption)
                ? wizardForm.Caption
                : wizardForm.Entity.Name,
            role: wizardForm.Role,
            form: form
        );
        GeneratedModelElements.Add(item: dataStructure);
        GeneratedModelElements.Add(item: panel);
        GeneratedModelElements.Add(item: form);
        GeneratedModelElements.Add(item: menu);
        if (wizardForm.Role != "*" && wizardForm.Role != "")
        {
            ServiceCommandUpdateScriptActivity activity = CreateRole(role: wizardForm.Role);
            GeneratedModelElements.Add(item: activity);
        }
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            "This Wizard will create a Menu from an Entity with these parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Datastructure: \t\t");
        richTextBoxSummary.AppendText(text: wizardForm.NameOfEntity);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Menu: \t\t\t");
        richTextBoxSummary.AppendText(
            text: wizardForm.Entity.Caption == null || wizardForm.Entity.Caption == ""
                ? wizardForm.NameOfEntity
                : wizardForm.Entity.Caption
        );
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Role: \t\t\t");
        richTextBoxSummary.AppendText(text: wizardForm.Role);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        ShowListItems(
            richTextBoxSummary: richTextBoxSummary,
            selectedFieldNames: wizardForm.SelectedFieldNames
        );
    }
}

public class CreateFormFromPanelCommand : AbstractMenuCommand
{
    PanelWizardForm panelWizard;
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    public override bool IsEnabled
    {
        get { return Owner is PanelControlSet; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        PanelControlSet panel = Owner as PanelControlSet;
        DataStructure ds = new DataStructure();
        FormControlSet frmSet = new FormControlSet();
        List<string> listdsName = GetListDatastructure(itemTypeConst: DataStructure.CategoryConst);
        var list = new List<ListViewItem>();
        list.Add(
            item: new ListViewItem(
                text: ds.GetType().SchemaItemDescription().Name,
                imageKey: ds.Icon
            )
        );
        list.Add(
            item: new ListViewItem(
                text: frmSet.GetType().SchemaItemDescription().Name,
                imageKey: frmSet.Icon
            )
        );
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        if (listdsName.Any(predicate: name => name == panel.Name))
        {
            stackPage.Push(obj: PagesList.StructureNamePage);
        }
        stackPage.Push(obj: PagesList.StartPage);
        panelWizard = new PanelWizardForm
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString(key: "CreateFormFromPanelWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString(key: "CreateFormFromPanelWizardDescription."),
            StructureList = listdsName,
            NameOfEntity = panel.Name,
            Pages = stackPage,
            Entity = panel,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
        };
        Wizard wiz = new Wizard(objectForm: panelWizard);
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        PanelControlSet panel = ((PanelControlSet)panelWizard.Entity);
        string groupName = null;
        if (panelWizard.Entity.Group != null)
        {
            groupName = panelWizard.Entity.Group.Name;
        }

        DataStructure dataStructure = EntityHelper.CreateDataStructure(
            entity: panel.DataEntity,
            name: panelWizard.NameOfEntity,
            persist: true
        );
        GeneratedModelElements.Add(item: dataStructure);
        FormControlSet form = GuiHelper.CreateForm(
            dataSource: dataStructure,
            groupName: groupName,
            defaultPanel: panel
        );
        GeneratedModelElements.Add(item: form);
        Origam.Workbench.Commands.EditSchemaItem edit =
            new Origam.Workbench.Commands.EditSchemaItem();
        edit.Owner = form;
        edit.Run();
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            "This Wizard will create a Screen from a ScreenSection with these parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Datastructure: \t");
        richTextBoxSummary.AppendText(text: panelWizard.NameOfEntity);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Screen: \t\t");
        richTextBoxSummary.AppendText(text: panelWizard.NameOfEntity);
    }
}

public class CreateMenuFromFormCommand : AbstractMenuCommand
{
    MenuFromForm menuFrom;
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    public override bool IsEnabled
    {
        get { return Owner is FormControlSet; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        FormControlSet form = Owner as FormControlSet;
        var list = new List<ListViewItem>();
        FormReferenceMenuItem form1 = new FormReferenceMenuItem();
        list.Add(
            item: new ListViewItem(
                text: form1.GetType().SchemaItemDescription().Name,
                imageKey: form1.Icon
            )
        );
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.MenuPage);
        stackPage.Push(obj: PagesList.StartPage);
        menuFrom = new MenuFromForm
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString(key: "CreateMenuFromFormWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString(key: "CreateMenuFromFormWizardDescription"),
            Pages = stackPage,
            Entity = form,
            Role = form.Name,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
        };
        Wizard wiz = new Wizard(objectForm: menuFrom);
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        FormReferenceMenuItem menu = MenuHelper.CreateMenuItem(
            caption: !string.IsNullOrEmpty(value: menuFrom.Caption)
                ? menuFrom.Caption
                : menuFrom.Entity.Name,
            role: menuFrom.Role,
            form: (FormControlSet)menuFrom.Entity
        );
        GeneratedModelElements.Add(item: menu);
        bool createRole = menuFrom.Role != "*" && menuFrom.Role != "";
        if (createRole)
        {
            ServiceCommandUpdateScriptActivity activity = CreateRole(role: menuFrom.Role);
            GeneratedModelElements.Add(item: activity);
        }
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            "This Wizard will create a Menu for a Screen with these parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Menu: \t");
        richTextBoxSummary.AppendText(text: menuFrom.Caption);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Role: \t");
        richTextBoxSummary.AppendText(text: menuFrom.Role);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
    }
}

public class CreateMenuFromDataConstantCommand : AbstractMenuCommand
{
    MenuFromForm menuFrom;
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    public override bool IsEnabled
    {
        get { return Owner is DataConstant; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        DataConstant constant = Owner as DataConstant;
        var list = new List<ListViewItem>();
        DataConstantReferenceMenuItem form1 = new DataConstantReferenceMenuItem();
        list.Add(
            item: new ListViewItem(
                text: form1.GetType().SchemaItemDescription().Name,
                imageKey: form1.Icon
            )
        );
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.MenuPage);
        stackPage.Push(obj: PagesList.StartPage);
        menuFrom = new MenuFromForm
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString(key: "CreateMenuFromDataConstantWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString(
                key: "CreateMenuFromDataConstantWizardDescription"
            ),
            Pages = stackPage,
            Entity = constant,
            Role = constant.Name,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
        };
        Wizard wiz = new Wizard(objectForm: menuFrom);
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        DataConstantReferenceMenuItem menu = MenuHelper.CreateMenuItem(
            caption: menuFrom.Caption,
            role: menuFrom.Role,
            constant: menuFrom.Entity as DataConstant
        );
        GeneratedModelElements.Add(item: menu);
        bool createRole = menuFrom.Role != "*" && menuFrom.Role != "";
        if (createRole)
        {
            ServiceCommandUpdateScriptActivity activity = CreateRole(role: menuFrom.Role);
            GeneratedModelElements.Add(item: activity);
        }
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            "This Wizard will create a Menu for a DataConstant with these parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Menu: \t");
        richTextBoxSummary.AppendText(text: menuFrom.Caption);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Role: \t");
        richTextBoxSummary.AppendText(text: menuFrom.Role);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
    }
}

public class CreateMenuFromSequentialWorkflowCommand : AbstractMenuCommand
{
    MenuFromForm menuFrom;
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    public override bool IsEnabled
    {
        get { return Owner is Schema.WorkflowModel.Workflow; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        Schema.WorkflowModel.Workflow wf = Owner as Schema.WorkflowModel.Workflow;
        var list = new List<ListViewItem>();
        WorkflowReferenceMenuItem workflowReference = new WorkflowReferenceMenuItem();
        list.Add(
            item: new ListViewItem(
                text: workflowReference.GetType().SchemaItemDescription().Name,
                imageKey: workflowReference.Icon
            )
        );
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.MenuPage);
        stackPage.Push(obj: PagesList.StartPage);
        menuFrom = new MenuFromForm
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString(key: "CreateMenuFromSequentialWorkflowWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString(
                key: "CreateMenuFromSequentialWorkflowWizardTitle"
            ),
            Pages = stackPage,
            Entity = wf,
            Role = wf.Name,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
        };
        Wizard wiz = new Wizard(objectForm: menuFrom);
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        WorkflowReferenceMenuItem menu = MenuHelper.CreateMenuItem(
            caption: menuFrom.Caption,
            role: menuFrom.Role,
            workflow: menuFrom.Entity as Schema.WorkflowModel.Workflow
        );
        GeneratedModelElements.Add(item: menu);
        bool createRole = menuFrom.Role != "*" && menuFrom.Role != "";
        if (createRole)
        {
            ServiceCommandUpdateScriptActivity activity = CreateRole(role: menuFrom.Role);
            GeneratedModelElements.Add(item: activity);
        }
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            "This Wizard will create a Menu for a Workflow with these parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Menu: \t");
        richTextBoxSummary.AppendText(text: menuFrom.Caption);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Role: \t");
        richTextBoxSummary.AppendText(text: menuFrom.Role);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
    }
}
