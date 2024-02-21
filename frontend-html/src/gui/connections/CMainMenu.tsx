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

import React, { RefObject } from "react";
import { MainMenuItem } from "gui/Components/MainMenu/MainMenuItem";
import { Icon } from "gui/Components/Icon/Icon";
import { inject, MobXProviderContext, Observer, observer } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import { getIsMainMenuLoading } from "model/selectors/MainMenu/getIsMainMenuLoading";
import { getMainMenu } from "model/selectors/MainMenu/getMainMenu";
import { action, observable } from "mobx";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { onMainMenuItemClick } from "model/actions-ui/MainMenu/onMainMenuItemClick";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import { T } from "utils/translation";
import { getFavorites } from "model/selectors/MainMenu/getFavorites";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { showDialog } from "model/selectors/getDialogStack";
import { ChooseFavoriteFolderDialog } from "gui/Components/Dialogs/ChooseFavoriteFolderDialog";
import { getIconUrl } from "gui/getIconUrl";
import { getMainMenuState } from "model/selectors/MainMenu/getMainMenuState";
import { getCustomAssetsRoute } from "model/selectors/User/getCustomAssetsRoute";
import { IMenuItemIcon } from "gui/Workbench/MainMenu/IMenuItemIcon";
import { onResetColumnConfigClick } from "model/actions-ui/MainMenu/onResetColumnConfigClick";
import S from "gui/connections/CMainMenu.module.scss";
import { SidebarSectionDivider } from "gui/Components/Sidebar/SidebarSectionDivider";
import { SidebarSectionHeader } from "gui/Components/Sidebar/SidebarSectionHeader";
import { SidebarSectionBody } from "gui/Components/Sidebar/SidebarSectionBody";
import { EditButton } from "gui/connections/MenuComponents/EditButton";
import { IEditingState, IMainMenuState } from "model/entities/types/IMainMenu";
import cx from "classnames";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { listFromNode, MenuItemList } from "./MenuItemList";

@inject(mainMenuState => mainMenuState)
@observer
export class CMainMenu extends React.Component<{
  isActive: boolean;
  onClick: () => void;
  mainMenuState?: IMainMenuState;
}> {
  static contextType = MobXProviderContext;

  @observable
  mouseInHeader = false;

  get application(): IApplication {
    return this.context.application;
  }

  onMouseEnter() {
    this.mouseInHeader = true;
    if (isMobileLayoutActive(this.application)) {
      this.props.onClick();
    }
  }

  @action
  onEditClick() {
    this.props.mainMenuState!.flipEditEnabled()
    const mainMenu = getMainMenu(this.application)!;
    if (this.props.mainMenuState!.editingEnabled) {
      this.props.onClick();
      if (!this.isAnyCommandVisible(mainMenu.menuUI)) {
        this.ensureAtLeastOneCommandVisible(mainMenu.menuUI);
      }
    }
  }

  isAnyCommandVisible(element: any) {
    const commandVisible = element.elements.some((child: any) => child.name === "Command")
    if (commandVisible) {
      return true;
    }
    for (const childElement of element.elements) {
      if (childElement.name === "Submenu" &&
        childElement.attributes.isHidden !== "true" &&
        this.props.mainMenuState!.isOpen(childElement.attributes.id)) {
        const commandsVisible = this.isAnyCommandVisible(childElement);
        if (commandsVisible) {
          return true;
        }
      }
    }
    return false;
  }

  ensureAtLeastOneCommandVisible(element: any) {
    const commandVisible = element.elements.some((child: any) => child.name === "Command")
    if (commandVisible) {
      return true;
    }
    for (const childElement of element.elements) {
      if (childElement.name === "Submenu" && childElement.attributes.isHidden !== "true") {
        if (!this.props.mainMenuState!.isOpen(childElement.attributes.id)) {
          this.props.mainMenuState!.setIsOpen(childElement.attributes.id, true);
        }
        const commandsVisible = this.ensureAtLeastOneCommandVisible(childElement);
        if (commandsVisible) {
          return true;
        }
      }
    }
    return false;
  }

  render() {
    const {application} = this;
    const isLoading = getIsMainMenuLoading(application);
    const mainMenu = getMainMenu(application);

    if (isLoading || !mainMenu) {
      return null; // TODO: More intelligent menu loading indicator...
    }
    return (
      <>
        <SidebarSectionDivider/>
        <div
          className={S.topMenuHeader}
          onMouseEnter={() => this.onMouseEnter()}
          onMouseLeave={() => this.mouseInHeader = false}
        >
          <SidebarSectionHeader
            id={"menuHeader"}
            isActive={this.props.isActive}
            icon={<Icon src="./icons/menu.svg" tooltip={T("Menu", "menu")}/>}
            label={T("Menu", "menu")}
            onClick={() => this.props.onClick()}
          />
          <Observer>
            {() =>
              <EditButton
                isVisible={this.mouseInHeader}
                isEnabled={this.props.mainMenuState!.editingEnabled}
                onClick={() => this.onEditClick()}
                tooltip={T("Manage Favourites", "manage_favorites")}
              />
            }
          </Observer>
        </div>
        <SidebarSectionBody isActive={this.props.isActive}>
          <MenuItemList ctx={application} editingState={this.props.mainMenuState!}/>
        </SidebarSectionBody>
      </>
    );
  }
}


@observer
export class CMainMenuCommandItem extends React.Component<{
  node: any;
  level: number;
  isOpen: boolean;
  editingState?: IEditingState;
}> {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  get menuId() {
    return this.props.node.attributes["id"];
  }

  get favorites() {
    return getFavorites(this.workbench);
  }

  render() {
    const {props} = this;
    const customAssetsRoute = getCustomAssetsRoute(this.workbench);
    const activeScreen = getActiveScreen(this.workbench);
    const activeMenuItemId = activeScreen ? activeScreen.menuItemId : undefined;
    const isOpenScreen = this.workbench.openedScreenIdSet.has(props.node.attributes.id);
    return (
      <Dropdowner
        trigger={({refTrigger, setDropped}) => (
          <div
            className={S.favoritesMenuItem + (isOpenScreen ? " " + S.isOpenedScreen : "")}
          >
            <MainMenuItem
              refDom={refTrigger}
              level={props.level}
              id={"menu_" + props.node.attributes.id}
              isActive={false}
              icon={
                <Icon
                  src={getIconUrl(
                    props.node.attributes.icon,
                    customAssetsRoute + "/" + props.node.attributes.icon
                  )}
                  tooltip={props.node.attributes.label}
                />
              }
              label={props.node.attributes.label}
              isHidden={!props.isOpen}
              // TODO: Implements selector for this idset
              isOpenedScreen={isOpenScreen}
              isActiveScreen={activeMenuItemId === props.node.attributes.id}
              onClick={(event) =>
                onMainMenuItemClick(this.workbench)({
                  event,
                  item: props.node,
                  idParameter: undefined,
                })
              }
              onContextMenu={(event) => {
                setDropped(true, event);
                event.preventDefault();
                event.stopPropagation();
              }}
            />
            {this.props.editingState?.editingEnabled &&
              <FavoritesAddRemoveButton
                isVisible={props.isOpen}
                menuId={this.menuId}
                ctx={this.workbench}/>
            }
          </div>
        )}
        content={({setDropped}) => (
          <Dropdown>
            <DropdownItem
              onClick={(event: any) => {
                setDropped(false);
                onMainMenuItemClick(this.workbench)({
                  event,
                  item: props.node,
                  idParameter: undefined,
                })
              }}
            >
              {T("Open", "open_form")}
            </DropdownItem>
            <DropdownItem
              onClick={(event: any) => {
                setDropped(false);
                onMainMenuItemClick(this.workbench)({
                  event,
                  item: props.node,
                  idParameter: undefined,
                  forceOpenNew: true
                })
              }}
            >
              {T("Open in New Tab", "open_in_new_tab")}
            </DropdownItem>
            {(props.node.attributes.type === "FormReferenceMenuItem" ||
                props.node.attributes.type === "FormReferenceMenuItem_WithSelection") &&
              <DropdownItem
                onClick={(event: any) => {
                  setDropped(false);
                  onResetColumnConfigClick(this.workbench)({
                    item: props.node
                  })
                }}
              >
                {T("Reset Column Configuration", "reset_column_configuration")}
              </DropdownItem>
            }
            {!this.favorites.isInAnyFavoriteFolder(this.menuId) && (
              <DropdownItem
                onClick={(event: any) => {
                  setDropped(false);
                  onAddToFavoritesClicked(this.workbench, this.menuId);
                }}
              >
                {T("Put to favourites", "put_to_favourites")}
              </DropdownItem>
            )}
          </Dropdown>
        )}
      />
    );
  }
}

@observer
export class CFavoritesMenuItem extends React.Component<{
  node: any;
  level: number;
  isOpen: boolean;
  editingEnabled: boolean;
}> {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  get menuId() {
    return this.props.node.attributes["id"];
  }

  get favorites() {
    return getFavorites(this.workbench);
  }

  getIconSrc() {
    if (this.props.editingEnabled) {
      return "./icons/minus.svg";
    }
    const customAssetsRoute = getCustomAssetsRoute(this.workbench);
    return getIconUrl(
      this.props.node.attributes.icon,
      customAssetsRoute + "/" + this.props.node.attributes.icon
    )
  }

  onIconClick(event: any) {
    if (this.props.editingEnabled) {
      onRemoveFromFavoritesClicked(this.workbench, this.menuId);
    } else {
      onMainMenuItemClick(this.workbench)({
        event,
        item: this.props.node,
        idParameter: undefined,
      })
    }
  }

  onClick(event: any) {
    if (this.props.editingEnabled) {
      return;
    }
    onMainMenuItemClick(this.workbench)({
      event,
      item: this.props.node,
      idParameter: undefined,
    })
  }

  render() {
    const {props} = this;
    const activeScreen = getActiveScreen(this.workbench);
    const activeMenuItemId = activeScreen ? activeScreen.menuItemId : undefined;
    const isOpenedScreen = this.workbench.openedScreenIdSet.has(props.node.attributes.id);
    return (
      <Dropdowner
        trigger={({refTrigger, setDropped}) => (
          <div className={cx(S.favoritesMenuItem, {openItem: isOpenedScreen})}>
            <MainMenuItem
              refDom={refTrigger}
              level={props.level}
              isActive={false}
              icon={
                <div onClick={event => this.onIconClick(event)}>
                  <Icon
                    src={this.getIconSrc()}
                    tooltip={props.node.attributes.label}
                    className={this.props.editingEnabled ? S.deleteIcon : ""}
                  />
                </div>
              }
              label={props.node.attributes.label}
              isHidden={!props.isOpen}
              isOpenedScreen={isOpenedScreen}
              isActiveScreen={activeMenuItemId === props.node.attributes.id}
              onClick={(event) => {
                if (this.props.editingEnabled) return;
                onMainMenuItemClick(this.workbench)({
                  event,
                  item: props.node,
                  idParameter: undefined,
                })
              }
              }
              onContextMenu={(event) => {
                setDropped(true, event);
                event.preventDefault();
                event.stopPropagation();
              }}
            />
            {this.props.editingEnabled ? <i className={"fas fa-bars " + S.itemGrip}/> : null}
          </div>
        )}
        content={({setDropped}) => (
          <Dropdown>
            <DropdownItem onClick={(event: any) => this.onClick(event)}>
              {T("Open", "open_form")}
            </DropdownItem>
            <DropdownItem
              onClick={(event: any) => {
                setDropped(false);
                onMainMenuItemClick(this.workbench)({
                  event,
                  item: props.node,
                  idParameter: undefined,
                  forceOpenNew: true
                })
              }}
            >
              {T("Open in New Tab", "open_in_new_tab")}
            </DropdownItem>
            {(props.node.attributes.type === "FormReferenceMenuItem" ||
                props.node.attributes.type === "FormReferenceMenuItem_WithSelection") &&
              <DropdownItem
                onClick={(event: any) => {
                  setDropped(false);
                  onResetColumnConfigClick(this.workbench)({
                    item: props.node
                  })
                }}
              >
                {T("Reset Column Configuration", "reset_column_configuration")}
              </DropdownItem>
            }
            {this.favorites.isInAnyFavoriteFolder(this.menuId) && (
              <DropdownItem
                onClick={(event: any) => {
                  setDropped(false);
                  onRemoveFromFavoritesClicked(this.workbench, this.menuId);
                }}
              >
                {T("Remove from Favourites", "remove_from_favourites")}
              </DropdownItem>
            )}
          </Dropdown>
        )}
      />
    );
  }
}

function onRemoveFromFavoritesClicked(ctx: any, menuId: string) {
  const favorites = getFavorites(ctx);
  runInFlowWithHandler({
    ctx: ctx,
    action: () => favorites.remove(menuId),
  });
}

export function onAddToFavoritesClicked(ctx: any, menuId: string) {
  const favorites = getFavorites(ctx);
  const closeDialog = showDialog(ctx,
    "",
    <ChooseFavoriteFolderDialog
      onOkClick={(folderId: string) => {
        runInFlowWithHandler({
          ctx: ctx,
          action: () => favorites.add(folderId, menuId),
        });
        closeDialog();
      }}
      onCancelClick={() => closeDialog()}
      favorites={favorites.favoriteFolders}
    />
  );
}

@observer
class FavoritesAddRemoveButton extends React.Component<{
  ctx: any;
  isVisible: boolean;
  menuId: string;
}> {
  render() {
    const favorites = getFavorites(this.props.ctx);
    return (
      <div
        className={cx(
          S.addToFavoritesIconContainer,
          {isHidden: !this.props.isVisible}
        )}
        onClick={() => {
          if (favorites.isInAnyFavoriteFolder(this.props.menuId)) {
            onRemoveFromFavoritesClicked(this.props.ctx, this.props.menuId);
          } else {
            onAddToFavoritesClicked(this.props.ctx, this.props.menuId)
          }
        }}
      >
        <Icon
          tooltip={T("Add To / Remove From Favourites", "put_remove_to_favourites")}
          className={cx(S.addToFavoritesIcon, {activeAddToFavoritesIcon: !favorites.isInAnyFavoriteFolder(this.props.menuId)})}
          src="./icons/favorites.svg"/>
      </div>
    );
  }
}

@observer
export class CMainMenuFolderItem extends React.Component<{
  node: any;
  level: number;
  isOpen: boolean;
  editingState: IEditingState
}> {
  static contextType = MobXProviderContext;
  itemRef: RefObject<HTMLDivElement> = React.createRef();

  componentDidMount() {
    this.mainMenuState.setReference(this.id, this.itemRef);
  }

  get id() {
    return this.props.node.attributes.id;
  }

  get mainMenuState() {
    return getMainMenuState(this.context.application);
  }

  @action.bound handleClick(event: any) {
    this.mainMenuState.flipIsOpen(this.id);
  }

  get icon() {
    if (this.props.node.attributes.icon !== IMenuItemIcon.Folder) {
      const customAssetsRoute = getCustomAssetsRoute(this.context.application);
      return <Icon src={customAssetsRoute + "/" + this.props.node.attributes.icon}
                   tooltip={this.props.node.attributes.label}/>;
    }
    if (this.mainMenuState.isOpen(this.id)) {
      return <Icon src="./icons/folder-open.svg" tooltip={this.props.node.attributes.label}/>;
    } else {
      return <Icon src="./icons/folder-closed.svg" tooltip={this.props.node.attributes.label}/>;
    }
  }

  render() {
    const {props} = this;
    return (
      <div
        id={this.id}
        ref={this.itemRef}>
        <MainMenuItem
          level={props.level}
          isActive={false}
          icon={this.icon}
          id={"menu_" + props.node.attributes.id}
          label={props.node.attributes.label}
          isHidden={!props.isOpen}
          onClick={this.handleClick}
          isHighLighted={this.id === this.mainMenuState.highLightedItemId}
        />
        {listFromNode(
          props.node,
          props.level + 1,
          this.props.isOpen && this.mainMenuState.isOpen(this.id),
          this.props.editingState
        )}
      </div>
    );
  }
}
