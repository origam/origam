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

import { observable } from "mobx";
import { IScreenPlugin } from "plugins/interfaces/IScreenPlugin";
import { EventHandler } from "utils/EventHandler";
import { ILocalization } from "plugins/interfaces/ILocalization";
import { ILocalizer } from "plugins/interfaces/ILocalizer";
import { IScreenPluginData } from "plugins/interfaces/IScreenPluginData";

// The abstract keyword had to be removed because of this issue:
// https://github.com/vitejs/vite/issues/12955 
export class AbstractScreenPlugin implements IScreenPlugin {
  $type_IScreenPlugin: 1 = 1; // required by the isIScreenPlugin function
  id: string = ""

  @observable
  initialized = false;

  refreshHandler = new EventHandler();

  requestSessionRefresh: (() => Promise<any>) | undefined;

  setScreenParameters: ((parameters: { [p: string]: string }) => void) | undefined;

  getComponent(
    data: IScreenPluginData,
    createLocalizer: (localizations: ILocalization[]) => ILocalizer): JSX.Element {
      return <></>;
    }

  onSessionRefreshed() {
    this.refreshHandler.call();
  }

  initialize(xmlAttributes: { [key: string]: string }): void {
    this.initialized = true;
  }
}

