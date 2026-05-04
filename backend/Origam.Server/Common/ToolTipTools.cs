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
using System.Data;
using Origam.Server.Common;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public static class ToolTipTools
{
    public static HelpTooltip NextTooltip()
    {
        return NextTooltip(formId: Guid.Empty.ToString());
    }

    public static HelpTooltip NextTooltip(string formId)
    {
        UserProfile profile = SecurityTools.CurrentUserProfile();
        // get list of all unused tooltips
        DataSet list = CoreServices.DataService.Instance.LoadData(
            dataStructureId: new Guid(g: "80529593-5e54-4d16-b7a8-be400aaaa41b"),
            methodId: new Guid(g: "4b9c5b97-f81f-4859-b2ac-3067da68e47a"),
            defaultSetId: Guid.Empty,
            sortSetId: new Guid(g: "e2446ba7-ae8f-4b87-a728-f461b093614f"),
            transactionId: null,
            paramName1: "OrigamTooltipHelpUsage_parBusinessPartnerId",
            paramValue1: profile.Id,
            paramName2: "OrigamTooltipHelp_parFormId",
            paramValue2: formId
        );
        IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
        Guid tooltipId = Guid.Empty;
        // check first possible tooltip available by role
        foreach (DataRow listRow in list.Tables[index: 0].Rows)
        {
            if (!listRow.IsNull(columnName: "Roles"))
            {
                if (
                    auth.Authorize(
                        principal: SecurityManager.CurrentPrincipal,
                        context: (string)listRow[columnName: "Roles"]
                    )
                )
                {
                    tooltipId = (Guid)listRow[columnName: "Id"];
                    break;
                }
            }
        }
        if (tooltipId == Guid.Empty)
        {
            return null;
        }

        DataSet data = CoreServices.DataService.Instance.LoadData(
            dataStructureId: new Guid(g: "e341c510-d6c4-4bf5-b59a-a349e8984162"),
            methodId: new Guid(g: "7eaf8cd8-e6a5-418d-b4e3-c7549e8080b4"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            paramName1: "OrigamTooltipHelp_parId",
            paramValue1: tooltipId
        );
        DataRow row = data.Tables[index: 0].Rows[index: 0];
        HelpTooltip tt = new HelpTooltip();
        tt.Id = tooltipId.ToString();
        tt.Context = (string)row[columnName: "Context"];
        if (!row.IsNull(columnName: "ObjectId"))
        {
            tt.ObjectId = row[columnName: "ObjectId"].ToString();
        }
        tt.SubContext = (string)row[columnName: "SubContext"];
        tt.RelatedComponent = (string)row[columnName: "RelatedComponent"];
        tt.Text = ((string)row[columnName: "Text"]).Replace(
            oldValue: "COLOR=\"#000000\"",
            newValue: ""
        );
        if (!row.IsNull(columnName: "DestroyParameter"))
        {
            tt.DestroyParameter = (string)row[columnName: "DestroyParameter"];
        }
        switch (row[columnName: "refOrigamTooltipHelpPositionId"].ToString())
        {
            case "41fa1c4f-9a12-4402-aa21-a921b0ceb52e": // left
            {
                tt.Position = 0;
                break;
            }

            case "c6f27d77-ab60-421c-b110-a2429a3e1c8b": // right
            {
                tt.Position = 1;
                break;
            }

            case "bdf7ca80-b110-4e16-932b-684f34c4fed3": // top
            {
                tt.Position = 2;
                break;
            }

            case "da40379a-ae75-4982-888c-17205b64bf4d": // bottom
            {
                tt.Position = 3;
                break;
            }
        }
        switch (row[columnName: "refOrigamTooltipHelpDestroyEventId"].ToString())
        {
            case "59d08fb8-5f49-4a44-b9d9-0ef79aca413a": // click
            {
                tt.DestroyCondition = 0;
                break;
            }

            case "f49d9d54-2f94-49b4-837a-735bf17c8b6e": // selected_item
            {
                tt.DestroyCondition = 1;
                break;
            }

            case "1341a433-94cd-4ae0-8e3f-f4949b80a2da": // open_form
            {
                tt.DestroyCondition = 2;
                break;
            }

            case "3e6c58c8-df5d-4347-ab9e-4d425423ed26": // focus_in
            {
                tt.DestroyCondition = 3;
                break;
            }

            case "3b701836-b4c0-4a0a-ac69-cc40e6057655": // focus_out
            {
                tt.DestroyCondition = 4;
                break;
            }
        }

        return tt;
    }
}
