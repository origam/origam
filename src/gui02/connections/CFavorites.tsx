import { observer } from "mobx-react";
import React from "react";
import { getIsMainMenuLoading } from "model/selectors/MainMenu/getIsMainMenuLoading";
import { getMainMenu } from "model/selectors/MainMenu/getMainMenu";
import { itemForNode } from "gui02/connections/CMainMenu";
import { getFavorites } from "model/selectors/MainMenu/getFavorites";
import { Dropdown } from "gui02/components/Dropdown/Dropdown";
import { DropdownItem } from "gui02/components/Dropdown/DropdownItem";
import { T } from "utils/translation";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import S from "gui02/connections/CFavorites.module.scss";
import { getDialogStack } from "model/selectors/getDialogStack";
import { FavoriteFolderPropertiesDialog } from "gui/Components/Dialogs/FavoriteFolderPropertiesDialog";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { SidebarSectionHeader } from "gui02/components/Sidebar/SidebarSectionHeader";
import { Icon } from "gui02/components/Icon/Icon";
import { SidebarSection } from "gui02/components/Sidebar/SidebarSection";
import { SidebarSectionDivider } from "gui02/components/Sidebar/SidebarSectionDivider";
import { SidebarSectionBody } from "gui02/components/Sidebar/SidebarSectionBody";
import { FavoriteFolder, Favorites } from "model/entities/Favorites";

@observer
export class CFavorites extends React.Component<{
  folder: FavoriteFolder;
  isActive: boolean;
  forceOpen?: boolean;
  onHeaderClick?: () => void;
  ctx: any;
}> {
  favorites: Favorites;

  constructor(props: any) {
    super(props);
    this.favorites = getFavorites(this.props.ctx);
  }

  get canBeDeleted() {
    return this.props.folder.id !== this.favorites.dafaultFavoritesFolderId;
  }

  onCreateNewFolderClick() {
    const closeDialog = getDialogStack(this.props.ctx).pushDialog(
      "",
      <FavoriteFolderPropertiesDialog
        title={T("New Favourites Folder", "new_group_title")}
        onOkClick={(name, isPinned) => {
          runInFlowWithHandler({
            ctx: this.props.ctx,
            action: () => this.favorites.createFolder(name, isPinned),
          });
          closeDialog();
        }}
        onCancelClick={() => closeDialog()}
      />
    );
  }

  onFolderProperiesClick() {
    const closeDialog = getDialogStack(this.props.ctx).pushDialog(
      "",
      <FavoriteFolderPropertiesDialog
        title={T("Favourites Folder Properties", "group_properties_title")}
        name={this.props.folder.name}
        isPinned={this.props.folder.isPinned}
        onOkClick={(name, isPinned) => {
          runInFlowWithHandler({
            ctx: this.props.ctx,
            action: () => this.favorites.updateFolder(this.props.folder.id, name, isPinned),
          });
          closeDialog();
        }}
        onCancelClick={() => closeDialog()}
      />
    );
  }

  listFromNode(node: any, level: number, isOpen: boolean) {
    return (
      <Dropdowner
        trigger={({ refTrigger, setDropped }) => (
          <div
            ref={refTrigger}
            className={S.favoritesList}
            onContextMenu={(event) => {
              setDropped(true, event);
              event.preventDefault();
              event.stopPropagation();
            }}
          >
            {node.elements
              .filter(
                (childNode: any) =>
                  childNode.attributes.isHidden !== "true" &&
                  childNode.name !== "Submenu" &&
                  this.props.folder.has(childNode.attributes["id"])
              )
              .map((node: any) => itemForNode(node, level, isOpen))}
          </div>
        )}
        content={({ setDropped }) => (
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

  renderHeader() {
    return (
      <Dropdowner
        trigger={({ refTrigger, setDropped }) => (
          <SidebarSectionHeader
            isActive={this.props.isActive}
            icon={<Icon src="./icons/favorites.svg" tooltip={this.props.folder.name} />}
            label={this.props.folder.name}
            onClick={() => this.props.onHeaderClick?.()}
            refDom={refTrigger}
            onContextMenu={(event) => {
              setDropped(true, event);
              event.preventDefault();
              event.stopPropagation();
            }}
          />
        )}
        content={({ setDropped }) => (
          <Dropdown>
            {this.canBeDeleted && (
              <DropdownItem
                onClick={(event: any) => {
                  setDropped(false);
                  runInFlowWithHandler({
                    ctx: this.props.ctx,
                    action: () => this.favorites.removeFolder(this.props.folder.id),
                  });
                }}
              >
                {T("Remove Folder", "remove_group")}
              </DropdownItem>
            )}
            {!this.props.folder.isPinned && (
              <DropdownItem
                onClick={(event: any) => {
                  setDropped(false);
                  this.props.folder.isPinned = true;
                }}
              >
                {T("Pin to the top", "group_pin")}
              </DropdownItem>
            )}
            {this.props.folder.isPinned && (
              <DropdownItem
                onClick={(event: any) => {
                  setDropped(false);
                  this.props.folder.isPinned = false;
                }}
              >
                {T("Unpin", "group_unpin")}
              </DropdownItem>
            )}
            <DropdownItem
              onClick={(event: any) => {
                setDropped(false);
                this.onFolderProperiesClick();
              }}
            >
              {T("Properties", "group_properties")}
            </DropdownItem>
          </Dropdown>
        )}
        style={{ height: "auto" }}
      />
    );
  }

  render() {
    const isLoading = getIsMainMenuLoading(this.props.ctx);
    const mainMenu = getMainMenu(this.props.ctx);

    if (isLoading || !mainMenu) {
      return null; // TODO: More intelligent menu loading indicator...
    }

    return this.props.forceOpen ? (
      <div className={S.forceOpenSection}>
        <SidebarSectionDivider />
        {this.renderHeader()}
        <SidebarSectionBody isActive={true}>
          {this.props.folder && <>{this.listFromNode(mainMenu.menuUI, 1, true)}</>}
        </SidebarSectionBody>
      </div>
    ) : (
      <SidebarSection isActive={this.props.isActive}>
        <SidebarSectionDivider />
        {this.renderHeader()}
        <SidebarSectionBody isActive={this.props.isActive}>
          {this.props.folder && <>{this.listFromNode(mainMenu.menuUI, 1, true)}</>}
        </SidebarSectionBody>
      </SidebarSection>
    );
  }
}
