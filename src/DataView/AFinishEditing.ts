import { action } from "mobx";
import { L, ML } from "../utils/types";
import { IAFinishEditing } from "./types/IAFinishEditing";
import { IEditing } from "./types/IEditing";
import { IASubmitForm } from "./types/IASubmitForm";
import { IDataViewMediator } from "./types/IDataViewMediator";
import { unpack } from "../utils/objects";
import * as DataViewActions from "./DataViewActions";

export class AFinishEditing implements IAFinishEditing {
  constructor(
    public P: {
      editing: L<IEditing>;
      aSubmitForm: L<IASubmitForm>;
      mediator: ML<IDataViewMediator>;
    }
  ) {}

  @action.bound
  public do() {
    console.log("FinishEditing");
    const editing = this.P.editing();
    // --------------------------------------------------------
    editing.setEditing(false);
    this.P.aSubmitForm().do();
    this.mediator.dispatch(DataViewActions.requestSaveData());
  }

  get mediator() {
    return unpack(this.P.mediator);
  }
}
