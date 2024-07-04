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
using System.Data;
using System.Linq;
using System.Text;
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
using Origam.Workbench;
using Origam.Workbench.Services;

namespace Origam.Gui;
public static class FormTools
{
    public static bool IsFormMenuReadOnly(FormReferenceMenuItem formRef)
    {
        bool result = formRef.ReadOnlyAccess;
        if (!result)
        {
            string authContext = SecurityManager
                .GetReadOnlyRoles(formRef.AuthorizationContext);
            result = SecurityManager
                .GetAuthorizationProvider()
                .Authorize(SecurityManager.CurrentPrincipal, authContext);
        }
        return result;
    }        
    
    public static bool IsFormMenuInitialScreen(AbstractMenuItem menuItem)
    {
        if (menuItem.AuthorizationContext == "*")
        {
            return false;
        }
        string authContext = SecurityManager
            .GetInitialScreenRoles(menuItem.AuthorizationContext);
        return SecurityManager
            .GetAuthorizationProvider()
            .Authorize(SecurityManager.CurrentPrincipal, authContext);
    }
    public static ControlSetItem GetItemFromControlSet(AbstractControlSet controlSet)
    {
        var children = controlSet.Alternatives
            .Cast<ControlSetItem>()
            .ToList();
        children.Sort(new AlternativeControlSetItemComparer());
        foreach (ControlSetItem item in children)
        {
            if (IsValid(item.Features, item.Roles))
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
        if ((WorkbenchSingleton.Workbench != null) 
        && WorkbenchSingleton.Workbench.ApplicationDataDisconnectedMode)
        {
            return true;
        }
        IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
        if (!parameterService.IsFeatureOn(features)) return false;
        if (roles != null && roles != String.Empty)
        {
            if (!SecurityManager.GetAuthorizationProvider().Authorize(SecurityManager.CurrentPrincipal, roles))
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
                string authContext = SecurityManager.GetReadOnlyRoles(cntrlSet.Roles);
                return SecurityManager.GetAuthorizationProvider().Authorize(SecurityManager.CurrentPrincipal, authContext);
            }
        }
        return currentReadOnlyStatus;
    }
    public static string FindTableByDataMember(DataSet ds, string member)
    {
        if (member == null) return "";
        if (ds == null) return "";
        string tableName = "";
        if (member.IndexOf(".") > 0)
        {
            string[] path = member.Split(".".ToCharArray());
            DataTable table = ds.Tables[path[0]];
            if (table == null)
            {
                throw new Exception(ResourceUtils.GetString("ErrorGenerateForm", path[0]));
            }
            for (int i = 1; i < path.Length; i++)
            {
                if (table.ChildRelations.Count > 0 &&
                    table.ChildRelations[path[i]] != null &&
                    table.ChildRelations[path[i]].ChildTable != null)
                {
                    table = table.ChildRelations[path[i]].ChildTable;
                }
                else
                {
                    // if editing screen sections the last part of the member is
                    // the column name so we try to find it in the last table
                    if (!table.Columns.Contains(path[i]))
                    {
                        throw (new ArgumentOutOfRangeException("DataMember", String.Format("Could not find entity `{0}' in data structure id `{1}'.", path[i], ds.ExtendedProperties["Id"])));
                    }
                }
            }
            tableName = table.TableName;
        }
        else
            tableName = member;
        return tableName;
    }
    public static DataSet GetSelectionDialogData(Guid entityId, Guid transformationBeforeId, bool createEmptyRow, object profileId)
    {
        return GetSelectionDialogData(entityId, transformationBeforeId, createEmptyRow, profileId, new Hashtable());
    }
    public static DataSet GetSelectionDialogData(Guid entityId, Guid transformationBeforeId, bool createEmptyRow, object profileId, Hashtable parameters)
    {
        IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        IDataEntity entity = persistence.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(entityId)) as IDataEntity;
        DatasetGenerator gen = new DatasetGenerator(true);
        DataSet sdData = gen.CreateDataSet(entity);
        sdData.RemoveNullConstraints();
        if (transformationBeforeId != Guid.Empty)
        {
            // we have to clone the dataset, because we need to return DataSet without XmlDataDocument bound to it
            IDataDocument dataDoc = DataDocumentFactory.New(DatasetTools.CloneDataSet(sdData));
            IDataDocument inputDoc = DataDocumentFactory.New(new DataSet("ROOT"));
            IServiceAgent transformer = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataTransformationService", RuleEngine.Create(new System.Collections.Hashtable(), null), null);
            transformer.MethodName = "Transform";
            transformer.Parameters.Add("XslScript", transformationBeforeId);
            transformer.Parameters.Add("Data", inputDoc);
            transformer.Parameters.Add("Parameters", parameters);
            transformer.Run();
            IXmlContainer transformationResult = transformer.Result as IXmlContainer;
            if (transformationResult != null)
            {
                dataDoc.Load(new XmlNodeReader(transformationResult.Xml));
            }
            sdData.Merge(dataDoc.DataSet);
        }
        else if (createEmptyRow)
        {
            DataRow row = DatasetTools.CreateRow(null, sdData.Tables[0], null, profileId);
            sdData.Tables[0].Rows.Add(row);
        }
        return sdData;
    }
    
    public static DataRow GetSelectionDialogResultRow(Guid entityId, Guid transformationAfterId, IDataDocument dataDoc, object profileId)
    {
        IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        IDataEntity entity = persistence.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(entityId)) as IDataEntity;
        // TRANSFORMATION - AFTER
        if (transformationAfterId != Guid.Empty)
        {
            IServiceAgent transformer = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataTransformationService", RuleEngine.Create(new System.Collections.Hashtable(), null), null);
            transformer.MethodName = "Transform";
            transformer.Parameters.Add("XslScript", transformationAfterId);
            transformer.Parameters.Add("Data", dataDoc);
            transformer.Run();
            XmlContainer transformationResult = transformer.Result as XmlContainer;
            if (transformationResult != null)
            {
                DatasetGenerator gen = new DatasetGenerator(true);
                DataSet resultData = gen.CreateDataSet(entity);
                resultData.EnforceConstraints = false;
                IDataDocument resultDoc = DataDocumentFactory.New(resultData);
                resultDoc.Load(new XmlNodeReader(transformationResult.Xml));
                DatasetTools.MergeDataSet(
                    dataDoc.DataSet, resultDoc.DataSet, null,
                    new MergeParams(profileId));
            }
        }
        return dataDoc.DataSet.Tables[0].Rows[0];
    }
}
