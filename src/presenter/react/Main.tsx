import { ILoggedUserScope } from "../../factory/types/ILoggedUserScope";
import { TopLevelLayout } from "./TopLevelLayout";
import { observer, inject } from "mobx-react";
import React from "react";
import { IMainMenuScope } from "../../factory/types/IMainMenuScope";
import { IMainViewsScope } from "../../factory/types/IMainViewsScope";

@inject("loggedUserScope", "mainMenuScope", "mainViewsScope")
@observer
export class Main extends React.Component<{
  loggedUserScope?: ILoggedUserScope;
  mainMenuScope?: IMainMenuScope;
  mainViewsScope?: IMainViewsScope;
}> {
  render() {
    const { loggedUser, aLogout } = this.props.loggedUserScope!;
    const { mainMenu } = this.props.mainMenuScope!;
    const mainViewsScope = this.props.mainViewsScope!;
    return (
      <TopLevelLayout
        loggedUser={loggedUser}
        aLogout={aLogout}
        mainMenu={mainMenu}
        mainViewsScope={mainViewsScope}
      />
    );
  }
}
