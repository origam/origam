import {IMainMenu, IMainMenuContent, IMainMenuData, IMainMenuEnvelope, IMainMenuState} from "./types/IMainMenu";
import {action, observable} from "mobx";
import {proxyEnrich} from "utils/esproxy";

export class MainMenuContent implements IMainMenuContent {
  $type_IMainMenuContent: 1 = 1;

  constructor(data: IMainMenuData) {
    Object.assign(this, data);
  }

  getItemById(id: string) {
    function recursive(node: any) {
      if (node.attributes.id === id) {
        return node;
      }
      for (let ch of node.elements || []) {
        const result: any = recursive(ch);
        if (result) return result;
      }
    }
    return recursive(this.menuUI);
  }

  menuUI: any;
  parent?: any;
}

export class MainMenuEnvelope implements IMainMenuEnvelope {
  $type_IMainMenuEnvelope: 1 = 1;

  @observable mainMenu?: IMainMenu | undefined;
  @observable isLoading: boolean = false;
  mainMenuState: IMainMenuState = new MainMenuState();

  @action.bound
  setMainMenu(mainMenu: IMainMenuContent | undefined): void {
    if (mainMenu) {
      mainMenu.parent = this;
      this.mainMenu = proxyEnrich<IMainMenuEnvelope, IMainMenuContent>(
        mainMenu
      );
    } else {
      this.mainMenu = undefined;
    }
  }

  @action.bound setLoading(state: boolean) {
    this.isLoading = state;
  }

  parent?: any;
}

export class MainMenuState implements IMainMenuState {

  @observable
  folderStateMap: Map<string, boolean> = new Map();

  closeAll(){
    this.folderStateMap.clear();
  }

  isOpen(menuId: string): boolean {
    return this.folderStateMap.get(menuId) ?? false;
  }

  setIsOpen(menuId: string, state: boolean){
    this.folderStateMap.set(menuId, state);
  }

  flipIsOpen(menuId: string){
    const newState = !this.isOpen(menuId);
    this.setIsOpen(menuId, newState);
  }
}
