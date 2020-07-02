import {IOpenedScreens} from "./types/IOpenedScreens";
import {IOpenedScreen} from "./types/IOpenedScreen";
import {action, computed, observable} from "mobx";
import {IAction} from "./types/IAction";

export class OpenedScreens implements IOpenedScreens {
  $type_IOpenedScreens: 1 = 1;

  parent?: any;
  @observable items: Array<IOpenedScreen> = [];

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

  findLastExistingItem(menuItemId: string): IOpenedScreen | undefined {
    const items = this.items.filter(item => item.menuItemId === menuItemId);
    items.sort((a, b) => a.order - b.order);
    return items.slice(-1)[0];
  }

  findClosestItem(menuItemId: string, order: number): IOpenedScreen | undefined {
    let idx = this.items.findIndex(item => item.menuItemId === menuItemId && item.order === order);
    if (idx === -1 || this.items.length === 1) {
      return undefined;
    }
    if (idx === 0) {
      idx = 1;
    } else {
      idx--;
    }
    return this.items[idx];
  }
}
