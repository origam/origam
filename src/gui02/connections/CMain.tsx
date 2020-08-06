import { MobXProviderContext, observer, Provider } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import { IApplicationPage } from "model/entities/types/IApplicationLifecycle";
import { getShownPage } from "model/selectors/Application/getShownPage";
import React from "react";
import { CLoginPage } from "./pages/CLoginPage";
import { CWorkbenchPage } from "./pages/CWorkbenchPage";
import { ApplicationDialogStack } from "gui/Components/Dialog/DialogStack";
import { getDialogStack } from "model/selectors/getDialogStack";
import cx from "classnames";
import {IWorkbench} from "../../model/entities/types/IWorkbench";

@observer
export class CMain extends React.Component {
  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }

  get workbench(): IWorkbench {
    return this.application.workbench!;
  }

  getPage() {
    const page = getShownPage(this.application);

    switch (page) {
      case IApplicationPage.Login:
        return (
          <>
            <ApplicationDialogStack />
            <CLoginPage />
          </>
        );
      case IApplicationPage.Workbench:
        return (
          <>
            <Provider workbench={this.workbench}>
              <ApplicationDialogStack />
              <CWorkbenchPage />
            </Provider>
          </>
        );
    }
  }

  render() {
    const dialogStack = getDialogStack(this.application);
    return (
      <div
        className={cx("toplevelContainer", {
          isBlurred: dialogStack.isAnyDialogShown,
        })}
      >
        {this.getPage()}
      </div>
    );
  }
}
