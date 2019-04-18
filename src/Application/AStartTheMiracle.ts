import { action } from "mobx";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";

interface IAppMachine {
  start(): void;
}

export class AStartTheMiracle {
  constructor(public P: {
    appMachine: ML<IAppMachine>
  }) {}

  @action.bound
  do() {
    this.appMachine.start();
  }

  get appMachine() {
    return unpack(this.P.appMachine);
  }
}