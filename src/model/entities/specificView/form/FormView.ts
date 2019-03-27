import { IFormView } from "./types/IFormView";
import { IProperties } from "../../data/types/IProperties";
import { ICursor } from "../../cursor/types/ICursor";
import { IViewType } from "src/model/entities/specificViews/types/IViewType";
import { IDataTable } from "../../data/types/IDataTable";
import { action } from "mobx";
import { IForm } from "../../form/types/IForm";
import { Form } from "../../form/Form";
import { IFormManager } from "../../form/types/IFormManager";

export interface IFormViewParam {
  id: string;
  cursor: ICursor;
  dataTable: IDataTable;
  properties: IProperties;
  reorderedProperties: IProperties;
}

export class FormView implements IFormView, IFormManager {
  constructor(param: IFormViewParam) {
    this.id = param.id;
    this.cursor = param.cursor;
    this.dataTable = param.dataTable;
    this.properties = param.properties;
    this.reorderedProperties = param.reorderedProperties;
  }

  type: IViewType.FormView = IViewType.FormView;
  id: string;
  cursor: ICursor;
  dataTable: IDataTable;
  properties: IProperties;
  reorderedProperties: IProperties;
  form: IForm | undefined;

  @action.bound
  activate(): void {
    this.cursor.selectFirstColumn();
    this.cursor.startEditing();
  }

  deactivate(): void {
    return;
  }

  @action.bound
  initFormIfNeeded() {
    if (!this.form) {
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
  }

  @action.bound
  destroyForm() {
    this.form = undefined;
  }

  @action.bound
  submitForm(): void {
    const { selRowId, selColId } = this.cursor;
    if (selRowId && selColId && this.form) {
      for (let [fieldId, value] of this.form.dirtyValues) {
        this.dataTable.setDirtyValueById(selRowId, fieldId, value);
      }
    }
    this.form = undefined;
  }
}
