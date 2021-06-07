import {IPlugin} from "./IPlugin";

export interface ISectionPlugin extends IPlugin {
  getFormParameters: (() => { [key: string]: string }) | undefined;
}

export const isISectionPlugin = (o: any): o is ISectionPlugin => o?.$type_ISectionPlugin;