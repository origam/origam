import { action } from "mobx";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { IAOnSubmitLogin } from "./types/IAOnSubmitLogin";

interface IAppMachine {
  submitLogin(userName: string, password: string): void;
}

export class AOnSubmitLogin implements IAOnSubmitLogin {
  constructor(
    public P: {
      appMachine: ML<IAppMachine>;
    }
  ) {}

  @action.bound do(userName: string, password: string) {
    this.appMachine.submitLogin(userName, password);
  }

  get appMachine() {
    return unpack(this.P.appMachine);
  }
}
