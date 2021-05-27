import {getDataView} from "../../model/selectors/DataView/getDataView";
import {createPluginData} from "./PluginData";
import {plugins} from "../implementations/registraction";
import React from "react";
import {IPlugin} from "../implementations/IPlugin";

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
