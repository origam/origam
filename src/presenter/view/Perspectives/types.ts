import { IViewType } from "../../../DataView/types/IViewType";
import { IApi } from "../../../Api/IApi";

export interface IToolbar {
  isLoading: boolean;
  isError: boolean;
  label: string;
  isFiltered: boolean;
  btnMoveUp: IToolbarButtonState;
  btnMoveDown: IToolbarButtonState;
  btnAdd: IToolbarButtonState;
  btnDelete: IToolbarButtonState;
  btnCopy: IToolbarButtonState;
  btnFirst: IToolbarButtonState;
  btnPrev: IToolbarButtonState;
  btnNext: IToolbarButtonState;
  btnLast: IToolbarButtonState;
  recordNo: string;
  recordTotal: string;
  btnsViews: IViewTypeBtn[];
  btnFilter: IToolbarButtonState;
  btnFilterDropdown: IToolbarButtonState;
  btnSettings: IToolbarButtonState;
}

export interface IToolbarButtonState {
  isEnabled: boolean;
  isVisible: boolean;
  isActive: boolean;
  onClick?(event: any): void;
}

export interface IViewTypeBtn {
  type: IViewType;
  btn: IToolbarButtonState
}

export interface IField {
  isFocused: boolean;
  isLoading: boolean;
  isInvalid: boolean;
  isReadOnly: boolean;
}

export type ITypedField = IField & ICellTypeDU;

export type ICellTypeDU =
  | IBoolCell
  | IDateTimeCell
  | IDropdownCell
  | INumberCell
  | ITextCell;

  export interface IBoolCell {
    type: "BoolCell";
    value: boolean;
    onChange(event: any, value: boolean): void;
  }

  export interface IDateTimeCell {
    type: "DateTimeCell";
    value: string;
    outputFormat: string;
    inputFormat: string | undefined;
    onChange(event: any, value: string): void;
  }

  export interface IDropdownCell {
    type: "DropdownCell";
    value: string;
    textualValue: string;
    isLoading: boolean;
    DataStructureEntityId: string;
    ColumnNames: string[];
    Property: string;
    RowId: string;
    LookupId: string;
    menuItemId: string;
    api: IApi;
    // dropdownTable: IDropdownTable | undefined;
    onTextChange(event: any, value: string): void;
    onItemSelect(event: any, value: string): void;
  }

  export interface INumberCell {
    type: "NumberCell";
    value: number;
    format: string;
  }

  export interface ITextCell {
    type: "TextCell"
    value: string;
    onChange(event: any, value: string): void;
  }