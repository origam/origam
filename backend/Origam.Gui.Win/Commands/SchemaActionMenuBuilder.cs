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
using Origam.DA.ObjectPersistence;
using Origam.Gui.Win.Wizards;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel.UI.Wizards;
using Origam.Schema.LookupModel.UI.Wizards;
using Origam.Schema.Wizards;
using Origam.Schema.WorkflowModel.UI;
using Origam.Schema.WorkflowModel.UI.Wizards;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Pads;
using Origam.Workbench.Services;

namespace Origam.Gui.Win.Commands;

public class SchemaActionsMenuBuilder : ISubmenuBuilder
{
    WorkbenchSchemaService _schemaService =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    object _owner = null;
    #region ISubmenuBuilder Members
    public bool LateBound
    {
        get { return false; }
    }

    public bool HasItems()
    {
        return true;
    }

    public AsMenuCommand[] BuildSubmenu(object owner)
    {
        _owner = owner ?? _schemaService.ActiveNode;
        var list = new List<AsMenuCommand>();
        //			CreateMenuItem(list, "Generate &Test Documentation", new Commands.GenerateTestDocumentation(), null);
        //			CreateMenuItem(list, "Generate &Use Case Documentation", new Commands.GenerateUseCaseDocumentation(), null);
        CreateMenuItem(
            list: list,
            text: "Show SQL",
            command: new ShowDataStructureSql(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Show SQL",
            command: new ShowDataStructureFilterSetSql(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Show SQL",
            command: new ShowDataStructureSortSetSql(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Show SQL",
            command: new ShowDataStructureEntitySql(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Generate Entity Fields",
            command: new GenerateDataStructureEntityColumns(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Save Data to File...",
            command: new SaveDataFromDataStructure(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Lookup...",
            command: new CreateLookupFromEntityCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Lookup Field (incl. entity)...",
            command: new CreateFieldWithLookupEntityCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Screen...",
            command: new CreateFormFromEntityCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Screen Section...",
            command: new CreatePanelFromEntityCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Relationship with key...",
            command: new CreateFieldWithRelationshipEntityCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Menu Item...",
            command: new CreateCompleteUICommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Screen",
            command: new CreateFormFromPanelCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Menu Item...",
            command: new CreateMenuFromFormCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Menu Item...",
            command: new CreateMenuFromDataConstantCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Menu Item...",
            command: new CreateMenuFromSequentialWorkflowCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Data Structure",
            command: new CreateDataStructureFromEntityCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Child Entity...",
            command: new CreateChildEntityCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Localization Child Entity...",
            command: new CreateLanguageTranslationEntityCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Localize",
            command: new LocalizeDatastructureCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Make Version Current",
            command: new MakeActiveVersionCurrent(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create (=) Filter",
            command: new CreateFilterByFieldCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create (=) Filter With Parameter",
            command: new CreateFilterWithParameterByFieldCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create (Like) Filter",
            command: new CreateFilterLikeByFieldCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create (Like) Filter With Parameter",
            command: new CreateFilterLikeWithParameterByFieldCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create (List) Filter With Parameter",
            command: new CreateFilterByListWithParameterByFieldCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create (Between) Filter With Parameters",
            command: new CreateFilterBetweenWithParameterByFieldCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Foreign Key...",
            command: new CreateForeignKeyCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Work Queue Class...",
            command: new CreateWorkQueueClassFromEntityCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Create Role",
            command: new CreateRoleCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Set Inheritance Off",
            command: new SetInheritanceOff(),
            image: null
        );
        //			CreateMenuItem(list, "Generate Default Folders", new Origam.Schema.Wizards.CreatePackageFolders(), null);
        CreateMenuItem(
            list: list,
            text: "Show XML",
            command: new ShowModelElementXmlCommand(),
            image: null
        );
        //			CreateMenuItem(list, "Generate XAML...", new Commands.GenerateXamlCommand(), null);
        CreateMenuItem(
            list: list,
            text: "Generate Mappings...",
            command: new GenerateWorkQueueClassEntityMappings(),
            image: null
        );
        //			CreateMenuItem(list, "Show Editor XML", new ShowEditorXml(), null);
        CreateMenuItem(
            list: list,
            text: "New LoadData Task",
            command: new CreateLoadDataCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "New StoreData Task",
            command: new CreateStoreDataCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "New Transform Task",
            command: new CreateTransformDataCommand(),
            image: null
        );
        CreateMenuItem(
            list: list,
            text: "Load/Transform/Save Tasks",
            command: new CreateLoadTransformSaveCommand(),
            image: null
        );
        return list.ToArray();
    }
    #endregion
    private void MenuItemClick(object sender, EventArgs e)
    {
        IPersistenceProvider persistenceProvider = ServiceManager
            .Services.GetService<IPersistenceService>()
            .SchemaProvider;
        try
        {
            persistenceProvider.BeginTransaction();
            AsMenuCommand cmd = sender as AsMenuCommand;
            cmd.Command.Run();
            // display newly created elements in the search results
            if (cmd.Command.GeneratedModelElements.Count > 0)
            {
                FindSchemaItemResultsPad findResults =
                    WorkbenchSingleton.Workbench.GetPad(type: typeof(FindSchemaItemResultsPad))
                    as FindSchemaItemResultsPad;
                findResults.DisplayResults(results: cmd.Command.GeneratedModelElements.ToArray());
                MessageBox.Show(
                    text: strings.ModelElementsUpdate_Message,
                    caption: strings.Results_Title,
                    buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Information
                );
            }
            // offer to execute any newly created deployment scripts
            List<AbstractUpdateScriptActivity> activities =
                new List<AbstractUpdateScriptActivity>();
            foreach (var item in cmd.Command.GeneratedModelElements)
            {
                var activity = item as AbstractUpdateScriptActivity;
                if (activity != null)
                {
                    activities.Add(item: activity);
                }
            }
            if (
                activities.Count > 0
                && MessageBox.Show(
                    text: strings.ExecuteDeploymentScripts_Question,
                    caption: strings.DeploymentScripts_Title,
                    buttons: MessageBoxButtons.YesNo,
                    icon: MessageBoxIcon.Question
                ) == DialogResult.Yes
            )
            {
                IDeploymentService deploymentService =
                    ServiceManager.Services.GetService(serviceType: typeof(IDeploymentService))
                    as IDeploymentService;
                foreach (var activity in activities)
                {
                    deploymentService.ExecuteActivity(key: activity.PrimaryKey);
                }
            }
            persistenceProvider.EndTransaction();
        }
        catch (Exception ex)
        {
            persistenceProvider.EndTransactionDontSave();
            AsMessageBox.ShowError(
                owner: WorkbenchSingleton.Workbench as Form,
                text: ex.Message,
                caption: strings.GenericError_Title,
                exception: ex
            );
        }
    }

    private void CreateMenuItem(
        List<AsMenuCommand> list,
        string text,
        ICommand command,
        Image image
    )
    {
        AsMenuCommand menuItem = new AsMenuCommand(label: text, menuCommand: command);
        command.Owner = _owner;

        if (menuItem.IsEnabled)
        {
            menuItem.Click += new EventHandler(MenuItemClick);

            if (image != null)
            {
                menuItem.Image = image;
            }
            list.Add(item: menuItem);
        }
        else
        {
            ((IDisposable)menuItem).Dispose();
        }
    }
}
