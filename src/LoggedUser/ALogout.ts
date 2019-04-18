import { action } from "mobx";
import { IApplicationMachine } from "../Application/types/IApplicationMachine";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";

export class ALogout {
  constructor(public P: { appMachine: ML<IApplicationMachine> }) {}

  @action.bound do() {
    this.appMachine.logout();
  }

  get appMachine() {
    return unpack(this.P.appMachine);
  }
}
