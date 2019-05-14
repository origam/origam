import { IFormField, ITableField } from "./types";
import { computed } from "mobx";
import { IPropCursor } from "../../../../DataView/types/IPropCursor";
import { ML } from "../../../../utils/types";
import { IRecCursor } from "../../../../DataView/types/IRecCursor";
import { unpack } from "../../../../utils/objects";
import { IDataTable } from "../../../../DataView/types/IDataTable";
import { IPropReorder } from "../../../../DataView/types/IPropReorder";
import { IEditing } from "../../../../DataView/types/IEditing";
import { IForm } from "../../../../DataView/types/IForm";

export class TableViewField implements IFormField {
  constructor(
    public P: {
      propCursor: ML<IPropCursor>;
      recCursor: ML<IRecCursor>;
      dataTable: ML<IDataTable>;
      propReorder: ML<IPropReorder>;
      editing: ML<IEditing>;
      form: ML<IForm>;
    }
  ) {}

  @computed get field(): ITableField | undefined {
    if (!this.isEditing) {
      return undefined;
    } else {
      const record = this.dataTable.getRecordByIdx(this.rowIndex);
      const property = this.propReorder.getByIndex(this.columnIndex);
      let value;
      let isLoading = false;
      let isError = false;
      if (record && property) {
        value = this.form.getValue(property.id);
        if (property.lookupResolver) {
          isError = property.lookupResolver.isError(value);
          isLoading = property.lookupResolver.isLoading(value);
          value = property.lookupResolver.getValue(value);
        }
        switch (property.column) {
          case "CheckBox":
            return {
              type: "BoolCell",
              value: value !== undefined && value !== null ? value : "",
              onChange: (event: any, value: boolean) => {
                this.form.setDirtyValue(property.id, value);
              },
              isLoading,
              isInvalid: false,
              isReadOnly: property.isReadOnly,
              isFocused: true
            };
          case "Text":
            return {
              type: "TextCell",
              value: value !== undefined && value !== null ? value : "",
              onChange: (event: any, value: string) => {
                this.form.setDirtyValue(property.id, value);
              },
              isLoading,
              isInvalid: false,
              isReadOnly: property.isReadOnly,
              isFocused: true
            };
        }
      }
      return {
        type: "TextCell",
        value: value !== undefined && value !== null ? value : "",
        onChange(event: any, value: string) {
          console.log("change", event, value);
        },
        isLoading,
        isInvalid: false,
        isReadOnly: true,
        isFocused: true
      };
    }
  }

  @computed get columnIndex(): number {
    return this.selPropIdx || 0;
  }

  @computed get rowIndex(): number {
    return this.selRecIdx || 0;
  }

  @computed get isEditing(): boolean {
    return this.editing.isEditing;
  }

  @computed get selPropIdx() {
    return this.propCursor.selId
      ? this.propReorder.getIndexById(this.propCursor.selId)
      : undefined;
  }

  @computed get selRecIdx() {
    return this.recCursor.selId
      ? this.dataTable.getRecordIndexById(this.recCursor.selId)
      : undefined;
  }

  get recCursor() {
    return unpack(this.P.recCursor);
  }

  get propCursor() {
    return unpack(this.P.propCursor);
  }

  get propReorder() {
    return unpack(this.P.propReorder);
  }

  get dataTable() {
    return unpack(this.P.dataTable);
  }

  get editing() {
    return unpack(this.P.editing);
  }

  get form() {
    return unpack(this.P.form);
  }
}
