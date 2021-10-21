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

import { IMainMenu, IMainMenuContent, IMainMenuData, IMainMenuEnvelope } from "./types/IMainMenu";
import { action, observable } from "mobx";
import { proxyEnrich } from "utils/esproxy";

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
