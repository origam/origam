import React from "react";
import { MobXProviderContext, observer } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import { LoginPage } from "gui02/components/LoginPage/LoginPage";
import { getLoginPageFormInfoText } from "model/actions-ui/LoginPage/getLoginPageFormInfoText";
import { getLoginPageIsWorking } from "model/actions-ui/LoginPage/getLoginPageIsWorking";
import { onLoginPageSubmitButtonClick } from "model/actions-ui/LoginPage/onLoginPageSubmitButtonClick";

@observer
export class CLoginPage extends React.Component {
  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }

  render() {
    return (
      <LoginPage
        formInfoText={getLoginPageFormInfoText(this.application)}
        isWorking={getLoginPageIsWorking(this.application)}
        onSubmitLogin={onLoginPageSubmitButtonClick(this.application)}
      />
    );
  }
}
