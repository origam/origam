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
using System.Data;
using System.Xml;
using Origam.DA.Service;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.Workflow;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class OrigamDesignerSessionStore : FormSessionStore
{
    private static IDictionary<string, DataStructure> _dataStructures =
        new Dictionary<string, DataStructure>();
    private static object _dataStructureslock = new object();
    SimpleModelData _modelData;

    public OrigamDesignerSessionStore(
        IBasicUIService service,
        UIRequest request,
        string name,
        FormReferenceMenuItem menuItem,
        Guid entityId,
        Analytics analytics
    )
        : base(
            service: service,
            request: request,
            name: name,
            menuItem: menuItem,
            analytics: analytics
        )
    {
        FormId = entityId;
        GetModelData();
        SimpleModelData.OrigamEntityRow entityRow =
            _modelData.OrigamEntity.Rows[index: 0] as SimpleModelData.OrigamEntityRow;
        if (entityRow.WorkflowCount == 0)
        {
            request.Parameters.Add(key: "WorkflowId", value: Guid.Empty);
            request.Parameters.Add(key: "StateId", value: Guid.Empty);
        }
        DataStructureId = new Guid(g: "1240e912-2c96-4bb7-800c-6b6649541efc"); //GenerateDataStructure();
    }

    public override string HelpTooltipFormId
    {
        get { return "d7822ba7-a353-4612-ac5d-d1b10b381671"; }
    }

    private void GetModelData()
    {
        _modelData =
            CoreServices.DataService.Instance.LoadData(
                dataStructureId: new Guid(g: "3aaec7cd-5e40-40af-b4ad-1edd0e0fdade"),
                methodId: new Guid(g: "b3cc44d5-aa9f-4ff5-b078-15dd2f7af46f"),
                defaultSetId: Guid.Empty,
                sortSetId: new Guid(g: "d217d65d-c1f4-4b53-b449-a971277cacb8"),
                transactionId: null,
                paramName1: "OrigamEntity_parId",
                paramValue1: FormId
            ) as SimpleModelData;
    }

    public override XmlDocument GetFormXml()
    {
        if (_modelData == null)
        {
            GetModelData();
        }
        DataSet dataset = new DatasetGenerator(userDefinedParameters: true).CreateDataSet(
            ds: DataStructure()
        );
        XmlDocument result = FormXmlBuilder.GetXml(
            simpleModel: _modelData,
            dataset: dataset,
            name: this.Title
        );
        _modelData = null;
        return result;
    }

    private Guid GenerateDataStructure()
    {
        string name = "xxx";
        lock (_dataStructureslock)
        {
            if (_dataStructures.ContainsKey(key: name))
            {
                return _dataStructures[key: name].Id;
            }
            DataStructure originalDataStructure = DataStructure(
                id: new Guid(g: "1240e912-2c96-4bb7-800c-6b6649541efc")
            );
            DataStructure clone = originalDataStructure.Clone() as DataStructure;
            clone.Persist();
            _dataStructures.Add(key: name, value: clone);

            return clone.Id;
        }
    }
}
