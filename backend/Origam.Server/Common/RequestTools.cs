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
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Server;

namespace Origam.Server;
internal static class RequestTools
{
    internal static UIRequest GetActionRequest(Hashtable parameters, IList selectedItems, EntityUIAction action)
    {
        UIRequest uir = GetActionRequestBase(parameters, action.Caption);
        Graphics menuIcon = null;
        
        EntityWorkflowAction ewa = action as EntityWorkflowAction;
        EntityMenuAction ema = action as EntityMenuAction;
        EntityReportAction era = action as EntityReportAction;
        WorkQueueWorkflowCommand wqwc = action as WorkQueueWorkflowCommand;
        if (wqwc != null)
        {
            uir = GetWorkflowActionRequest(parameters, wqwc.WorkflowId, action.Caption);
        }
        else if (ewa != null)
        {
            uir = GetWorkflowActionRequest(parameters, ewa.WorkflowId, action.Caption);
        }
        else if (era != null)
        {
            uir = GetReportActionRequest(parameters, era.ReportId, action.Caption);
        }
        else if (ema != null)
        {
            uir.Caption = ema.Menu.DisplayName;
            uir.ObjectId = ema.MenuId.ToString();
            menuIcon = ema.Menu.MenuIcon;
            FormReferenceMenuItem frmi = ema.Menu as FormReferenceMenuItem;
            WorkflowReferenceMenuItem wrmi = ema.Menu as WorkflowReferenceMenuItem;
            ReportReferenceMenuItem rrmi = ema.Menu as ReportReferenceMenuItem;
            PanelControlSet dialogPanel = null;
            if (frmi != null)
            {
                uir.DataRequested = !frmi.IsLazyLoaded;
                if (selectedItems.Count > 1)
                {
                    throw new Exception(Resources.ErrorOpenFormMultipleRecords);
                }
                if (frmi.SelectionDialogPanel == null)
                {
                    uir.Type = UIRequestType.FormReferenceMenuItem;
                }
                else
                {
                    uir.Type = UIRequestType.FormReferenceMenuItem_WithSelection;
                    dialogPanel = frmi.SelectionDialogPanel;
                }
            }
            else if (wrmi != null)
            {
                uir.Type = UIRequestType.WorkflowReferenceMenuItem;
                // TODO naplnit do parametr� seznam ID
            }
            else if (rrmi != null)
            {
                if (selectedItems.Count > 1)
                {
                    throw new Exception(Resources.ErrorReportOpenFormMultipleRecords);
                }
                if (rrmi.SelectionDialogPanel == null)
                {
                    uir.Type = UIRequestType.ReportReferenceMenuItem;
                }
                else
                {
                    uir.Type = UIRequestType.ReportReferenceMenuItem_WithSelection;
                    dialogPanel = rrmi.SelectionDialogPanel;
                }
                if (dialogPanel != null)
                {
                    int height; int width;
                    MenuXmlBuilder.GetSelectionDialogSize(
                        dialogPanel, out width, out height);
                    uir.DialogHeight = height;
                    uir.DialogWidth = width;
                }
            }
            else
            {
                throw new Exception(Resources.ErrorUnsupportedMenuItemType);
            }
        }
        uir.Icon = MenuXmlBuilder.ResolveMenuIcon(uir.Type.ToString(), menuIcon);
        return uir;
    }
    
    private static UIRequest GetActionRequestBase(
        Hashtable parameters, string caption)
    {
        return new UIRequest
        {
            IsDataOnly = false,
            IsStandalone = false,
            Parameters = parameters,
            Caption = caption,
        };
    }
    private static UIRequest GetWorkflowActionRequest(Hashtable parameters, Guid workflowId, string caption)
    {
        UIRequest uir = GetActionRequestBase(parameters, caption);
        uir.Type = UIRequestType.WorkflowReferenceMenuItem;
        uir.ObjectId = workflowId.ToString();
        return uir;
    }
    private static UIRequest GetReportActionRequest(Hashtable parameters, Guid reportId, string caption)
    {
        UIRequest uir = GetActionRequestBase(parameters, caption);
        uir.Type = UIRequestType.ReportReferenceMenuItem;
        uir.ObjectId = reportId.ToString();
        return uir;
    }
    
}
