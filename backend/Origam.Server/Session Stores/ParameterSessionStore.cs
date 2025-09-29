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

#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Data;
using System.Xml;
using Origam.DA.Service;
using Origam.Gui;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Server;
using Origam.Workbench.Services;

namespace Origam.Server;

class ParameterSessionStore : SessionStore
{
    private Guid _dataStructureId;
    private string _titleName;
    private IDataLookup _lookup;
    private DataConstant _constant;

    public ParameterSessionStore(
        IBasicUIService service,
        UIRequest request,
        DataConstant constant,
        IDataLookup lookup,
        string titleName,
        string name,
        bool refreshPortalAfterSave,
        Analytics analytics
    )
        : base(service, request, name, analytics)
    {
        this.Constant = constant;
        this.Lookup = lookup;
        this.TitleName = titleName;
        this.RefreshPortalAfterSave = refreshPortalAfterSave;
    }

    #region Properties
    public Guid DataStructureId
    {
        get { return _dataStructureId; }
        set { _dataStructureId = value; }
    }
    public IDataLookup Lookup
    {
        get { return _lookup; }
        set { _lookup = value; }
    }
    public string TitleName
    {
        get { return _titleName; }
        set { _titleName = value; }
    }
    public DataConstant Constant
    {
        get { return _constant; }
        set { _constant = value; }
    }
    #endregion
    #region Overriden Session Methods
    public override void Init()
    {
        // resolve the formId for the parameter
        if (Lookup == null)
        {
            switch (this.Constant.DataType)
            {
                case OrigamDataType.Boolean:
                {
                    this.FormId = new Guid("1dd31104-afa7-4309-95b4-f58707c867d3");
                    break;
                }

                case OrigamDataType.Float:
                case OrigamDataType.Currency:
                {
                    this.FormId = new Guid("11c112bc-9d2b-4ae3-8e0b-7c328700844d");
                    break;
                }

                case OrigamDataType.Date:
                {
                    this.FormId = new Guid("fca89d9b-12b3-42fa-98d5-dae57f96eddf");
                    break;
                }

                case OrigamDataType.Integer:
                case OrigamDataType.Long:
                {
                    this.FormId = new Guid("97347fac-9c02-492b-826a-6afaaa2ef7ae");
                    break;
                }

                case OrigamDataType.Memo:
                case OrigamDataType.String:
                case OrigamDataType.UniqueIdentifier:
                {
                    this.FormId = new Guid("d9e147bf-0b27-47c5-a281-284f9369009d");
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(
                        "DataType",
                        this.Constant.DataType,
                        "Unsupported data type for DataConstantReferenceMenuItem."
                    );
            }
        }
        else
        {
            if (this.Constant.DataType == OrigamDataType.UniqueIdentifier)
            {
                this.FormId = new Guid("c5866e5c-1c8d-45ad-9694-c7bebccf2cd9");
            }
            else
            {
                throw new Exception("Parameters with lookup must be UniqueIdentifier data type.");
            }
        }
        // load the form definition
        IPersistenceService ps =
            ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        FormControlSet form =
            ps.SchemaProvider.RetrieveInstance(
                typeof(DataStructure),
                new ModelElementKey(this.FormId)
            ) as FormControlSet;
        this.DataStructureId = form.DataSourceId;
        // prepare the data source
        DataSet data = new DatasetGenerator(true).CreateDataSet(form.DataStructure);
        DataTable t = data.Tables["SD"];
        DataRow r = t.NewRow();
        LoadParameterData(r, this.Constant.Id);
        t.Rows.Add(r);
        this.SetDataSource(data);
        // add SD as dirty-enabled otherwise - since it is virtual - the form's dirty flag
        // would be reset when editing
        this.DirtyEnabledEntities.Add("SD");
    }

    public override object ExecuteActionInternal(string actionId)
    {
        switch (actionId)
        {
            case ACTION_SAVE:
                return SaveParameterData();
            case ACTION_REFRESH:
                return Refresh();
            default:
                throw new ArgumentOutOfRangeException(
                    "actionId",
                    actionId,
                    Resources.ErrorContextUnknownAction
                );
        }
    }

    public override XmlDocument GetFormXml()
    {
        XmlDocument formXml;
        IPersistenceService ps =
            ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        FormControlSet fcs =
            ps.SchemaProvider.RetrieveInstance(
                typeof(FormControlSet),
                new ModelElementKey(this.FormId),
                false
            ) as FormControlSet;
        foreach (ISchemaItem item in fcs.ChildItemsRecursive)
        {
            ControlSetItem panel = item as ControlSetItem;
            if (panel != null && panel.ControlItem.IsComplexType)
            {
                foreach (
                    ISchemaItem panelChild in panel.ControlItem.PanelControlSet.ChildItemsRecursive
                )
                {
                    PropertyValueItem pvi = panelChild as PropertyValueItem;
                    if (pvi != null && pvi.ControlPropertyItem.Name == "LookupId")
                    {
                        pvi.GuidValue = (Guid)Lookup.PrimaryKey["Id"];
                    }
                    //else if (pvi != null && pvi.Value == "$caption")
                    //{
                    //    pvi.Value = this.TitleName;
                    //}
                }
            }
        }
        formXml = Origam
            .OrigamEngine.ModelXmlBuilders.FormXmlBuilder.GetXml(
                fcs,
                this.Data,
                this.TitleName,
                true,
                new Guid(this.Request.ObjectId),
                false,
                ""
            )
            .Document;
        XmlNode node = formXml.SelectSingleNode("//*[@*='$caption']");
        foreach (XmlAttribute att in node.Attributes)
        {
            if (att.Value == "$caption")
            {
                att.Value = this.TitleName;
            }
        }
        return formXml;
    }
    #endregion
    #region Private Methods
    private static void LoadParameterData(DataRow r, Guid parameterId)
    {
        IParameterService paramSvc =
            ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
        object value = paramSvc.GetParameterValue(parameterId);
        r["Id"] = Guid.NewGuid();
        if (value is int intValue)
        {
            r["i1"] = intValue;
        }
        else if (value is bool boolValue)
        {
            r["b1"] = boolValue;
        }
        else if (value is decimal decimalValue)
        {
            r["c1"] = decimalValue;
        }
        else if (value is Guid guidValue && (guidValue != Guid.Empty))
        {
            r["g1"] = guidValue;
        }
        else if (value is string stringValue)
        {
            r["s1"] = stringValue;
        }
        else if (value is DateTime dateTimeValue)
        {
            r["d1"] = dateTimeValue;
        }
    }

    private object SaveParameterData()
    {
        var listOfChanges = new List<ChangeInfo>();
        DataRow r = this.Data.Tables["SD"].Rows[0];
        IParameterService ps =
            ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
        int intValue = 0;
        string stringValue = "";
        Guid guidValue = Guid.Empty;
        bool boolValue = false;
        decimal decimalValue = 0;
        object dateValue = null;
        if (!r.IsNull("g1"))
        {
            guidValue = (Guid)r["g1"];
        }

        if (!r.IsNull("i1"))
        {
            intValue = (int)r["i1"];
        }

        if (!r.IsNull("s1"))
        {
            stringValue = (string)r["s1"];
        }

        if (!r.IsNull("b1"))
        {
            boolValue = (bool)r["b1"];
        }

        if (!r.IsNull("c1"))
        {
            decimalValue = (decimal)r["c1"];
        }

        if (!r.IsNull("d1"))
        {
            dateValue = (DateTime)r["d1"];
        }

        object value = DataConstant.ConvertValue(
            this.Constant.DataType,
            stringValue,
            intValue,
            guidValue,
            decimalValue,
            decimalValue,
            boolValue,
            dateValue
        );
        ps.SetCustomParameterValue(
            this.Constant.Id,
            value,
            guidValue,
            intValue,
            stringValue,
            boolValue,
            decimalValue,
            decimalValue,
            dateValue
        );
        listOfChanges.Add(ChangeInfo.SavedChangeInfo());
        if (RefreshPortalAfterSave)
        {
            listOfChanges.Add(ChangeInfo.RefreshPortalInfo());
        }
        return listOfChanges;
    }

    private object Refresh()
    {
        LoadParameterData(this.Data.Tables["SD"].Rows[0], this.Constant.Id);
        return this.Data;
    }
    #endregion
}
