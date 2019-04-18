import { IAStartView } from "./types/IAStartView";
import { ML } from "../utils/types";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { unpack } from "../utils/objects";
import { action } from "mobx";

export class AStartView implements IAStartView {
  constructor(public P: { machine: ML<IDataViewMachine> }) {}

  @action.bound
  do(): void {
    this.machine.start();
  }

  get machine() {
    return unpack(this.P.machine);
  }
}
