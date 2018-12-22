import { runInAction } from "mobx";
import { observer, Provider } from "mobx-react";
import * as React from "react";
import { MainViews } from "./Application/MainViews";
import { api } from "./DataLoadingStrategy/api";
import { TopLevelLayout } from "./uiComponents/TopLevelLayout";
import { MainMenu } from "./MainMenu/MainMenu";
import { IMainViews } from "./Application/types";
import { IMainMenu } from "./MainMenu/types";

function buildApplication() {
  return
}

@observer
export default class App extends React.Component {
  constructor(props: any) {
    super(props);
    this.mainViews = new MainViews(api);
    this.mainMenu = new MainMenu(api);
  }

  public componentDidMount() {
    runInAction(() => {
      this.mainViews.start();
      this.mainMenu.start();
    });
  }

  public mainViews: IMainViews;
  public mainMenu: IMainMenu;

  public render() {
    return (
      <Provider mainViews={this.mainViews} mainMenu={this.mainMenu}>
        <TopLevelLayout />
      </Provider>
    );
  }
}
