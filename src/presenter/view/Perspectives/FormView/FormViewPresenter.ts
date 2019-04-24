import { IFormView, IUIFormRoot, IFormField } from "./types";
import { IViewType } from "../../../../DataView/types/IViewType";
import { ML } from "../../../../utils/types";
import { IToolbar } from "../types";
import { unpack } from "../../../../utils/objects";
import { computed } from "mobx";
import { IPropReorder } from "../../../../DataView/types/IPropReorder";
import { IDataTable } from "../../../../DataView/types/IDataTable";
import { IRecCursor } from "../../../../DataView/types/IRecCursor";
import { IPropCursor } from "../../../../DataView/types/IPropCursor";
import { IProperty } from "../../../../DataView/types/IProperty";

export class FormViewPresenter implements IFormView {
  constructor(
    public P: {
      toolbar: ML<IToolbar | undefined>;
      uiStructure: ML<IUIFormRoot[]>;
      propReorder: ML<IPropReorder>;
      dataTable: ML<IDataTable>;
      recCursor: ML<IRecCursor>;
      propCursor: ML<IPropCursor>;
    }
  ) {}

  type: IViewType.Form = IViewType.Form;

  @computed get fields() {
    const entries: Array<[string, IFormField]> = [];
    for (let prop of this.propReorder.reorderedItems) {
      entries.push([prop.id, this.getField(prop)]);
    }
    console.log("***** FE", entries);
    return new Map(entries);
  }

  getField(prop: IProperty): IFormField {
    const record = this.recCursor.selId
      ? this.dataTable.getRecordById(this.recCursor.selId)
      : undefined;
    const property = prop;
    let value;
    let isLoading = false;
    let isError = false;
    if (record && property) {
      value = this.dataTable.getValue(record, property);
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
            onChange(event: any, value: boolean) {
              console.log("change", event, value);
            },
            isLoading,
            isInvalid: false,
            isReadOnly: property.isReadOnly
          };
        case "Text":
          return {
            type: "TextCell",
            value: value !== undefined && value !== null ? value : "",
            onChange(event: any, value: string) {
              console.log("change", event, value);
            },
            isLoading,
            isInvalid: false,
            isReadOnly: property.isReadOnly
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
      isReadOnly: property.isReadOnly
    };
  }

  get toolbar() {
    return unpack(this.P.toolbar);
  }

  get uiStructure() {
    return unpack(this.P.uiStructure);
  }

  get propReorder() {
    return unpack(this.P.propReorder);
  }

  get dataTable() {
    return unpack(this.P.dataTable);
  }

  get recCursor() {
    return unpack(this.P.recCursor);
  }

  get propCursor() {
    return unpack(this.P.propCursor);
  }
}
