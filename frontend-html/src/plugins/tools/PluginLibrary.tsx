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

import { getDataView } from "model/selectors/DataView/getDataView";
import { createPluginData } from "./PluginData";
import React, { Fragment } from "react";
import { registerPlugins } from "plugins/tools/PluginRegistration";
import { Localizer } from "plugins/tools/Localizer";
import { ILocalization, IPlugin } from "@origam/plugin-interfaces";
import { Observer } from "mobx-react";

const pluginFactoryFunctions: Map<string, () => IPlugin> = new Map<string, () => IPlugin>();

export function registerPlugin(pluginName: string, factoryFunction: () => IPlugin) {
  pluginFactoryFunctions.set(pluginName, factoryFunction)
}

registerPlugins();

export class PluginLibrary {
  pluginInstances: Map<string, IPlugin> = new Map<string, IPlugin>();

  getComponent(args: { name: string, modelInstanceId: string, sessionId: string, ctx: any }): JSX.Element {
    const plugin = this.get({
      name: args.name,
      modelInstanceId: args.modelInstanceId,
      sessionId: args.sessionId
    });
    const dataView = getDataView(args.ctx);
    const pluginData = createPluginData(dataView)
    const createLocalizer = (localizations: ILocalization[]) => new Localizer(localizations, "en-us");
    return (
      <Observer>
        {
          () => <Fragment key={plugin.id}>{plugin.getComponent(pluginData!, createLocalizer)}</Fragment>
        }
      </Observer>);
  }

  get(args: { name: string, modelInstanceId: string, sessionId: string }): IPlugin {
    if (!args.modelInstanceId) {
      throw new Error("modelInstanceId must have a value")
    }
    if (!args.sessionId) {
      throw new Error("sessionId must have a value")
    }
    if (!args.name) {
      throw new Error("name must have a value")
    }
    let pluginId = args.modelInstanceId + "_" + args.sessionId;
    if (this.pluginInstances.has(pluginId)) {
      return this.pluginInstances.get(pluginId)!;
    }
    const plugin = this.makePluginInstance(args.name);
    plugin.id = pluginId;
    this.pluginInstances.set(plugin.id, plugin);
    return plugin;
  }

  makePluginInstance(name: string) {
    if (!pluginFactoryFunctions.has(name)) {
      throw new Error(`Cannot find plugin class named: ${name}`)
    }
    return pluginFactoryFunctions.get(name)!();
  }
}

export const pluginLibrary = new PluginLibrary();
