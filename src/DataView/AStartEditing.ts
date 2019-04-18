import { action } from "mobx";
import { L } from "../utils/types";
import { IAStartEditing } from "./types/IAStartEditing";
import { IEditing } from "./types/IEditing";
import { IAInitForm } from "./types/IAInitForm";


export class AStartEditing implements IAStartEditing {
  constructor(public P: {
    editing: L<IEditing>;
    aInitForm: L<IAInitForm>;
  }) { }

  @action.bound
  public do() {
    const editing = this.P.editing();
    // --------------------------------------------------------
    editing.setEditing(true);
    this.P.aInitForm().do();
  }
}
