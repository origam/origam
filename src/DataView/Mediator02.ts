import { IDataTable } from "./types/IDataTable";
import { IEditing } from "./types/IEditing";
import { IAvailViews } from "./types/IAvailViews";
import { IRecCursor } from "./types/IRecCursor";
import { IForm } from "./types/IForm";
import { IRecords } from "./types/IRecords";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { IProperties } from "./types/IProperties";
import { IApi } from "../Api/IApi";
import { IDataSource } from "../Screens/types";

export interface IParentMediator {
  api: IApi;
  menuItemId: string;
}

export interface ISelection {
  selRowId: string | undefined;
  selColId: string | undefined;
  selRowIdx: number | undefined;
  selColIdx: number | undefined;
}

export interface IMediator02 {
  editing: IEditing;
  dataTable: IDataTable;
  availViews: IAvailViews;
  recCursor: IRecCursor;
  form: IForm;
  records: IRecords;
  properties: IProperties;
  machine: IDataViewMachine;

  api: IApi;
  menuItemId: string;
  dataStructureEntityId: string;
  propertyIdsToLoad: string[];
  dataSource: IDataSource;
  selectedIdGetter: () => string | undefined;
}

export class Mediator02 implements IMediator02 {
  constructor(
    public P: {
      parentMediator: IParentMediator;
      dataStructureEntityId: string;
      dataSource: IDataSource;
      editing: () => IEditing;
      dataTable: () => IDataTable;
      availViews: () => IAvailViews;
      recCursor: () => IRecCursor;
      form: () => IForm;
      records: () => IRecords;
      properties: () => IProperties;
      machine: () => IDataViewMachine;
    }
  ) {}

  get editing(): IEditing {
    return this.P.editing();
  }

  get dataTable(): IDataTable {
    return this.P.dataTable();
  }

  get availViews(): IAvailViews {
    return this.P.availViews();
  }

  get recCursor(): IRecCursor {
    return this.P.recCursor();
  }

  get form(): IForm {
    return this.P.form();
  }

  get records(): IRecords {
    return this.P.records();
  }

  get properties(): IProperties {
    return this.P.properties();
  }

  get machine(): IDataViewMachine {
    return this.P.machine();
  }

  get api(): IApi {
    return this.P.parentMediator.api;
  }

  get menuItemId(): string {
    return this.P.parentMediator.menuItemId;
  }

  get dataStructureEntityId(): string {
    return this.P.dataStructureEntityId;
  }

  get propertyIdsToLoad(): string[] {
    return this.properties.ids;
  }

  get dataSource(): IDataSource {
    return this.P.dataSource;
  }

  selectedIdGetter(): string | undefined {
    return this.recCursor.selId;
  }
}
