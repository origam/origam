#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Origam.Workbench;

namespace Origam.Server
{
    public enum UIMode
    {
        Default = 0,              // voláno standardnì bez parametrù, pøípadnì po selection dialogu s pøedanými parametry nebo z jiného formuláøe pomocí tlaèítka s pøedanými parametry
        SelectionDialog = 1,      // voláno pøed formuláøem obsahujícím selection dialog
        SingleRecordEdit = 2      // voláno pøi kliknutí na link v comboboxu s pøedáním jednoho parametru – hodnoty pole comboboxu
    }

    public interface IUIService
    {
        /// <summary>
        /// Returns menu for the active user.
        /// </summary>
        /// <returns></returns>
        string GetMenu();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuId"></param>
        /// <param name="parameters"></param>
        /// <param name="mode"></param>
        /// <returns>Session Form Identifier</returns>
//        UIResult InitUI(Guid menuId, IDictionary<string, object> parameters, int mode);
        UIResult InitUI(UIRequest request);

        /// <summary>
        /// Called after closing the form.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        void DestroyUI(string sessionFormIdentifier);

        /// <summary>
        /// Called when user presses Save button.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        IList SaveData(string sessionFormIdentifier);

        /// <summary>
        /// Loads detail data for the parent when the form uses delayed data loading.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity">Entity for which we need a returned list.</param>
        /// <param name="parentId">Parent entity's Id for which we are loading the child records.</param>
        /// <param name="rootRecordId">Root entity's id as a context for which the data are being loaded.
        /// If the rootRecordId is not loaded in the context anymore, the request is thrown away (empty list is returned).</param>
        /// <returns>List of child rows from the requested parent record.</returns>
        ArrayList GetData(string sessionFormIdentifier, string childEntity, object parentRecordId, object rootRecordId);

        /// <summary>
        /// Gets a complete copy of the data, without refreshing from the database.
        /// E.g. in case of an exception, client reloads data from the server.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <returns></returns>
        IDictionary<string, object> GetSessionData(string sessionFormIdentifier);

        /// <summary>
        /// Refreshes the data from the database and returns the complete copy of the form's data.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <returns></returns>
        IDictionary<string, object> RefreshData(string sessionFormIdentifier);

        /// <summary>
        /// Gets new row object.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="parameters"></param>
        /// <param name="requestiPoužingGrid"></param>
        /// <returns></returns>
        IList CreateObject(string sessionFormIdentifier, string entity, IDictionary<string, object> parameters, string requestingGrid);

        /// <summary>
        /// Passes updated value to the form data.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <param name="property"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        IList UpdateObject(string sessionFormIdentifier, string entity, object id, string property, object newValue);

        /// <summary>
        /// Deletes the object in the form data.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        IList DeleteObject(string sessionFormIdentifier, string entity, object id);

        /// <summary>
        /// Gets the list for the lookup control (e.g. combo-box).
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <param name="lookupId"></param>
        /// <param name="parameters"></param>
        /// <param name="showUniqueValues"></param>
        /// <returns></returns>
        IDictionary<string, ArrayList> GetLookupListEx(string sessionFormIdentifier, string entity, string property, object id, string lookupId, IDictionary<string, object> parameters, bool showUniqueValues);

        /// <summary>
        /// Gets the label for the lookup control (e.g. combo-box).
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="lookupId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        IList GetLookupLabels(string lookupId, object[] ids);

        /// <summary>
        /// Returns menu Id for the given value (in case that clicking on the lookup's link could open different menu depending on
        /// the displayed value). E.g. clicking on the invoice could open either sales or purchase invoice form.
        /// </summary>
        /// <param name="lookupId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IMenuBindingResult GetLookupEditMenu(string lookupId, object value);

        /// <summary>
        /// Stores the new column configuration to the database.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="formPanelId"></param>
        /// <param name="columnConfigurations"></param>
        void SaveColumnConfig(string formPanelId, IList columnConfigurations);

        /// <summary>
        /// Stores the split panel configuration to the database
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="position"></param>
        void SaveSplitPanelConfig(string instanceId, int position);

        /// <summary>
        /// Returns row/field level security and formatting information for the given row.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        IList RowStates(string sessionFormIdentifier, string entity, object[] ids);

        /// <summary>
        /// Saves a new named filter for a given panel.
        /// </summary>
        /// <param name="panelId"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Guid SaveFilter(string sessionFormIdentifier, string entity, string panelId, UIGridFilterConfiguration filter);

        /// <summary>
        /// Deletes a named filter.
        /// </summary>
        /// <param name="filterId"></param>
        void DeleteFilter(string filterId);

        /// <summary>
        /// Sets default filter for a given panel instance.
        /// </summary>
        /// <param name="panelInstanceId"></param>
        /// <param name="panelId"></param>
        /// <param name="filter"></param>
        void SetDefaultFilter(string sessionFormIdentifier, string entity, string panelInstanceId, string panelId, UIGridFilterConfiguration filter);

        /// <summary>
        /// Resests default filter for a given panel instance (there will be no filter).
        /// </summary>
        /// <param name="panelId"></param>
        void ResetDefaultFilter(string panelInstanceId, string sessionFormIdentifier);

        /// <summary>
        /// Saves UI panel's state (e.g. grid visibility)
        /// </summary>
        /// <param name="panel"></param>
        void SavePanelState(UIPanel panel, string sessionFormIdentifier);

        /// <summary>
        /// Returns audit data for the row requested.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        IDictionary<string, ArrayList> GetAudit(string sessionId, string entity, object id);

        /// <summary>
        /// Returns menu and open sessions of the current user.
        /// </summary>
        /// <returns></returns>
        PortalResult InitPortal(string locale);

        /// <summary>
        /// Logs out current user from the portal (destroys all sessions etc.)
        /// </summary>
        void Logout();

        /// <summary>
        /// Moves the currently running workflow to the next step.
        /// </summary>
        /// <param name="sessionFormIdentifier"></param>
        /// <returns></returns>
        UIResult WorkflowNext(string sessionFormIdentifier);

        UIResult WorkflowAbort(string sessionFormIdentifier);

        UIResult WorkflowRepeat(string sessionFormIdentifier);
    }
}
