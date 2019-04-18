import { action } from "mobx";
import { L } from "../utils/types";
import { IACancelEditing } from "./types/IACancelEditing";
import { IEditing } from "./types/IEditing";
import { IForm } from "./types/IForm";


export class ACancelEditing implements IACancelEditing {
  constructor(
    public P: {
      editing: L<IEditing>;
      form: L<IForm>;
    }
  ) {}

  @action.bound
  public do() {
    const editing = this.P.editing();
    const form = this.P.form();
    // --------------------------------------------------------
    editing.setEditing(false);
    form.destroy();
  }
}
