import React from "react";
import { observer, inject } from "mobx-react";
import { IAppMachineState } from "../../Application/types/IAppMachineState";
import { IApplicationMachine } from "../../Application/types/IApplicationMachine";
import { Login } from "./Login";
import { Main } from "./Main";
import { IApplicationScope } from "../../factory/types/IApplicationScope";

@inject("applicationScope")
@observer
export class Application extends React.Component<{applicationScope? : IApplicationScope}> {
  render() {
    const {appMachine} = this.props.applicationScope!;
    switch (appMachine.stateValue) {
      case IAppMachineState.GET_CREDENTIALS:
      case IAppMachineState.DO_LOGIN:
      case IAppMachineState.LOGOUT:
        return <Login />
      case IAppMachineState.DO_LOAD_MENU:
      case IAppMachineState.LOGGED_IN:
        return <Main />
      default:
        return null;
    }
  }
}