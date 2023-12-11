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
import { EventHandler } from "utils/EventHandler";
import { ISectionPluginData } from "plugins/interfaces/ISectionPluginData";
import { ILocalization } from "plugins/interfaces/ILocalization";
import { ILocalizer } from "plugins/interfaces/ILocalizer";
import { ISectionPlugin } from "plugins/interfaces/ISectionPlugin";

export abstract class AbstractSectionPlugin implements ISectionPlugin {
  $type_ISectionPlugin: 1 = 1; // required by the isISectionPlugin function
  id: string = ""

  @observable
  initialized = false;

  refreshHandler = new EventHandler();

  abstract getComponent(
    data: ISectionPluginData,
    createLocalizer: (localizations: ILocalization[]) => ILocalizer): JSX.Element

  onSessionRefreshed() {
    this.refreshHandler.call();
  }

  initialize(xmlAttributes: { [key: string]: string }): void {
    this.initialized = true;
  }

  getScreenParameters: (() => { [p: string]: string }) | undefined;
}