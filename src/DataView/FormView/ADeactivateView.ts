
import { action } from "mobx";
import { IADeactivateView } from "../types/IADeactivateView";
import { IAFinishEditing } from "../types/IAFinishEditing";
import { IEditing } from "../types/IEditing";
import { ML } from "../../utils/types";
import { unpack } from "../../utils/objects";

export class ADeactivateView implements IADeactivateView {
  constructor(public P: {
    editing: ML<IEditing>;
    aFinishEditing: ML<IAFinishEditing>;
  }) {}

  @action.bound
  do() : void {
    if(this.editing.isEditing) {
      this.aFinishEditing.do();
    }
  }

  get editing() {
    return unpack(this.P.editing);
  }

  get aFinishEditing() {
    return unpack(this.P.aFinishEditing);
  }
}