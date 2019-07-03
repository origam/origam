import React from "react";
import { LoginPage } from "./Login/LoginPage";
import { observer, inject } from "mobx-react";
import { getApplicationLifecycle } from "../model/selectors/getApplicationLifecycle";
import { IApplicationPage } from "../model/types/IApplicationLifecycle";
import { WorkbenchPage } from "./Workbench/WorkbenchPage";

@inject(({ application }) => ({
  page: getApplicationLifecycle(application).shownPage
}))
@observer
export class Main extends React.Component<{ page?: IApplicationPage }> {
  render() {
    switch (this.props.page!) {
      case IApplicationPage.Login:
        return <LoginPage />;
      case IApplicationPage.Workbench:
        return <WorkbenchPage />;
    }
  }
}
