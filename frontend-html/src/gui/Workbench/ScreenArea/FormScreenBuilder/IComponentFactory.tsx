import { INavigationNode } from "gui/connections/MobileComponents/Navigation/NavigationNode";
import { ReactNode } from "react";

export interface IComponentFactory {
  getDetailNavigator(masterNavigationNode: INavigationNode): ReactNode;

  getTabNavigator(masterNode: INavigationNode): ReactNode;

  getDataView(args: { key: string, id: string, modelInstanceId: string, isHeadless: boolean }): ReactNode;
}