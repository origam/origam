import { IAppMachineState } from "./IAppMachineState";
export interface IAppMachineStateSchema {
  states: {
    [IAppMachineState.PASS_XSTATE_INIT]: {};
    [IAppMachineState.GET_CREDENTIALS]: {};
    [IAppMachineState.DO_LOGIN]: {};
    [IAppMachineState.DO_LOAD_MENU]: {};
    [IAppMachineState.LOGGED_IN]: {};
    [IAppMachineState.LOGOUT]: {};
  };
}
