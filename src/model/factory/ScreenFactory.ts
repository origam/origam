import { Cursor } from "../entities/cursor/Cursor";
import { DataTable } from "../entities/data/DataTable";
import { Properties } from "../entities/data/Properties";
import { Property } from "../entities/data/Property";
import { Records } from "../entities/data/Records";
import { TableView } from "../entities/specificView/table/TableView";
import { DataViews } from "../entities/specificViews/DataViews";
import { IPropertyParam, IDataViewParam } from "../types/ModelParam";
import { Model } from "../entities/Model";
import { IViewType } from "../entities/specificViews/types/IViewType";
import { ExtSelRowState } from "../entities/cursor/ExtSelRowState";
import { IExtSelRowState } from "../entities/cursor/types/ICursor";
import { FormView } from "../entities/specificView/form/FormView";

export function buildTableView({
  id,
  dataTable,
  properties,
  extSelRowState
}: {
  id: string;
  dataTable: DataTable;
  properties: Properties;
  extSelRowState: IExtSelRowState;
}) {
  const reorderedProperties = new Properties(
    properties.items,
    properties.items.map(item => item.id).filter(id => id !== "Id")
  );
  const cursor = new Cursor(dataTable, reorderedProperties, extSelRowState);
  const tableView = new TableView({
    id,
    dataTable,
    cursor,
    properties,
    reorderedProperties
  });
  return tableView;
}

export function buildFormView({
  id,
  dataTable,
  properties,
  extSelRowState
}: {
  id: string;
  dataTable: DataTable;
  properties: Properties;
  extSelRowState: IExtSelRowState;
}) {
  const reorderedProperties = new Properties(
    properties.items,
    properties.items.map(item => item.id).filter(id => id !== "Id")
  );
  const cursor = new Cursor(dataTable, reorderedProperties, extSelRowState);
  const formView = new FormView({
    id,
    dataTable,
    cursor,
    properties,
    reorderedProperties
  });
  return formView;
}

export function buildDataViews({
  id,
  initialView,
  propertyParams
}: {
  id: string;
  initialView: IViewType;
  propertyParams: IPropertyParam[];
}) {
  const records = new Records();
  const properties = new Properties(
    propertyParams.map(
      (param, idx) =>
        new Property({
          ...param,
          lookup: undefined,
          recordDataIndex: idx
        })
    )
  );
  const dataTable = new DataTable({ records, properties });
  const extSelRowState = new ExtSelRowState();
  const tableView = buildTableView({
    id,
    dataTable,
    properties,
    extSelRowState
  });
  const formView = buildFormView({
    id,
    dataTable,
    properties,
    extSelRowState
  });
  const dataViews = new DataViews(id, [tableView, formView], initialView);
  return dataViews;
}

export function buildAllDataViews(modelParam: IDataViewParam[]) {
  const dataViews = modelParam.map(mp =>
    buildDataViews({ ...mp, propertyParams: mp.properties })
  );
  const model = new Model(dataViews);
  return model;
}

export function buildScreen(modelParam: IDataViewParam[]) {
  return buildAllDataViews(modelParam);
}
