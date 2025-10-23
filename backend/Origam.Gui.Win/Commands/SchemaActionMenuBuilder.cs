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
        ServiceManager.Services.GetService(typeof(WorkbenchSchemaService))
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
        CreateMenuItem(list, "Show SQL", new ShowDataStructureFilterSetSql(), null);
        CreateMenuItem(list, "Show SQL", new ShowDataStructureSortSetSql(), null);
        CreateMenuItem(list, "Show SQL", new ShowDataStructureEntitySql(), null);
        CreateMenuItem(
            list,
            "Generate Entity Fields",
            new GenerateDataStructureEntityColumns(),
            null
        );
        CreateMenuItem(list, "Save Data to File...", new SaveDataFromDataStructure(), null);
        CreateMenuItem(list, "Create Lookup...", new CreateLookupFromEntityCommand(), null);
        CreateMenuItem(
            list,
            "Create Lookup Field (incl. entity)...",
            new CreateFieldWithLookupEntityCommand(),
            null
        );
        CreateMenuItem(list, "Create Screen...", new CreateFormFromEntityCommand(), null);
        CreateMenuItem(list, "Create Screen Section...", new CreatePanelFromEntityCommand(), null);
        CreateMenuItem(
            list,
            "Create Relationship with key...",
            new CreateFieldWithRelationshipEntityCommand(),
            null
        );
        CreateMenuItem(list, "Create Menu Item...", new CreateCompleteUICommand(), null);
        CreateMenuItem(list, "Create Screen", new CreateFormFromPanelCommand(), null);
        CreateMenuItem(list, "Create Menu Item...", new CreateMenuFromFormCommand(), null);
        CreateMenuItem(list, "Create Menu Item...", new CreateMenuFromDataConstantCommand(), null);
        CreateMenuItem(
            list,
            "Create Menu Item...",
            new CreateMenuFromSequentialWorkflowCommand(),
            null
        );
        CreateMenuItem(
            list,
            "Create Data Structure",
            new CreateDataStructureFromEntityCommand(),
            null
        );
        CreateMenuItem(list, "Create Child Entity...", new CreateChildEntityCommand(), null);
        CreateMenuItem(
            list,
            "Create Localization Child Entity...",
            new CreateLanguageTranslationEntityCommand(),
            null
        );
        CreateMenuItem(list, "Localize", new LocalizeDatastructureCommand(), null);
        CreateMenuItem(list, "Make Version Current", new MakeActiveVersionCurrent(), null);
        CreateMenuItem(list, "Create (=) Filter", new CreateFilterByFieldCommand(), null);
        CreateMenuItem(
            list,
            "Create (=) Filter With Parameter",
            new CreateFilterWithParameterByFieldCommand(),
            null
        );
        CreateMenuItem(list, "Create (Like) Filter", new CreateFilterLikeByFieldCommand(), null);
        CreateMenuItem(
            list,
            "Create (Like) Filter With Parameter",
            new CreateFilterLikeWithParameterByFieldCommand(),
            null
        );
        CreateMenuItem(
            list,
            "Create (List) Filter With Parameter",
            new CreateFilterByListWithParameterByFieldCommand(),
            null
        );
        CreateMenuItem(
            list,
            "Create (Between) Filter With Parameters",
            new CreateFilterBetweenWithParameterByFieldCommand(),
            null
        );
        CreateMenuItem(list, "Create Foreign Key...", new CreateForeignKeyCommand(), null);
        CreateMenuItem(
            list,
            "Create Work Queue Class...",
            new CreateWorkQueueClassFromEntityCommand(),
            null
        );
        CreateMenuItem(list, "Create Role", new CreateRoleCommand(), null);
        CreateMenuItem(list, "Set Inheritance Off", new SetInheritanceOff(), null);
        //			CreateMenuItem(list, "Generate Default Folders", new Origam.Schema.Wizards.CreatePackageFolders(), null);
        CreateMenuItem(list, "Show XML", new ShowModelElementXmlCommand(), null);
        //			CreateMenuItem(list, "Generate XAML...", new Commands.GenerateXamlCommand(), null);
        CreateMenuItem(
            list,
            "Generate Mappings...",
            new GenerateWorkQueueClassEntityMappings(),
            null
        );
        //			CreateMenuItem(list, "Show Editor XML", new ShowEditorXml(), null);
        CreateMenuItem(list, "New LoadData Task", new CreateLoadDataCommand(), null);
        CreateMenuItem(list, "New StoreData Task", new CreateStoreDataCommand(), null);
        CreateMenuItem(list, "New Transform Task", new CreateTransformDataCommand(), null);
        CreateMenuItem(
            list,
            "Load/Transform/Save Tasks",
            new CreateLoadTransformSaveCommand(),
            null
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
                    WorkbenchSingleton.Workbench.GetPad(typeof(FindSchemaItemResultsPad))
                    as FindSchemaItemResultsPad;
                findResults.DisplayResults(cmd.Command.GeneratedModelElements.ToArray());
                MessageBox.Show(
                    strings.ModelElementsUpdate_Message,
                    strings.Results_Title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
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
                    activities.Add(activity);
                }
            }
            if (
                activities.Count > 0
                && MessageBox.Show(
                    strings.ExecuteDeploymentScripts_Question,
                    strings.DeploymentScripts_Title,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                ) == DialogResult.Yes
            )
            {
                IDeploymentService deploymentService =
                    ServiceManager.Services.GetService(typeof(IDeploymentService))
                    as IDeploymentService;
                foreach (var activity in activities)
                {
                    deploymentService.ExecuteActivity(activity.PrimaryKey);
                }
            }
            persistenceProvider.EndTransaction();
        }
        catch (Exception ex)
        {
            persistenceProvider.EndTransactionDontSave();
            AsMessageBox.ShowError(
                WorkbenchSingleton.Workbench as Form,
                ex.Message,
                strings.GenericError_Title,
                ex
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
        AsMenuCommand menuItem = new AsMenuCommand(text, command);
        command.Owner = _owner;

        if (menuItem.IsEnabled)
        {
            menuItem.Click += new EventHandler(MenuItemClick);

            if (image != null)
            {
                menuItem.Image = image;
            }
            list.Add(menuItem);
        }
        else
        {
            ((IDisposable)menuItem).Dispose();
        }
    }
}
