import { IAReloadChildren } from "./types/IAReloadChildren";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { action } from "mobx";

export class AReloadChildren implements IAReloadChildren {
  constructor(public P: { dataViewMachine: ML<IDataViewMachine> }) {}

  @action.bound
  do(): void {
    this.dataViewMachine.descendantsDispatch("LOAD_FRESH");
  }

  get dataViewMachine() {
    return unpack(this.P.dataViewMachine);
  }
}
