import { IPropReorder } from "../types/IPropReorder";
import { IPropCursor } from "../types/IPropCursor";
import { IFormViewMachine } from "./types";
import { IProperties } from "../types/IProperties";
import { IRecords } from "../types/IRecords";
import { IRecCursor } from "../types/IRecCursor";
import { IEditing } from "../types/IEditing";
import { IAvailViews } from "../types/IAvailViews";
import { IForm } from "../types/IForm";
import { IDataTable } from "../types/IDataTable";

export interface IParentMediator {
  properties: IProperties;
  records: IRecords;
  recCursor: IRecCursor;
  editing: IEditing;
  availViews: IAvailViews;
  form: IForm;
  dataTable: IDataTable;
}

export interface IFormViewMediator {
  initPropIds: string[];
  propReorder: IPropReorder;
  propCursor: IPropCursor;
  machine: IFormViewMachine;

  properties: IProperties;
  records: IRecords;
  recCursor: IRecCursor;
  editing: IEditing;
  availViews: IAvailViews;
  form: IForm;
  dataTable: IDataTable;
}

export class FormViewMediator implements IFormViewMediator {
  
  constructor(
    public P: {
      initPropIds: string[];
      parentMediator: IParentMediator;
      propReorder: () => IPropReorder;
      propCursor: () => IPropCursor;
      machine: () => IFormViewMachine;
    }
  ) {}

  get propReorder(): IPropReorder {
    return this.P.propReorder();
  }

  get propCursor(): IPropCursor {
    return this.P.propCursor();
  }

  get machine(): IFormViewMachine {
    return this.P.machine();
  }

  get properties(): IProperties {
    return this.P.parentMediator.properties;
  }

  get records(): IRecords {
    return this.P.parentMediator.records;
  }

  get recCursor(): IRecCursor {
    return this.P.parentMediator.recCursor;
  }

  get editing(): IEditing {
    return this.P.parentMediator.editing;
  }

  get availViews(): IAvailViews {
    return this.P.parentMediator.availViews;
  }

  get form(): IForm {
    return this.P.parentMediator.form;
  }

  get dataTable(): IDataTable {
    return this.P.parentMediator.dataTable;
  }

  get initPropIds(): string[] {
    return this.P.initPropIds;
  }
}
