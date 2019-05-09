import { action } from "mobx";
import { L } from "../utils/types";
import { IAFinishEditing } from "./types/IAFinishEditing";
import { IEditing } from "./types/IEditing";
import { IASubmitForm } from "./types/IASubmitForm";



export class AFinishEditing implements IAFinishEditing {
  constructor(public P: {
    editing: L<IEditing>;
    aSubmitForm: L<IASubmitForm>;
  }) { }

  @action.bound
  public do() {
    console.log('FinishEditing')
    const editing = this.P.editing();
    // --------------------------------------------------------
    editing.setEditing(false);
    this.P.aSubmitForm().do()
  }
}
