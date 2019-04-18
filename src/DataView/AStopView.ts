import { IAStopView } from "./types/IAStopView";
import { ML } from "../utils/types";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { action } from "mobx";
import { unpack } from "../utils/objects";

export class AStopView implements IAStopView {
  constructor(public P: { machine: ML<IDataViewMachine> }) {}

  @action.bound
  do(): void {
    this.machine.stop();
  }

  get machine() {
    return unpack(this.P.machine);
  }
}
