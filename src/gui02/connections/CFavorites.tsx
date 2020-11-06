import { MobXProviderContext, observer } from "mobx-react";
import React from "react";
import { IApplication } from "model/entities/types/IApplication";
import { getIsMainMenuLoading } from "model/selectors/MainMenu/getIsMainMenuLoading";
import { getMainMenu } from "model/selectors/MainMenu/getMainMenu";
import { itemForNode } from "gui02/connections/CMainMenu";
import {getFavorites} from "model/selectors/MainMenu/getFavorites";
import {MainMenuUL} from "gui02/components/MainMenu/MainMenuUL";

@observer
export class CFavorites extends React.Component {
  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }


  listFromNode(node: any, level: number, isOpen: boolean) {
    const favorites = getFavorites(this.application);

    return (
      <>
        {node.elements
          .filter((childNode: any) =>
            childNode.attributes.isHidden !== "true" &&
            childNode.name !== "Submenu" &&
            favorites.isFavorite(childNode.attributes["id"]))
          .map((node: any) => itemForNode(node, level, isOpen))}
      </>
    );
  }

  render() {
    const { props, application } = this;
    const isLoading = getIsMainMenuLoading(application);
    const mainMenu = getMainMenu(application);

    if (isLoading || !mainMenu) {
      return null; // TODO: More intelligent menu loading indicator...
    }
    console.log(mainMenu!.menuUI);
    return <>{this.listFromNode(mainMenu.menuUI, 1, true)}</>;
  }
}
