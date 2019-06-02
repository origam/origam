import { action } from "mobx";
import * as DataViewAction from "./DataViewActions";
import { IAInitForm } from "./types/IAInitForm";
import { IAStartEditing } from "./types/IAStartEditing";
import { IEditing } from "./types/IEditing";

export class AStartEditing implements IAStartEditing {
  constructor(
    public P: {
      editing: IEditing;
      aInitForm: IAInitForm;
    }
  ) {}

  @action.bound
  public do() {
    console.log("StartEditing");
    const editing = this.P.editing;
    // --------------------------------------------------------
    editing.setEditing(true);
    this.P.aInitForm.do();
  }
}
