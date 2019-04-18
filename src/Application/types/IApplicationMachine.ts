import { IAppMachineState } from "./IAppMachineState";
export interface IApplicationMachine {
  start(): void;
  stop(): void;
  submitLogin(userName: string, password: string): void;
  done(): void;
  failed(): void;
  logout(): void;
  stateValue: IAppMachineState;
  isWorking: boolean;
}
