import { observable } from "mobx";
import { MainPageContents } from "gui/connections/MobileComponents/MobileMain";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";

export class MobileState {
  @observable
  actionDropUpHidden = false;

  @observable
  refreshButtonHidden = false;

  @observable
  saveButtonHidden = false;

  _workbench: IWorkbench | undefined;

  @observable
  _mainPageContents = MainPageContents.Screen

  set mainPageContents(value: MainPageContents){
    this._mainPageContents = value;
    switch (this._mainPageContents){
      case MainPageContents.About:
      case MainPageContents.Menu:
      case MainPageContents.Search:
        this.actionDropUpHidden = true;
        this.refreshButtonHidden = true;
        this.saveButtonHidden = true;
        break;
      case MainPageContents.Screen:
        this.actionDropUpHidden = false;
        this.refreshButtonHidden = false;
        this.saveButtonHidden = false;
        break;
      default:
        throw new Error(+this._mainPageContents + " not implemented")
    }
  }

  get mainPageContents(){
    return this._mainPageContents;
  }

  set workbench(workbench: IWorkbench){
    let workbenchLifecycle = getWorkbenchLifecycle(workbench);
    workbenchLifecycle.addMainMenuItemClickHandler(
      () => this.mainPageContents = MainPageContents.Screen
    );
    this._workbench = workbench;
  }

  async close() {
    if(this._mainPageContents === MainPageContents.Screen){
      const activeScreen = getOpenedNonDialogScreenItems(this._workbench)
        .find(screen => screen.isActive);
      if(activeScreen){
        await onScreenTabCloseClick(activeScreen)(null);
      }
    }else{
      this.mainPageContents = MainPageContents.Screen;
    }
  }
}
interface IMobileLayoutState{
  actionDropUpHidden: boolean;
  refreshButtonHidden: boolean;
  saveButtonHidden: boolean;
  close(ctx: any): Promise<IMobileLayoutState>;
}

class ShowMenuState implements IMobileLayoutState{
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ShowScreenState();
  }
}

class ShowAboutState implements IMobileLayoutState{
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ShowScreenState();
  }
}

class ShowSearchState implements IMobileLayoutState{
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ShowScreenState();
  }
}

class ShowScreenState implements IMobileLayoutState{
  actionDropUpHidden = false;
  refreshButtonHidden = false;
  saveButtonHidden = false;

  async close(ctx: any): Promise<IMobileLayoutState> {
    const activeScreen = getOpenedNonDialogScreenItems(ctx)
      .find(screen => screen.isActive);
    if(activeScreen){
      await onScreenTabCloseClick(activeScreen)(null);
    }
    return this;
  }
}