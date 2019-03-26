import { ITableView } from "./types/ITableView";
import { ICursor } from "../../cursor/types/ICursor";
import { IProperties } from "../../data/types/IProperties";
import { IDataTable } from "../../data/types/IDataTable";

import { IForm } from "../../form/types/IForm";
import { IViewType } from "src/model/entities/specificViews/types/IViewType";
import { action } from "mobx";
import { Form } from "../../form/Form";

export interface ITableViewParam {
  id: string;
  cursor: ICursor;
  dataTable: IDataTable;
  properties: IProperties;
  reorderedProperties: IProperties;
}

export class TableView implements ITableView {

  constructor(param: ITableViewParam) {
    this.id = param.id;
    this.cursor = param.cursor;
    this.dataTable = param.dataTable;
    this.properties = param.properties;
    this.reorderedProperties = param.reorderedProperties;
  }

  id: string;
  type: IViewType.TableView = IViewType.TableView;
  cursor: ICursor;
  dataTable: IDataTable;
  properties: IProperties;
  reorderedProperties: IProperties;
  form: IForm | undefined;

  // form: IForm | undefined;

  visibleCells: any;

  updateVisibleRegion(
    rowIdxStart: number,
    rowIdxEnd: number,
    columnIdxStart: number,
    columnIdxEnd: number
  ): void {
    /*this.visibleCells.updateRegion(
      rowIdxStart,
      rowIdxEnd,
      columnIdxStart,
      columnIdxEnd
    );*/
  }

  activate(): void {
    return
  }

  deactivate(): void {
    return
  }

  @action.bound
  initForm() {
    const initialValues = this.reorderedProperties.items.map(prop => {
      return [
        prop.id,
        this.cursor.selRowId
          ? this.dataTable.getValueById(this.cursor.selRowId, prop.id)
          : ""
      ] as [string, any];
    });
    this.form = new Form(new Map(initialValues));
  }

  @action.bound
  finishForm() {
    this.form = undefined;
  }

  @action.bound
  cancelForm(): void {
    this.form = undefined;
  }

  selectCell(rowId: string | undefined, columnId: string | undefined): void {
    this.cursor.selectCell(rowId, columnId);
  }

  newRow() {
    const record = this.dataTable.getNewRecord();
    if (this.cursor.isSelected) {
      this.dataTable.insertRecordAfterId(this.cursor.selRowId!, record);
    } else {
      this.dataTable.insertRecordBeforeId(
        this.visibleCells.firstVisibleId,
        record
      );
    }
    this.cursor.startEditRow(record.id);
  }

  deleteRow() {
    if (this.cursor.isSelected) {
      const { selRowId } = this.cursor;
      this.cursor.selectClosestRowToId(selRowId!);
      this.dataTable.deleteRecordById(selRowId!);
    }
  }

  copyRow() {
    if (this.cursor.isSelected) {
      const { selRowId } = this.cursor;
      const record = this.dataTable.copyRecordById(selRowId!);
      this.dataTable.insertRecordAfterId(selRowId!, record);
      this.cursor.startEditRow(record.id);
    }
  }

  nextColumn() {
    return;
  }

  prevColumn() {
    return;
  }
}
