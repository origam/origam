import { observable, action } from "mobx";
import { start } from "repl";
import { IAPI } from "src/DataLoadingStrategy/types";
import { getToken } from "src/DataLoadingStrategy/api";
import * as xmlJs from "xml-js";
import { IMainMenu } from "./types";
import { interpretMenu } from "./MenuInterpreter";

export class MainMenu implements IMainMenu {
  constructor(public api: IAPI) {}

  @observable.ref public reactMenu: React.ReactNode = null;
  @observable public expandedSections = new Map();

  @action.bound
  public start() {
    this.api.loadMenu({ token: getToken() }).then(
      action((response: any) => {
        const { data } = response;
        const {reactMenu} = interpretMenu(data);
        this.reactMenu = reactMenu;
      })
    );
  }

  @action.bound
  public expandSection(id: string) {
    this.expandedSections.set(id, true);
  }

  @action.bound
  public collapseSection(id: string) {
    this.expandedSections.delete(id);
  }

  public isSectionExpanded(id: string): boolean {
    return this.expandedSections.has(id);
  }

  @action.bound
  public handleSectionClick(event: any, id: string) {
    if (this.isSectionExpanded(id)) {
      this.collapseSection(id);
    } else {
      this.expandSection(id);
    }
  }
}
