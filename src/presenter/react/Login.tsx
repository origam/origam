import { observer, inject } from "mobx-react";
import React from "react";
import { LoginLayout } from "./LoginLayout";
import { IAOnSubmitLogin } from "../../LoggedUser/types/IAOnSubmitLogin";
import { IApplicationMachine } from "../../Application/types/IApplicationMachine";
import { IAnouncer } from "../../Application/types/IAnouncer";
import { ILoggedUserScope } from "../../factory/types/ILoggedUserScope";
import { IApplicationScope } from "../../factory/types/IApplicationScope";

@inject("loggedUserScope", "applicationScope")
@observer
export class Login extends React.Component<{
  loggedUserScope?: ILoggedUserScope;
  applicationScope?: IApplicationScope;
}> {
  render() {
    const loggedUserScope = this.props.loggedUserScope!;
    const {aOnSubmitLogin} = loggedUserScope;
    const applicationScope = this.props.applicationScope!;
    const {appMachine, anouncer} = applicationScope;
    return (
      <LoginLayout
        aSubmitLogin={aOnSubmitLogin}
        isWorking={appMachine.isWorking}
        formInfoText={anouncer.message}
      />
    );
  }
}
