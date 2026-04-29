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
using System.Security.Principal;
using System.Windows.Forms;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Services;
using Origam.UI;

namespace Origam.Workbench.Services;

public class ControlsLookUpService : IControlsLookUpService
{
    private Hashtable _controls = new Hashtable();
    private readonly DataLookupService dataLookupService;

    public ControlsLookUpService()
    {
        this.dataLookupService = ServiceManager.Services.GetService<DataLookupService>();
    }

    public void InitializeService() { }

    public void UnloadService()
    {
        _controls.Clear();
    }

    public void AddLookupControl(ILookupControl lookupControl, Form form, bool showEditCommand)
    {
        System.Diagnostics.Debug.Assert(
            condition: form != null,
            message: ResourceUtils.GetString(key: "ErrorFormLookupControlFail")
        );
        _controls.Add(key: lookupControl, value: form);
        AbstractDataLookup lookup = dataLookupService.GetLookup(lookupId: lookupControl.LookupId);
        if (lookup == null)
        {
            throw new NullReferenceException(
                message: "Lookup not specified in control: " + (lookupControl as Control).Name
            );
        }
        // set the EditMenu visibility, if user can or cannot run the Edit command
        lookupControl.LookupShowEditButton = (
            showEditCommand & HasEditListMenuBinding(lookup: lookup)
        );
        lookupControl.LookupCanEditSourceRecord = HasEditRecordMenuBinding(lookup: lookup);
        lookupControl.LookupListDisplayMember = lookup.ListDisplayMember;
        lookupControl.LookupListValueMember = lookup.ListValueMember;
        lookupControl.LookupListTreeParentMember = lookup.TreeParentMember;
        lookupControl.SuppressEmptyColumns = lookup.SuppressEmptyColumns;
        lookupControl.LookupDisplayTextRequested += lookupControl_LookupDisplayTextRequested;
        lookupControl.LookupShowSourceListRequested += lookupControl_LookupShowSourceListRequested;
        lookupControl.LookupEditSourceRecordRequested +=
            lookupControl_LookupEditSourceRecordRequested;
        lookupControl.LookupListRefreshRequested += lookupControl_LookupListRefreshRequested;
    }

    public void RemoveLookupControl(ILookupControl lookupControl)
    {
        lookupControl.LookupDisplayTextRequested -= lookupControl_LookupDisplayTextRequested;
        lookupControl.LookupShowSourceListRequested -= lookupControl_LookupShowSourceListRequested;
        lookupControl.LookupListRefreshRequested -= lookupControl_LookupListRefreshRequested;
        lookupControl.LookupEditSourceRecordRequested -=
            lookupControl_LookupEditSourceRecordRequested;
        _controls.Remove(key: lookupControl);
    }

    public void RemoveLookupControlsByForm(Form form)
    {
        var controls = _controls.Keys.Cast<Control>().ToList();
        foreach (Control control in controls)
        {
            if (_controls[key: control].Equals(obj: form))
            {
                this.RemoveLookupControl(lookupControl: control as ILookupControl);
            }
        }
    }

    private void lookupControl_LookupShowSourceListRequested(object sender, EventArgs e)
    {
        ILookupControl control = sender as ILookupControl;
        DataServiceDataLookup lookup =
            dataLookupService.GetLookup(lookupId: control.LookupId) as DataServiceDataLookup;
        DataLookupMenuBinding binding = dataLookupService.GetMenuBindingElement(
            lookup: lookup,
            value: null
        );
        if (binding != null)
        {
            OnLookupShowSourceListRequested(menuItem: binding.MenuItem, e: EventArgs.Empty);
        }
    }

    private void lookupControl_LookupEditSourceRecordRequested(object sender, EventArgs e)
    {
        ILookupControl control = sender as ILookupControl;
        DataServiceDataLookup lookup =
            dataLookupService.GetLookup(lookupId: control.LookupId) as DataServiceDataLookup;
        DataLookupMenuBinding binding = dataLookupService.GetMenuBindingElement(
            lookup: lookup,
            value: control.LookupValue
        );
        if (binding != null)
        {
            ParameterizedEventArgs args = new ParameterizedEventArgs();
            foreach (
                var entry in dataLookupService.LinkParameters(
                    linkTarget: binding.MenuItem,
                    value: control.LookupValue
                )
            )
            {
                args.Parameters.Add(key: entry.Key, value: entry.Value);
            }
            if (binding.MenuItem is FormReferenceMenuItem)
            {
                if (control is Control)
                {
                    IOrigamForm form = (control as Control).FindForm() as IOrigamForm;
                    args.SourceForm = form;
                }
            }
            OnLookupEditSourceRecordRequested(menuItem: binding.MenuItem, e: args);
        }
    }

    protected virtual void OnLookupShowSourceListRequested(AbstractMenuItem menuItem, EventArgs e)
    {
        OrigamArchitect.Commands.ExecuteSchemaItem cmd =
            new OrigamArchitect.Commands.ExecuteSchemaItem();
        cmd.Owner = menuItem;
        cmd.Run();
    }

    protected virtual void OnLookupEditSourceRecordRequested(
        AbstractMenuItem menuItem,
        ParameterizedEventArgs e
    )
    {
        ParameterizedEventArgs args = e as ParameterizedEventArgs;
        if (args == null)
        {
            return;
        }

        WorkbenchSingleton.Workbench.ProcessGuiLink(
            sourceForm: args.SourceForm,
            linkTarget: menuItem,
            parameters: args.Parameters
        );
    }

    private bool HasEditListMenuBinding(AbstractDataLookup lookup)
    {
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        IPrincipal principal = SecurityManager.CurrentPrincipal;
        foreach (DataLookupMenuBinding binding in lookup.MenuBindings)
        {
            if (
                binding.SelectionLookup == null
                && dataLookupService.AuthorizeMenuBinding(
                    authorizationProvider: authorizationProvider,
                    principal: principal,
                    binding: binding
                )
            )
            {
                return true;
            }
        }
        return false;
    }

    private bool HasEditRecordMenuBinding(AbstractDataLookup lookup)
    {
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        foreach (DataLookupMenuBinding binding in lookup.MenuBindings)
        {
            if (
                dataLookupService.AuthorizeMenuBinding(
                    authorizationProvider: authorizationProvider,
                    principal: SecurityManager.CurrentPrincipal,
                    binding: binding
                )
            )
            {
                return true;
            }
        }
        return false;
    }

    private void lookupControl_LookupListRefreshRequested(object sender, EventArgs e)
    {
        ILookupControl control = sender as ILookupControl;
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (!settings.UseProgressiveCaching)
        {
            // clear cache
            dataLookupService.RemoveFromCache(id: control.LookupId);
            // refresh current text
            lookupControl_LookupDisplayTextRequested(sender: sender, e: e);
        }
        DataTable lookupTable = dataLookupService.GetList(
            request: ILookupControlToLookupListRequest(control: control)
        );
        DataView view = new DataView(table: lookupTable);
        control.LookupList = view;
    }

    private void lookupControl_LookupDisplayTextRequested(object sender, EventArgs e)
    {
        ILookupControl control = sender as ILookupControl;
        control.LookupDisplayText = dataLookupService
            .GetDisplayText(
                lookupId: control.LookupId,
                lookupValue: control.LookupValue,
                transactionId: null
            )
            .ToString();
    }

    private LookupListRequest ILookupControlToLookupListRequest(ILookupControl control)
    {
        LookupListRequest request = new LookupListRequest();
        request.LookupId = control.LookupId;
        request.FieldName = control.ColumnName;
        request.ParameterMappings = control.ParameterMappingsHashtable;
        request.CurrentRow = control.CurrentRow;
        request.ShowUniqueValues = control.ShowUniqueValues;
        request.SearchText = ""; // control.SearchText.Replace("*", "%");
        return request;
    }
}
