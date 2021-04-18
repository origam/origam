import {IOpenedScreens} from "./types/IOpenedScreens";
import {IOpenedScreen} from "./types/IOpenedScreen";
import {action, computed, observable} from "mobx";
import {IAction} from "./types/IAction";

export class OpenedScreens implements IOpenedScreens {
  $type_IOpenedScreens: 1 = 1;

  parent?: any;
  @observable items: Array<IOpenedScreen> = [];

  isShown(openedScreen: IOpenedScreen): boolean{
    return this.items.indexOf(openedScreen) > - 1;
  }

  @action.bound
  pushItem(item: IOpenedScreen): void {
    item.stackPosition = this.maxStackPosition + 1;
    this.items.push(item);
    item.parent = this;
  }

  @action.bound
  deleteItem(menuItemId: string, order: number): void {
    const idx = this.items.findIndex(
      item => item.menuItemId === menuItemId && item.order === order
    );
    idx > -1 && this.items.splice(idx, 1);
  }

  @action.bound activateItem(menuItemId: string, order: number) {
    this.items.forEach(item => item.setActive(false));
    const item = this.items.find(item => item.menuItemId === menuItemId && item.order === order);
    if (item) {
      item.setActive(true);
      item.stackPosition = this.maxStackPosition + 1;
    }
  }

  @computed get activeItem(): IOpenedScreen | undefined {
    return this.items.find(item => item.isActive);
  }

  @computed get activeScreenActions(): Array<{
    section: string;
    actions: IAction[];
  }> {
    const activeScreen = this.activeItem;
    const result =
      activeScreen && !activeScreen.content.isLoading
        ? activeScreen.content.formScreen!.toolbarActions
        : [];
    return result;
  }

  @computed get maxStackPosition() {
    return Math.max(...this.items.map(item => item.stackPosition), 0);
  }

  findLastExistingTabItem(menuItemId: string): IOpenedScreen | undefined {
    const items = this.items.filter(item => item.menuItemId === menuItemId && !item.isDialog);
    items.sort((a, b) => a.order - b.order);
    return items.slice(-1)[0];
  }

  findTopmostItemExcept(menuItemId: string, order: number): IOpenedScreen | undefined {
    const itemsSortedByStackPosition = [...this.items].sort((a, b) => b.stackPosition - a.stackPosition);
    let idx = itemsSortedByStackPosition.findIndex(item => item.menuItemId === menuItemId && item.order === order );
    if(idx > -1) {
      itemsSortedByStackPosition.splice(idx, 1);
    }
    return itemsSortedByStackPosition[0];
  }
}
