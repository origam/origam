import { MobXProviderContext, observer } from "mobx-react";
import React from "react";
import { IApplication } from "model/entities/types/IApplication";
import { getIsMainMenuLoading } from "model/selectors/MainMenu/getIsMainMenuLoading";
import { getMainMenu } from "model/selectors/MainMenu/getMainMenu";
import { itemForNode } from "gui02/connections/CMainMenu";
import {getFavorites} from "model/selectors/MainMenu/getFavorites";
import {MainMenuUL} from "gui02/components/MainMenu/MainMenuUL";
import {MainMenuItem} from "gui02/components/MainMenu/MainMenuItem";
import {Icon} from "gui02/components/Icon/Icon";
import {onMainMenuItemClick} from "model/actions-ui/MainMenu/onMainMenuItemClick";
import {Dropdown} from "gui02/components/Dropdown/Dropdown";
import {DropdownItem} from "gui02/components/Dropdown/DropdownItem";
import {T} from "utils/translation";
import {Dropdowner} from "gui/Components/Dropdowner/Dropdowner";
import S from "gui02/connections/CFavorites.module.scss";

@observer
export class CFavorites extends React.Component {
  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }


  listFromNode(node: any, level: number, isOpen: boolean) {
    const favorites = getFavorites(this.application);

    return (
      // <div>
      //   {node.elements
      //     .filter((childNode: any) =>
      //       childNode.attributes.isHidden !== "true" &&
      //       childNode.name !== "Submenu" &&
      //       favorites.isFavorite(childNode.attributes["id"]))
      //     .map((node: any) => itemForNode(node, level, isOpen))}
      // </div>


    <Dropdowner
      trigger={({ refTrigger, setDropped }) => (
        // <MainMenuItem
        //   refDom={refTrigger}
        //   level={props.level}
        //   isActive={false}
        //   icon={
        //     <Icon
        //       src={iconUrl(props.node.attributes.icon)}
        //       tooltip={props.node.attributes.label}
        //     />
        //   }
        //   label={
        //     props.node.attributes
        //       .label /*+ (props.node.attributes.dontRequestData === "true" ? "(DRD)" : "")*/
        //   }
        //   isHidden={!props.isOpen}
        //   // TODO: Implements selector for this idset
        //   isOpenedScreen={this.workbench.openedScreenIdSet.has(props.node.attributes.id)}
        //   isActiveScreen={activeMenuItemId === props.node.attributes.id}
        //   onClick={(event) =>
        //     onMainMenuItemClick(this.workbench)({
        //       event,
        //       item: props.node,
        //       idParameter: undefined,
        //     })
        //   }
        //   onContextMenu={(event) => {
        //     setDropped(true);
        //     event.preventDefault();
        //   }}
        // />
        <div ref = {refTrigger}
             className={S.favoritesList}
             onContextMenu={(event) => {
                 setDropped(true, event);
                 event.preventDefault();
                 event.stopPropagation();
               }}>
          {node.elements
            .filter((childNode: any) =>
              childNode.attributes.isHidden !== "true" &&
              childNode.name !== "Submenu" &&
              favorites.isFavorite(childNode.attributes["id"]))
            .map((node: any) => itemForNode(node, level, isOpen))}
        </div>
      )}
      content={({ setDropped }) => (
        <Dropdown>
          <DropdownItem
            onClick={(event: any) => {
              setDropped(false);
              // this.onAddToFavoritesClicked();
            }}
          >
            {T("Put to favourites", "add_group")}
          </DropdownItem>
        </Dropdown>
      )}
    />
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
