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
using System.Xml;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Gui;

public static class FormTools
{
    public static bool IsFormMenuReadOnly(FormReferenceMenuItem formRef)
    {
        bool result = formRef.ReadOnlyAccess;
        if (!result)
        {
            string authContext = SecurityManager.GetReadOnlyRoles(
                roles: formRef.AuthorizationContext
            );
            result = SecurityManager
                .GetAuthorizationProvider()
                .Authorize(principal: SecurityManager.CurrentPrincipal, context: authContext);
        }
        return result;
    }

    public static bool IsFormMenuInitialScreen(AbstractMenuItem menuItem)
    {
        if (menuItem.AuthorizationContext == "*")
        {
            return false;
        }
        string authContext = SecurityManager.GetInitialScreenRoles(
            roles: menuItem.AuthorizationContext
        );
        return SecurityManager
            .GetAuthorizationProvider()
            .Authorize(principal: SecurityManager.CurrentPrincipal, context: authContext);
    }

    public static ControlSetItem GetItemFromControlSet(AbstractControlSet controlSet)
    {
        var children = controlSet.Alternatives.Cast<ControlSetItem>().ToList();
        children.Sort(comparer: new AlternativeControlSetItemComparer());
        foreach (ControlSetItem item in children)
        {
            if (IsValid(features: item.Features, roles: item.Roles))
            {
                return item;
            }
        }
        return controlSet.MainItem;
    }

    public static bool IsValid(string features, string roles)
    {
        // if we're running architect in desconnected mode, we consider
        // everything valid
        // there's even question, wheter this validation is necessary
        // when designing in architect
#if !NETSTANDARD
        if (
            (WorkbenchSingleton.Workbench != null)
            && WorkbenchSingleton.Workbench.ApplicationDataDisconnectedMode
        )
        {
            return true;
        }
#endif
        IParameterService parameterService =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        if (!parameterService.IsFeatureOn(featureCode: features))
        {
            return false;
        }

        if (roles != null && roles != String.Empty)
        {
            if (
                !SecurityManager
                    .GetAuthorizationProvider()
                    .Authorize(principal: SecurityManager.CurrentPrincipal, context: roles)
            )
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Get read only status.
    /// If parent was read only, it will be re-examined here. That means, that when there is a menu item with ReadOnly
    /// set to "true" and there exist some fields or complete panels/groups/tabs inside that form that have a "Roles" property
    /// set, these might get not-read-only, unless they are also set ReadOnly in the user's security settings.
    /// </summary>
    /// <param name="cntrlSet"></param>
    /// <returns></returns>
    public static bool GetReadOnlyStatus(ControlSetItem cntrlSet, bool currentReadOnlyStatus)
    {
        if (cntrlSet.Roles != "" && cntrlSet.Roles != null)
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            if (settings.ActivateReadOnlyRoles)
            {
                string authContext = SecurityManager.GetReadOnlyRoles(roles: cntrlSet.Roles);
                return SecurityManager
                    .GetAuthorizationProvider()
                    .Authorize(principal: SecurityManager.CurrentPrincipal, context: authContext);
            }
        }
        return currentReadOnlyStatus;
    }

    public static string FindTableByDataMember(DataSet ds, string member)
    {
        if (member == null)
        {
            return "";
        }

        if (ds == null)
        {
            return "";
        }

        string tableName = "";
        if (member.IndexOf(value: ".") > 0)
        {
            string[] path = member.Split(separator: ".".ToCharArray());
            DataTable table = ds.Tables[name: path[0]];
            if (table == null)
            {
                throw new Exception(
                    message: ResourceUtils.GetString(key: "ErrorGenerateForm", args: path[0])
                );
            }
            for (int i = 1; i < path.Length; i++)
            {
                if (
                    table.ChildRelations.Count > 0
                    && table.ChildRelations[name: path[i]] != null
                    && table.ChildRelations[name: path[i]].ChildTable != null
                )
                {
                    table = table.ChildRelations[name: path[i]].ChildTable;
                }
                else
                {
                    // if editing screen sections the last part of the member is
                    // the column name so we try to find it in the last table
                    if (!table.Columns.Contains(name: path[i]))
                    {
                        throw (
                            new ArgumentOutOfRangeException(
                                paramName: "DataMember",
                                message: String.Format(
                                    format: "Could not find entity `{0}' in data structure id `{1}'.",
                                    arg0: path[i],
                                    arg1: ds.ExtendedProperties[key: "Id"]
                                )
                            )
                        );
                    }
                }
            }
            tableName = table.TableName;
        }
        else
        {
            tableName = member;
        }

        return tableName;
    }

    public static DataSet GetSelectionDialogData(
        Guid entityId,
        Guid transformationBeforeId,
        bool createEmptyRow,
        object profileId
    )
    {
        return GetSelectionDialogData(
            entityId: entityId,
            transformationBeforeId: transformationBeforeId,
            createEmptyRow: createEmptyRow,
            profileId: profileId,
            parameters: new Hashtable()
        );
    }

    public static DataSet GetSelectionDialogData(
        Guid entityId,
        Guid transformationBeforeId,
        bool createEmptyRow,
        object profileId,
        Hashtable parameters
    )
    {
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        IDataEntity entity =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(id: entityId)
            ) as IDataEntity;
        DatasetGenerator gen = new DatasetGenerator(userDefinedParameters: true);
        DataSet sdData = gen.CreateDataSet(entity: entity);
        sdData.RemoveNullConstraints();
        if (transformationBeforeId != Guid.Empty)
        {
            // we have to clone the dataset, because we need to return DataSet without XmlDataDocument bound to it
            IDataDocument dataDoc = DataDocumentFactory.New(
                dataSet: DatasetTools.CloneDataSet(dataset: sdData)
            );
            IDataDocument inputDoc = DataDocumentFactory.New(
                dataSet: new DataSet(dataSetName: "ROOT")
            );
            IServiceAgent transformer = (
                ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
                as IBusinessServicesService
            ).GetAgent(
                serviceType: "DataTransformationService",
                ruleEngine: RuleEngine.Create(
                    contextStores: new System.Collections.Hashtable(),
                    transactionId: null
                ),
                workflowEngine: null
            );
            transformer.MethodName = "Transform";
            transformer.Parameters.Add(key: "XslScript", value: transformationBeforeId);
            transformer.Parameters.Add(key: "Data", value: inputDoc);
            transformer.Parameters.Add(key: "Parameters", value: parameters);
            transformer.Run();
            IXmlContainer transformationResult = transformer.Result as IXmlContainer;
            if (transformationResult != null)
            {
                dataDoc.Load(xmlReader: new XmlNodeReader(node: transformationResult.Xml));
            }
            sdData.Merge(dataSet: dataDoc.DataSet);
        }
        else if (createEmptyRow)
        {
            DataRow row = DatasetTools.CreateRow(
                parentRow: null,
                newRowTable: sdData.Tables[index: 0],
                relation: null,
                profileId: profileId
            );
            sdData.Tables[index: 0].Rows.Add(row: row);
        }
        return sdData;
    }

    public static DataRow GetSelectionDialogResultRow(
        Guid entityId,
        Guid transformationAfterId,
        IDataDocument dataDoc,
        object profileId
    )
    {
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        IDataEntity entity =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(id: entityId)
            ) as IDataEntity;
        // TRANSFORMATION - AFTER
        if (transformationAfterId != Guid.Empty)
        {
            IServiceAgent transformer = (
                ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
                as IBusinessServicesService
            ).GetAgent(
                serviceType: "DataTransformationService",
                ruleEngine: RuleEngine.Create(
                    contextStores: new System.Collections.Hashtable(),
                    transactionId: null
                ),
                workflowEngine: null
            );
            transformer.MethodName = "Transform";
            transformer.Parameters.Add(key: "XslScript", value: transformationAfterId);
            transformer.Parameters.Add(key: "Data", value: dataDoc);
            transformer.Run();
            XmlContainer transformationResult = transformer.Result as XmlContainer;
            if (transformationResult != null)
            {
                DatasetGenerator gen = new DatasetGenerator(userDefinedParameters: true);
                DataSet resultData = gen.CreateDataSet(entity: entity);
                resultData.EnforceConstraints = false;
                IDataDocument resultDoc = DataDocumentFactory.New(dataSet: resultData);
                resultDoc.Load(xmlReader: new XmlNodeReader(node: transformationResult.Xml));
                DatasetTools.MergeDataSet(
                    inout_dsTarget: dataDoc.DataSet,
                    in_dsSource: resultDoc.DataSet,
                    changeList: null,
                    mergeParams: new MergeParams(ProfileId: profileId)
                );
            }
        }
        return dataDoc.DataSet.Tables[index: 0].Rows[index: 0];
    }
}
