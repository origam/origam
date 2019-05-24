import { action } from "mobx";
import { L, ML } from "../utils/types";
import { IAFinishEditing } from "./types/IAFinishEditing";
import { IEditing } from "./types/IEditing";
import { IASubmitForm } from "./types/IASubmitForm";
import { IDataViewMediator } from "./types/IDataViewMediator";
import { unpack } from "../utils/objects";
import * as DataViewActions from "./DataViewActions";
import { IForm } from "./types/IForm";

export class AFinishEditing implements IAFinishEditing {
  constructor(
    public P: {
      editing: IEditing;
      form: IForm;
      aSubmitForm: IASubmitForm;
      dispatch: (action: any) => void;
    }
  ) {}

  @action.bound
  public do() {
    console.log("FinishEditing");
    const editing = this.P.editing;
    // --------------------------------------------------------
    editing.setEditing(false);
    this.P.aSubmitForm.do();
    this.P.form.destroy();
    this.P.dispatch(DataViewActions.requestSaveData());
  }


}
