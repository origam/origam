import { MobXProviderContext, observer, Provider } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import React from "react";
import { CWorkbenchPage } from "gui/connections/pages/CWorkbenchPage";
import { ApplicationDialogStack } from "gui/Components/Dialog/DialogStack";
import { getDialogStack } from "model/selectors/getDialogStack";
import cx from "classnames";
import {IWorkbench} from "model/entities/types/IWorkbench";

@observer
export class CMain extends React.Component {
  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }

  get workbench(): IWorkbench {
    return this.application.workbench!;
  }

  render() {
    return (
      <div className={"toplevelContainer"} >
        <Provider workbench={this.workbench}>
          <ApplicationDialogStack />
          <CWorkbenchPage />
        </Provider>
      </div>
    );
  }
}
