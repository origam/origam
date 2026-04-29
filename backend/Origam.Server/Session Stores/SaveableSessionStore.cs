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
using Origam.DA;
using Origam.Gui;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Server.Session_Stores;
using Origam.Service.Core;
using Origam.Workbench.Services;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public abstract class SaveableSessionStore : SessionStore
{
    private Dictionary<Guid, Dictionary<Guid, IList<Guid>>> _entityFieldDependencies =
        new Dictionary<Guid, Dictionary<Guid, IList<Guid>>>();
    private bool _dependenciesInitialized = false;
    private DataSetBuilder datasetbuilder = new DataSetBuilder();

    public SaveableSessionStore(
        IBasicUIService service,
        UIRequest request,
        string name,
        Analytics analytics
    )
        : base(service: service, request: request, name: name, analytics: analytics) { }

    private Guid _dataStructureId;
    public Guid DataStructureId
    {
        get { return _dataStructureId; }
        set
        {
            _dataStructureId = value;
            this.DirtyEnabledEntities.Clear();
            _entityFieldDependencies.Clear();
            _dependenciesInitialized = false;
            if (_dataStructureId != Guid.Empty)
            {
                IPersistenceService ps =
                    ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
                    as IPersistenceService;
                DataStructure ds =
                    ps.SchemaProvider.RetrieveInstance(
                        type: typeof(DataStructure),
                        primaryKey: new ModelElementKey(id: this.DataStructureId)
                    ) as DataStructure;
                foreach (DataStructureEntity entity in ds.Entities)
                {
                    this.DirtyEnabledEntities.Add(item: entity.Name);
                }
            }
        }
    }

    public DataStructure DataStructure()
    {
        return DataStructure(id: DataStructureId);
    }

    internal DataSet InitializeFullStructure(DataStructureDefaultSet defaultSet)
    {
        return datasetbuilder.InitializeFullStructure(id: DataStructureId, defaultSet: defaultSet); // new DatasetGenerator(true).CreateDataSet(DataStructure(), true, _menuItem.DefaultSet);
    }

    internal DataSetBuilder GetDataSetBuilder()
    {
        return datasetbuilder;
    }

    public DataStructure DataStructure(Guid id)
    {
        return datasetbuilder.DataStructure(id: id);
    }

    private DataStructureTemplate _template;
    public DataStructureTemplate Template
    {
        get { return _template; }
        set { _template = value; }
    }

    internal virtual List<ChangeInfo> Save()
    {
        if (Data.HasErrors)
        {
            throw new OrigamValidationException(message: Resources.ErrorInForm);
        }
        var listOfChanges = new List<ChangeInfo>();
        IList<DataRow> changedRows = new List<DataRow>();
        Hashtable changedKeys = new Hashtable();
        foreach (DataTable t in Data.Tables)
        {
            foreach (DataRow r in t.Rows)
            {
                if ((r.RowState == DataRowState.Modified) || (r.RowState == DataRowState.Added))
                {
                    changedRows.Add(item: r);
                    if (!changedKeys.ContainsKey(key: DatasetTools.PrimaryKey(row: r)[0]))
                    {
                        changedKeys.Add(key: DatasetTools.PrimaryKey(row: r)[0], value: null);
                    }
                }
            }
        }
        // store the data
        try
        {
            CoreServices.DataService.Instance.StoreData(
                dataStructureId: DataStructureId,
                data: Data,
                loadActualValuesAfterUpdate: RefreshAfterSaveType
                    == SaveRefreshType.RefreshChangedRecords,
                transactionId: TransationId
            );
        }
        catch (DBConcurrencyException ex)
        {
            throw new RuleException(message: ex.Message);
        }
        if (RefreshAfterSaveType == SaveRefreshType.RefreshChangedRecords)
        {
            foreach (DataRow r in changedRows)
            {
                listOfChanges.AddRange(
                    collection: GetChangesByRow(
                        requestingGrid: null,
                        row: r,
                        operation: 0,
                        ignoreKeys: changedKeys,
                        includeRowStates: true,
                        hasErrors: false,
                        hasChanges: false,
                        fromTemplate: false
                    )
                );
                // if there is a list, we update the list, so it has the actual changed data
                if ((DataList != null) && DataList.Tables.Contains(name: r.Table.TableName))
                {
                    UpdateListRow(r: r);
                }
            }
        }
        // add SAVED info, so the form looses its * sign
        listOfChanges.Add(item: ChangeInfo.SavedChangeInfo());
        switch (RefreshAfterSaveType)
        {
            case SaveRefreshType.RefreshCompleteForm:
            {
                listOfChanges.Add(item: ChangeInfo.RefreshFormChangeInfo());
                break;
            }

            case SaveRefreshType.ReloadActualRecord:
            {
                listOfChanges.Add(item: ChangeInfo.ReloadCurrentRecordChangeInfo());
                break;
            }
        }
        if (RefreshPortalAfterSave)
        {
            listOfChanges.Add(item: ChangeInfo.RefreshPortalInfo());
        }
        return listOfChanges;
    }

    public override List<ChangeInfo> CreateObject(
        string entity,
        IDictionary<string, object> values,
        IDictionary<string, object> parameters,
        string requestingGrid
    )
    {
        if (this.Template == null || !this.Template.Entity.Name.Equals(value: entity))
        {
            return base.CreateObject(
                entity: entity,
                values: values,
                parameters: parameters,
                requestingGrid: requestingGrid
            );
        }
        DataTable table = GetDataTable(entity: entity, data: this.Data);
        object[] key;
        if (parameters.Count == 0)
        {
            key = TemplateTools.AddTemplateRecord(
                parentRow: null,
                template: this.Template,
                dataMember: entity,
                dataStructureId: this.DataStructureId,
                formData: this.Data
            );
        }
        else
        {
            object[] keys = new object[parameters.Count];
            parameters.Values.CopyTo(array: keys, arrayIndex: 0);
            DataRelation relation = table.ParentRelations[index: 0];
            DataRow parentRow = relation.ParentTable.Rows.Find(keys: keys);
            key = TemplateTools.AddTemplateRecord(
                parentRow: parentRow,
                template: this.Template,
                dataMember: entity,
                dataStructureId: this.DataStructureId,
                formData: this.Data
            );
        }
        DataRow newRow = table.Rows.Find(keys: key);
        NewRowToDataList(newRow: newRow);
        List<ChangeInfo> listOfChanges = GetChangesByRow(
            requestingGrid: requestingGrid,
            row: newRow,
            operation: Operation.Create,
            hasErrors: this.Data.HasErrors,
            hasChanges: this.Data.HasChanges(),
            fromTemplate: true
        );

        return listOfChanges;
    }

    public override IEnumerable<ChangeInfo> UpdateObject(
        string entity,
        object id,
        string property,
        object newValue
    )
    {
        lock (_lock)
        {
            return UpdateObjectWithDependenies(
                entity: entity,
                id: id,
                property: property,
                newValue: newValue,
                isTopLevel: true
            );
        }
    }

    public IEnumerable<ChangeInfo> UpdateObjectWithDependenies(
        string entity,
        object id,
        string property,
        object newValue,
        bool isTopLevel
    )
    {
        InitializeFieldDependencies();
        DataTable table = GetDataTable(entity: entity, data: this.Data);
        Guid dsEntityId = (Guid)table.ExtendedProperties[key: "Id"];
        Guid fieldId = (Guid)table.Columns[name: property].ExtendedProperties[key: "Id"];
        if (_entityFieldDependencies.ContainsKey(key: dsEntityId))
        {
            if (_entityFieldDependencies[key: dsEntityId].ContainsKey(key: fieldId))
            {
                foreach (
                    Guid dependentColumnId in _entityFieldDependencies[key: dsEntityId][
                        key: fieldId
                    ]
                )
                {
                    string dependentColumnName = ColumnNameById(
                        table: table,
                        columnId: dependentColumnId
                    );
                    try
                    {
                        this.UpdateObjectWithDependenies(
                            entity: entity,
                            id: id,
                            property: dependentColumnName,
                            newValue: null,
                            isTopLevel: false
                        );
                    }
                    catch (NullReferenceException e)
                    {
                        throw new NullReferenceException(
                            message: String.Format(
                                format: Resources.ErrorDependentColumnNotFound,
                                args: [dependentColumnName, property, entity, e.Message]
                            )
                        );
                    }
                }
            }
        }
        // call actual UpdateObject, do rowstates only for toplevel
        // (last) update
        if (isTopLevel)
        {
            return base.UpdateObject(
                entity: entity,
                id: id,
                property: property,
                newValue: newValue
            );
        }
        base.UpdateObjectsWithoutGetChanges(
            entity: entity,
            id: id,
            property: property,
            newValue: newValue
        );

        return new List<ChangeInfo>();
    }

    private static string ColumnNameById(DataTable table, Guid columnId)
    {
        foreach (DataColumn col in table.Columns)
        {
            if ((Guid)col.ExtendedProperties[key: "Id"] == columnId)
            {
                return col.ColumnName;
            }
        }
        throw new ArgumentOutOfRangeException(
            paramName: "columnId",
            actualValue: columnId,
            message: "Column not found in entity " + table.TableName
        );
    }

    public override string Title
    {
        get
        {
            if (!this.IsModalDialog)
            {
                return base.Title;
            }

            return "";
        }
        set { base.Title = value; }
    }

    private void InitializeFieldDependencies()
    {
        if (_dependenciesInitialized)
        {
            return;
        }

        IPersistenceService ps = ServiceManager.Services.GetService<IPersistenceService>();
        foreach (DataTable table in this.Data.Tables)
        {
            // get entity definition
            DataStructureEntity modelEntity =
                ps.SchemaProvider.RetrieveInstance(
                    type: typeof(DataStructureEntity),
                    primaryKey: new ModelElementKey(id: (Guid)table.ExtendedProperties[key: "Id"])
                ) as DataStructureEntity;
            Dictionary<Guid, IList<Guid>> entityDependency = new Dictionary<Guid, IList<Guid>>();
            _entityFieldDependencies.Add(key: modelEntity.Id, value: entityDependency);
            // browse all the entity fields
            foreach (DataStructureColumn column in modelEntity.Columns)
            {
                // read the dependencies
                foreach (
                    var dep in column.Field.ChildItemsByType<EntityFieldDependency>(
                        itemType: EntityFieldDependency.CategoryConst
                    )
                )
                {
                    IList<Guid> fieldDependencies;
                    Guid fieldId = (Guid)dep.Field.PrimaryKey[key: "Id"];
                    if (entityDependency.ContainsKey(key: fieldId))
                    {
                        fieldDependencies = entityDependency[key: fieldId];
                    }
                    else
                    {
                        fieldDependencies = new List<Guid>();
                        entityDependency.Add(key: fieldId, value: fieldDependencies);
                    }
                    // add the dependency
                    fieldDependencies.Add(item: (Guid)column.Field.PrimaryKey[key: "Id"]);
                }
            }
        }
        _dependenciesInitialized = true;
    }
}
