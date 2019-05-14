import { IAReloadChildren } from "./types/IAReloadChildren";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { action } from "mobx";

export class AReloadChildren implements IAReloadChildren {
  constructor(public P: { machine: IDataViewMachine }) {}

  @action.bound
  do(): void {
    this.machine.descendantsDispatch("LOAD_FRESH");
  }

  get machine() {
    return this.P.machine;
  }
}
