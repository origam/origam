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
using System.Collections.Generic;
using System.Collections;
using System.Data;

using core = Origam.Workbench.Services.CoreServices;
using Origam.Schema.EntityModel;
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.DA;
using Origam.Gui;
using Origam.Schema.GuiModel;
using Origam.Rule;
using Origam.Server;
using Origam.Server.Session_Stores;
using Origam.Schema.MenuModel;
using Origam.Service.Core;

namespace Origam.Server;

public abstract class SaveableSessionStore : SessionStore
{
    private Dictionary<Guid, Dictionary<Guid,IList<Guid>>> 
        _entityFieldDependencies = new Dictionary<Guid,Dictionary<Guid,IList<Guid>>>();
    private bool _dependenciesInitialized = false;
    private DataSetBuilder datasetbuilder = new DataSetBuilder();

    public SaveableSessionStore(IBasicUIService service, UIRequest request, string name, Analytics analytics)
        : base(service, request, name, analytics)
    {
        }

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
                    IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
                    DataStructure ds = ps.SchemaProvider.RetrieveInstance(typeof(DataStructure), new ModelElementKey(this.DataStructureId)) as DataStructure;

                    foreach (DataStructureEntity entity in ds.Entities)
                    {
                        this.DirtyEnabledEntities.Add(entity.Name);
                    }
                }
            }
    }

    public DataStructure DataStructure()
    {
            return DataStructure(DataStructureId);
        }
    internal DataSet InitializeFullStructure(DataStructureDefaultSet defaultSet)
    {
            return datasetbuilder.InitializeFullStructure(DataStructureId, defaultSet);// new DatasetGenerator(true).CreateDataSet(DataStructure(), true, _menuItem.DefaultSet);
        }

    internal DataSetBuilder GetDataSetBuilder()
    {
            return datasetbuilder;
        }
    public DataStructure DataStructure(Guid id)
    {
            return datasetbuilder.DataStructure(id);
        }

    private DataStructureTemplate _template;
    public DataStructureTemplate Template
    {
        get { return _template; }
        set { _template = value; }
    }

    internal virtual object Save()
    {
            if (Data.HasErrors)
            {
                throw new UIException(Resources.ErrorInForm);
            }
            ArrayList listOfChanges = new ArrayList();
            IList<DataRow> changedRows = new List<DataRow>();
            Hashtable changedKeys = new Hashtable();
            foreach (DataTable t in Data.Tables)
            {
                foreach (DataRow r in t.Rows)
                {
                    if ((r.RowState == DataRowState.Modified) 
                    || (r.RowState == DataRowState.Added))
                    {
                        changedRows.Add(r);
                        if (!changedKeys.ContainsKey(
                            DatasetTools.PrimaryKey(r)[0]))
                        {
                            changedKeys.Add(DatasetTools.PrimaryKey(r)[0], null);
                        }
                    }
                }
            }
            // store the data
            try
            {
                core.DataService.Instance.StoreData(
                    DataStructureId, Data, 
                    RefreshAfterSaveType 
                    == SaveRefreshType.RefreshChangedRecords, 
                    TransationId);
            }
            catch (DBConcurrencyException ex)
            {
                throw new RuleException(ex.Message);
            }
            if (RefreshAfterSaveType == SaveRefreshType.RefreshChangedRecords)
            {
                foreach (DataRow r in changedRows)
                {
                    listOfChanges.AddRange(GetChangesByRow(
                        null, r, 0, changedKeys, true, false, false, false));
                    // if there is a list, we update the list, so it has the actual changed data
                    if ((DataList != null) 
                    && DataList.Tables.Contains(r.Table.TableName))
                    {
                        UpdateListRow(r);
                    }
                }
            }
            // add SAVED info, so the form looses its * sign
            listOfChanges.Add(ChangeInfo.SavedChangeInfo());
            switch (RefreshAfterSaveType)
            {
                case SaveRefreshType.RefreshCompleteForm:
                    listOfChanges.Add(ChangeInfo.RefreshFormChangeInfo());
                    break;

                case SaveRefreshType.ReloadActualRecord:
                    listOfChanges.Add(ChangeInfo.ReloadCurrentRecordChangeInfo());
                    break;
            }
            if (RefreshPortalAfterSave)
            {
                listOfChanges.Add(ChangeInfo.RefreshPortalInfo());
            }
            return listOfChanges;
        }

    public override List<ChangeInfo> CreateObject(string entity, IDictionary<string, object> values, 
        IDictionary<string, object> parameters, string requestingGrid)
    {
            if (this.Template == null || !this.Template.Entity.Name.Equals(entity))
            {
                return base.CreateObject(entity, values, parameters, requestingGrid);
            }
            else
            {
                DataTable table = GetTable(entity, this.Data);
                object[] key;
                if (parameters.Count == 0)
                {
                    key = TemplateTools.AddTemplateRecord(null, this.Template, entity, this.DataStructureId, this.Data);
                }
                else
                {
                    object[] keys = new object[parameters.Count];
                    parameters.Values.CopyTo(keys, 0);

                    DataRelation relation = table.ParentRelations[0];
                    DataRow parentRow = relation.ParentTable.Rows.Find(keys);

                    key = TemplateTools.AddTemplateRecord(parentRow, this.Template, entity, this.DataStructureId, this.Data);
                }

                DataRow newRow = table.Rows.Find(key);
                NewRowToDataList(newRow);
                List<ChangeInfo> listOfChanges = GetChangesByRow(requestingGrid, 
                    newRow, Operation.Create, this.Data.HasErrors, 
                    this.Data.HasChanges(), true);

                return listOfChanges;
            }
        }

    public override IEnumerable<ChangeInfo> UpdateObject(
        string entity, object id, string property, object newValue)
    {
            lock (_lock)
            {
                return UpdateObjectWithDependenies(entity, id,
                    property, newValue, true);
            }
        }

    public IEnumerable<ChangeInfo>
        UpdateObjectWithDependenies(
            string entity, object id, string property, object newValue,
            bool isTopLevel)
    {
            InitializeFieldDependencies();
            DataTable table = GetTable(entity, this.Data);
            Guid dsEntityId = (Guid)table.ExtendedProperties["Id"];
            Guid fieldId = (Guid)table.Columns[property].ExtendedProperties["Id"];

            if (_entityFieldDependencies.ContainsKey(dsEntityId))
            {
                if (_entityFieldDependencies[dsEntityId].ContainsKey(fieldId))
                {
                    foreach(Guid dependentColumnId in _entityFieldDependencies[dsEntityId][fieldId])
                    {
                        string dependentColumnName = ColumnNameById(table, dependentColumnId);
                        try
                        {
                            this.UpdateObjectWithDependenies(
                                entity, id,	dependentColumnName, null, false);
                        }
                        catch (NullReferenceException e) 
                        {
                            throw new NullReferenceException(
                                String.Format(Resources.ErrorDependentColumnNotFound,
                                dependentColumnName, property, entity, e.Message));
                        }
                    }
                }
            }

            // call actual UpdateObject, do rowstates only for toplevel
            // (last) update
            if (isTopLevel)
            {
                return base.UpdateObject(entity, id,
                    property, newValue);
            }
            else 
            {
                base.UpdateObjectsWithoutGetChanges(entity, id, property, newValue);
                return new List<ChangeInfo>();
            }
        }

    private static string ColumnNameById(DataTable table, Guid columnId)
    {
            foreach (DataColumn col in table.Columns)
            {
                if ((Guid)col.ExtendedProperties["Id"] == columnId)
                {
                    return col.ColumnName;
                }
            }
            throw new ArgumentOutOfRangeException("columnId", columnId, "Column not found in entity " + table.TableName);
        }

    public override string Title
    {
        get
        {
                if (!this.IsModalDialog)
                {
                    return base.Title;
                }
                else
                {
                    return "";
                }
            }
        set
        {
                base.Title = value;
            }
    }

    private void InitializeFieldDependencies()
    {
            if (_dependenciesInitialized) return;
            IPersistenceService ps = ServiceManager.Services.GetService<IPersistenceService>();

            foreach (DataTable table in this.Data.Tables)
            {
                // get entity definition
                DataStructureEntity modelEntity = ps.SchemaProvider.RetrieveInstance(typeof(DataStructureEntity),
                    new ModelElementKey((Guid)table.ExtendedProperties["Id"])) as DataStructureEntity;

                Dictionary<Guid, IList<Guid>> entityDependency = new Dictionary<Guid, IList<Guid>>();
                _entityFieldDependencies.Add(modelEntity.Id, entityDependency);

                // browse all the entity fields
                foreach (DataStructureColumn column in modelEntity.Columns)
                {
                    // read the dependencies
                    foreach (EntityFieldDependency dep in column.Field.ChildItemsByType(EntityFieldDependency.CategoryConst))
                    {
                        IList<Guid> fieldDependencies;
                        Guid fieldId = (Guid)dep.Field.PrimaryKey["Id"];
                        if (entityDependency.ContainsKey(fieldId))
                        {
                            fieldDependencies = entityDependency[fieldId];
                        }
                        else
                        {
                            fieldDependencies = new List<Guid>();
                            entityDependency.Add(fieldId, fieldDependencies);
                        }

                        // add the dependency
                        fieldDependencies.Add((Guid)column.Field.PrimaryKey["Id"]);
                    }
                }
            }
            _dependenciesInitialized = true;
        }
}