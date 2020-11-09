import {MobXProviderContext, observer} from "mobx-react";
import React from "react";
import {IApplication} from "model/entities/types/IApplication";
import {getIsMainMenuLoading} from "model/selectors/MainMenu/getIsMainMenuLoading";
import {getMainMenu} from "model/selectors/MainMenu/getMainMenu";
import {itemForNode} from "gui02/connections/CMainMenu";
import {getFavorites} from "model/selectors/MainMenu/getFavorites";
import {Dropdown} from "gui02/components/Dropdown/Dropdown";
import {DropdownItem} from "gui02/components/Dropdown/DropdownItem";
import {T} from "utils/translation";
import {Dropdowner} from "gui/Components/Dropdowner/Dropdowner";
import S from "gui02/connections/CFavorites.module.scss";
import {getDialogStack} from "model/selectors/getDialogStack";
import {CreateFavoriteFolderDialog} from "gui/Components/Dialogs/CreateFavoriteFolderDialog";
import {runInFlowWithHandler} from "utils/runInFlowWithHandler";

@observer
export class CFavorites extends React.Component<{
  folderName: string;
  ctx: any;
}> {

  onCreateNewFolderClick() {
    const closeDialog = getDialogStack(this.props.ctx).pushDialog(
      "",
      <CreateFavoriteFolderDialog
        onOkClick={(name: string) => {
          runInFlowWithHandler({
            ctx: this.props.ctx,
            action: () => getFavorites(this.props.ctx).createFolder(name)
          });
          closeDialog();
        }}
        onCancelClick={() => {
          closeDialog();
        }}
      />
    );
  }

  listFromNode(node: any, level: number, isOpen: boolean) {
    const favorites = getFavorites(this.props.ctx);

    return (
      <Dropdowner
        trigger={({refTrigger, setDropped}) => (
          <div ref={refTrigger}
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
                favorites.isFavorite(this.props.folderName, childNode.attributes["id"]))
              .map((node: any) => itemForNode(node, level, isOpen))}
          </div>
        )}
        content={({setDropped}) => (
          <Dropdown>
            <DropdownItem
              onClick={(event: any) => {
                setDropped(false);
                this.onCreateNewFolderClick();
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
    const isLoading = getIsMainMenuLoading(this.props.ctx);
    const mainMenu = getMainMenu(this.props.ctx);

    if (isLoading || !mainMenu) {
      return null; // TODO: More intelligent menu loading indicator...
    }
    console.log(mainMenu!.menuUI);
    return <>{this.listFromNode(mainMenu.menuUI, 1, true)}</>;
  }
}
