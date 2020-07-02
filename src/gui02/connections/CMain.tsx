import {MobXProviderContext, observer} from "mobx-react";
import {IApplication} from "model/entities/types/IApplication";
import {IApplicationPage} from "model/entities/types/IApplicationLifecycle";
import {getShownPage} from "model/selectors/Application/getShownPage";
import React from "react";
import {CLoginPage} from "./pages/CLoginPage";
import {CWorkbenchPage} from "./pages/CWorkbenchPage";
import {ApplicationDialogStack} from "gui/Components/Dialog/DialogStack";
import {getDialogStack} from "model/selectors/getDialogStack";
import cx from "classnames";

@observer
export class CMain extends React.Component {
  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }

  getPage() {
    const page = getShownPage(this.application);

    switch (page) {
      case IApplicationPage.Login:
        return <CLoginPage />;
      case IApplicationPage.Workbench:
        return <CWorkbenchPage />;
    }
  }

  render() {
    const dialogStack = getDialogStack(this.application);
    return (
      <div
        className={cx("toplevelContainer", {
          isBlurred: dialogStack.isAnyDialogShown
        })}
      >
        <ApplicationDialogStack />
        {this.getPage()}
      </div>
    );
  }
}
