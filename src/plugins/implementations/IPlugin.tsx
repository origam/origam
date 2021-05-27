import React from "react";
import {IPluginData} from "../types/IPluginData";

export interface IPlugin {
  getComponent(data: IPluginData): JSX.Element;
  name: string;
}
