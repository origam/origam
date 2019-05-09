import { IFormView, IUIFormRoot, IFormField } from "./types";
import { IViewType } from "../../../../DataView/types/IViewType";
import { ML } from "../../../../utils/types";
import { IToolbar } from "../types";
import { unpack } from "../../../../utils/objects";
import { computed, action } from "mobx";
import { IPropReorder } from "../../../../DataView/types/IPropReorder";
import { IDataTable } from "../../../../DataView/types/IDataTable";
import { IRecCursor } from "../../../../DataView/types/IRecCursor";
import { IPropCursor } from "../../../../DataView/types/IPropCursor";
import { IProperty } from "../../../../DataView/types/IProperty";
import { IForm } from "../../../../DataView/types/IForm";
import { IASelNextProp } from "../../../../DataView/types/IASelNextProp";
import { IASelPrevProp } from "../../../../DataView/types/IASelPrevProp";
import { AOnHandleClick } from "../../../../Screens/AOnHandleClick";
import { IASelProp } from "../../../../DataView/types/IASelProp";



export class FormViewPresenter implements IFormView {
  constructor(
    public P: {
      toolbar: ML<IToolbar | undefined>;
      uiStructure: ML<IUIFormRoot[]>;
      propReorder: ML<IPropReorder>;
      dataTable: ML<IDataTable>;
      recCursor: ML<IRecCursor>;
      propCursor: ML<IPropCursor>;
      form: ML<IForm>;
      aSelNextProp: ML<IASelNextProp>;
      aSelPrevProp: ML<IASelPrevProp>;
      aSelProp: ML<IASelProp>;
      isLoading: () => boolean;
    }
  ) {}

  type: IViewType.Form = IViewType.Form;

  get isLoading() {
    return this.P.isLoading();
  }

  @computed get fields() {
    const entries: Array<[string, IFormField]> = [];
    for (let prop of this.propReorder.reorderedItems) {
      entries.push([prop.id, this.getField(prop)]);
    }
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
      // value = this.dataTable.getValue(record, property);
      value = this.form.getValue(prop.id)
      if (property.lookupResolver) {
        isError = property.lookupResolver.isError(value);
        isLoading = property.lookupResolver.isLoading(value);
        value = property.lookupResolver.getValue(value);
      }
      // console.log("+++", this.propCursor.selId, prop.id);
      switch (property.column) {
        case "CheckBox":
          return {
            type: "BoolCell",
            value: value !== undefined && value !== null ? value : "",
            onChange: (event: any, value: boolean) => {
              this.form.setDirtyValue(property.id, value);
            },
            onKeyDown: this.handleKeyDown,
            onClick: (event: any) => this.handleClick(event, property.id),
            isLoading,
            isFocused: this.propCursor.selId === prop.id,
            isInvalid: false,
            isReadOnly: property.isReadOnly
          };
        case "Text":
          return {
            type: "TextCell",
            value: value !== undefined && value !== null ? value : "",
            onChange: (event: any, value: string) => {
              this.form.setDirtyValue(property.id, value);
            },
            onKeyDown: this.handleKeyDown,
            onClick: (event: any) => this.handleClick(event, property.id),
            isFocused: this.propCursor.selId === prop.id,
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
      onKeyDown: this.handleKeyDown,
      onClick: (event: any) => this.handleClick(event, property.id),
      isFocused: this.propCursor.selId === prop.id,
      isLoading,
      isInvalid: false,
      isReadOnly: property.isReadOnly
    };
  }

  @action.bound handleKeyDown(event: any) {
    console.log("Form view tex editor key down", event.key)
    switch(event.key) {
      case "Tab":
        if(event.shiftKey) {
          this.aSelPrevProp.do();
        } else {
          this.aSelNextProp.do();
        }
        event.stopPropagation();
        event.preventDefault();
      break;
    }
  }

  @action.bound handleClick(event: any, propId: string) {
    this.aSelProp.do(propId)
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

  get form() {
    return unpack(this.P.form);
  }

  get aSelNextProp() {
    return unpack(this.P.aSelNextProp);
  }

  get aSelPrevProp() {
    return unpack(this.P.aSelPrevProp);
  }

  get aSelProp() {
    return unpack(this.P.aSelProp);
  }
}
