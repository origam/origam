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
using System.Text;
using System.Windows.Forms;
using MoreLinq;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Services;
using Origam.UI.Commands;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.UI;

/// <summary>
/// Summary description for AbstractMenuCommand.
/// </summary>
public abstract class AbstractMenuCommand : AbstractCommand, IMenuCommand, IRunCommand
{
    public virtual bool IsEnabled { get; set; } = true;

    #region Property
    public ServiceCommandUpdateScriptActivity CreateTableScript(string name, Guid guid)
    {
        AbstractSqlDataService abstractSqlData = (AbstractSqlDataService)
            DataServiceFactory.GetDataService();
        string script = abstractSqlData.EntityDdl(entityId: guid);
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        settings.DeployPlatforms?.ForEach(action: platform =>
        {
            AbstractSqlDataService DsPlatform = (AbstractSqlDataService)
                DataServiceFactory.GetDataService(deployPlatform: platform);
            string platformscript = DsPlatform.EntityDdl(entityId: guid);
            ServiceCommandUpdateScriptActivity _create = DeploymentHelper.CreateDatabaseScript(
                name: name,
                script: platformscript,
                platformName: DsPlatform.PlatformName
            );
        });
        return DeploymentHelper.CreateDatabaseScript(
            name: name,
            script: script,
            platformName: abstractSqlData.PlatformName
        );
    }

    public ServiceCommandUpdateScriptActivity CreateDatabaseScript(
        string name,
        IDictionary<AbstractSqlDataService, StringBuilder> dict
    )
    {
        ServiceCommandUpdateScriptActivity create = null;
        for (int index = 0; index < dict.Count; index++)
        {
            var item = dict.ElementAt(index: index);
            create = DeploymentHelper.CreateDatabaseScript(
                name: name,
                script: ((StringBuilder)item.Value).ToString(),
                platformName: ((AbstractSqlDataService)item.Key).PlatformName
            );
        }
        return create;
    }

    public IDictionary<AbstractSqlDataService, StringBuilder> InitDictionary()
    {
        IDictionary<AbstractSqlDataService, StringBuilder> dict =
            new Dictionary<AbstractSqlDataService, StringBuilder>();
        AbstractSqlDataService abstractSqlData = (AbstractSqlDataService)
            DataServiceFactory.GetDataService();
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        settings.DeployPlatforms?.ForEach(action: platform =>
        {
            AbstractSqlDataService DsPlatform = (AbstractSqlDataService)
                DataServiceFactory.GetDataService(deployPlatform: platform);
            dict.Add(key: DsPlatform, value: new StringBuilder());
        });
        dict.Add(key: abstractSqlData, value: new StringBuilder());
        return dict;
    }

    public void FieldsScripts(
        FieldMappingItem fk,
        FieldMappingItem baseField,
        IDataEntity baseEntity
    )
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        string[] fkDdl = DataService.Instance.FieldDdl(fieldId: fk.Id);
        int i = 0;
        foreach (string ddl in fkDdl)
        {
            // if the foreign key is based on an existing field
            // take only the foreign key ddl
            if (baseField == null || i == 1)
            {
                var script3 = DeploymentHelper.CreateDatabaseScript(
                    name: baseEntity.Name + "_" + fk.Name,
                    script: ddl,
                    platformName: (
                        (AbstractSqlDataService)DataServiceFactory.GetDataService()
                    ).PlatformName
                );
                GeneratedModelElements.Add(item: script3);
            }
            i++;
        }
        settings.DeployPlatforms?.ForEach(action: platform =>
        {
            AbstractSqlDataService DsPlatform = (AbstractSqlDataService)
                DataServiceFactory.GetDataService(deployPlatform: platform);
            fkDdl = DsPlatform.FieldDdl(fieldId: fk.Id);
            i = 0;
            foreach (string ddl in fkDdl)
            {
                // if the foreign key is based on an existing field
                // take only the foreign key ddl
                if (baseField == null || i == 1)
                {
                    var script3 = DeploymentHelper.CreateDatabaseScript(
                        name: baseEntity.Name + "_" + fk.Name,
                        script: ddl,
                        platformName: DsPlatform.PlatformName
                    );
                }
                i++;
            }
        });
    }

    public static List<string> GetListDatastructure(string itemTypeConst)
    {
        ISchemaService schema =
            ServiceManager.Services.GetService(serviceType: typeof(ISchemaService))
            as ISchemaService;
        DataStructureSchemaItemProvider dsprovider =
            schema.GetProvider(type: typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        return dsprovider
            .ChildItemsByType<ISchemaItem>(itemType: itemTypeConst)
            .Select(selector: x => x.Name)
            .ToList();
    }

    public ServiceCommandUpdateScriptActivity CreateRole(string role)
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        AbstractSqlDataService abstractSqlData = (AbstractSqlDataService)
            DataServiceFactory.GetDataService();
        settings.DeployPlatforms?.ForEach(action: platform =>
        {
            AbstractSqlDataService DsPlatform = (AbstractSqlDataService)
                DataServiceFactory.GetDataService(deployPlatform: platform);
            ServiceCommandUpdateScriptActivity _create = DeploymentHelper.CreateSystemRole(
                roleName: role,
                abstractSqlData: DsPlatform
            );
        });
        return DeploymentHelper.CreateSystemRole(roleName: role, abstractSqlData: abstractSqlData);
    }
    #endregion
    #region IDisposable Members
    public virtual void Dispose() { }

    public virtual void Execute() { }

    public virtual int GetImageIndex(string icon)
    {
        return 0;
    }

    public virtual void SetSummaryText(object summary)
    {
        RichTextBox box = (RichTextBox)summary;
        box.Text = "Not implemented";
    }

    public static void ShowListItems(RichTextBox richTextBoxSummary, Hashtable selectedFieldNames)
    {
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "List of fields: \t\t");
        foreach (DictionaryEntry row in selectedFieldNames)
        {
            richTextBoxSummary.AppendText(text: row.Key.ToString());
            richTextBoxSummary.AppendText(text: Environment.NewLine);
            richTextBoxSummary.AppendText(text: "\t\t\t");
        }
    }
    #endregion
}
