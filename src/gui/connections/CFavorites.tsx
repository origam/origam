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

import {Observer, observer} from "mobx-react";
import React from "react";
import {getIsMainMenuLoading} from "model/selectors/MainMenu/getIsMainMenuLoading";
import {getMainMenu} from "model/selectors/MainMenu/getMainMenu";
import {CFavoritesMenuItem} from "gui/connections/CMainMenu";
import {getFavorites} from "model/selectors/MainMenu/getFavorites";
import {Dropdown} from "gui/Components/Dropdown/Dropdown";
import {DropdownItem} from "gui/Components/Dropdown/DropdownItem";
import {T} from "utils/translation";
import {Dropdowner} from "gui/Components/Dropdowner/Dropdowner";
import S from "gui/connections/CFavorites.module.scss";
import {getDialogStack} from "model/selectors/getDialogStack";
import {FavoriteFolderPropertiesDialog} from "gui/Components/Dialogs/FavoriteFolderPropertiesDialog";
import {runInFlowWithHandler} from "utils/runInFlowWithHandler";
import {SidebarSectionHeader} from "gui/Components/Sidebar/SidebarSectionHeader";
import {Icon} from "gui/Components/Icon/Icon";
import {SidebarSection} from "gui/Components/Sidebar/SidebarSection";
import {SidebarSectionDivider} from "gui/Components/Sidebar/SidebarSectionDivider";
import {SidebarSectionBody} from "gui/Components/Sidebar/SidebarSectionBody";
import {FavoriteFolder, Favorites} from "model/entities/Favorites";
import {Draggable, Droppable} from "react-beautiful-dnd";
import {action, observable} from "mobx";
import {EditButton} from "gui/connections/MenuComponents/EditButton";
import {PinButton} from "gui/connections/MenuComponents/PinButton";

@observer
export class CFavorites extends React.Component<{
  folder: FavoriteFolder;
  isActive: boolean;
  forceOpen?: boolean;
  onHeaderClick?: () => void;
  ctx: any;
}> {
  favorites: Favorites;

  dragStateContainer = new DragStateContainer();

  @observable
  mouseInHeader = false;

  constructor(props: any) {
    super(props);
    this.favorites = getFavorites(this.props.ctx);
  }

  get canBeDeleted() {
    return this.props.folder.id !== this.favorites.defaultFavoritesFolderId;
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

  onFolderPropertiesClick() {
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

  @action
  onEditClick(){
    this.dragStateContainer.flipEditEnabled();
    this.props.onHeaderClick?.();
  }

  listFromNode(node: any, level: number, isOpen: boolean) {
    const menuNodes = getAllElements(node)
      .filter(
        (childNode: any) =>
          childNode.attributes.isHidden !== "true" &&
          childNode.name !== "Submenu")
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
            <Droppable droppableId={"favorite_folder_"+this.props.folder.id}>
              {(provided) => (
                <Observer>
                  {() => (
                    <div  {...provided.droppableProps} ref={provided.innerRef}>
                      {this.props.folder.itemIds
                        .map(itemId => menuNodes.find((childNode: any) => childNode.attributes["id"] === itemId))
                        .map((node: any, index: number) =>
                            <Draggable
                              key={node.$iid}
                              draggableId={"favorite_item_" + node.attributes.id}
                              index={index}
                              isDragDisabled={!this.dragStateContainer.editingEnabled}
                            >
                              {(provided) => (
                                <Observer>
                                  {()=>
                                    <div ref={provided.innerRef} {...provided.draggableProps} {...provided.dragHandleProps}>
                                      <CFavoritesMenuItem
                                        node={node}
                                        level={level}
                                        isOpen={isOpen}
                                        editingEnabled={this.dragStateContainer.editingEnabled}
                                      />
                                    </div>
                                  }
                                </Observer>
                              )}
                            </Draggable>
                        )
                          }
                        {provided.placeholder}
                    </div>
                  )}
                </Observer>
              )}
            </Droppable>
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
              {T("Add Folder", "add_group")}
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
          <Droppable droppableId={"favorite_folder_header_"+this.props.folder.id}>
            {(provided) =>
              <div
                className={S.favoritesFolderHeader}
                {...provided.droppableProps}
                ref={provided.innerRef}
                onMouseEnter={() => this.mouseInHeader = true}
                onMouseLeave={() => this.mouseInHeader = false}
              >
                <SidebarSectionHeader
                  isActive={!this.props.forceOpen && this.props.isActive}
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
                <Observer>
                  {() =>
                    <div>
                      <PinButton
                        isVisible={this.mouseInHeader || this.dragStateContainer.editingEnabled}
                        isPinned={this.props.folder.isPinned}
                        onClick={()=> this.favorites.setPinned(this.props.folder.id, !this.props.folder.isPinned)}
                      />
                      <EditButton
                        isVisible={this.mouseInHeader}
                        isEnabled={this.dragStateContainer.editingEnabled}
                        onClick={()=>this.onEditClick()}
                        tooltip={ T("Edit Favourites", "edit_favorites")}
                      />
                    </div>
                  }
                </Observer>
              </div>}
          </Droppable>
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
                  runInFlowWithHandler({
                    ctx: this.props.ctx,
                    action: () => this.favorites.setPinned(this.props.folder.id, true),
                  });
                }}
              >
                {T("Pin to the top", "group_pin")}
              </DropdownItem>
            )}
            {this.props.folder.isPinned && (
              <DropdownItem
                onClick={(event: any) => {
                  setDropped(false);
                  runInFlowWithHandler({
                    ctx: this.props.ctx,
                    action: () => this.favorites.setPinned(this.props.folder.id, false),
                  });
                }}
              >
                {T("Unpin", "group_unpin")}
              </DropdownItem>
            )}
             <DropdownItem
              onClick={(event: any) => {
                setDropped(false);
                this.onCreateNewFolderClick();
              }}
            >
              {T("Put to favourites", "add_group")}
            </DropdownItem>
            <DropdownItem
              onClick={(event: any) => {
                setDropped(false);
                this.onFolderPropertiesClick();
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
          {this.props.folder && this.props.isActive && <>{this.listFromNode(mainMenu.menuUI, 1, true)}</>}
        </SidebarSectionBody>
      </SidebarSection>
    );
  }
}

class DragStateContainer {

  private static instances: DragStateContainer[] = [];

  @observable
  editingEnabled = false;

  constructor(){
    DragStateContainer.instances.push(this);
  }

  flipEditEnabled() {
    if(!this.editingEnabled){
      for (let instance of DragStateContainer.instances) {
        instance.editingEnabled = false;
      }
    }
    this.editingEnabled = !this.editingEnabled;
  }
}

function getAllElements(node: any): any{
  return Array.from(getAllElementsRecursive(node.elements));
}

function *getAllElementsRecursive(elements: any[]): any{
  for(let childNode of elements){
    if(childNode.name === "Submenu"){
      yield* getAllElementsRecursive(childNode.elements); 
    }else{
      yield childNode;
    }
  }
}

