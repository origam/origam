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

import { IReloader, IWebScreen } from "./types/IWebScreen";
import { IOpenedScreen } from "./types/IOpenedScreen";
import { action, observable } from "mobx";
import { IFormScreenEnvelope } from "./types/IFormScreen";
import { IMainMenuItemType } from "./types/IMainMenu";
import { EventHandler } from "utils/EventHandler";

export class WebScreen implements IWebScreen, IOpenedScreen {
  $type_IOpenedScreen: 1 = 1;
  $type_IWebScreen: 1 = 1;
  parentSessionId: string | undefined;

  isBeingClosed = false;

  constructor(
    title: string,
    public screenUrl: string,
    public menuItemId: string,
    public order: number,
    public canRefresh: boolean
  ) {
    this.tabTitle = title;
    this.formTitle = title;
  }

  reloader: IReloader | null = null;
  @observable stackPosition: number = 0;
  @observable tabTitle = "";
  @observable formTitle = "";
  @observable isActive = false;
  isDialog = false;
  isClosed = false;

  @action.bound
  setActive(state: boolean): void {
    this.isActive = state;
    if(this.isActive){
      this.activationHandler.call();
    }
  }

  setContent(screen: IFormScreenEnvelope): void {
  }

  setTitle(title: string): void {
    this.tabTitle = title;
  }

  setReloader(reloader: IReloader | null): void {
    this.reloader = reloader;
  }

  reload() {
    this.reloader && this.reloader.reload();
  }

  parent?: any;

  menuItemType: IMainMenuItemType = null as any;

  lazyLoading = false;
  dialogInfo = undefined;
  content: IFormScreenEnvelope = null as any;
  parameters: { [key: string]: any } = {};
  hasDynamicTitle: boolean = false;
  parentContext: IOpenedScreen | undefined;

  activationHandler = new EventHandler();

  onWindowMove(top: number, left: number): void {
  }

  get positionOffset(): { [p: string]: number } {
    return {topOffset: 0, leftOffset: 0};
  }
}
