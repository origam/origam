/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

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
