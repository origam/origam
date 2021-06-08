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

import {getDataView} from "../../model/selectors/DataView/getDataView";
import {createPluginData} from "./PluginData";
import {plugins} from "../implementations/registraction";
import React from "react";
import {IPlugin} from "../types/IPlugin";

export class PluginLibrary {
  pluginNodes: Map<string, IPlugin> = new Map<string, IPlugin>();

  register(plugin: IPlugin){
    if(!plugin || !plugin.name){
      throw new Error("Plugin name cannot be empty")
    }
    this.pluginNodes.set(plugin.name, plugin)
  }

  getComponent(name: string, ctx: any): JSX.Element {
    let plugin = this.get(name);
    let dataView = getDataView(ctx);
    const pluginData = createPluginData(dataView)
    return plugin.getComponent(pluginData!);
  }

  get(name: string): IPlugin {
    if(!this.pluginNodes.has(name)){
      throw new Error("Cannot find plugin named: " + name)
    }
    return this.pluginNodes.get(name)!;
  }
}

export const pluginLibrary = new PluginLibrary();
for (let plugin of plugins) {
  pluginLibrary.register(plugin)
}
