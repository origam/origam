/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

import { useState } from "react";
import { FavoriteItemsList, canBeDeleted, onCreateFavoritesFolderClick, onFolderPropertiesClick } from "gui/connections/CFavorites";
import { FavoriteFolder } from "model/entities/Favorites";
import { observer } from "mobx-react";
import { getFavorites } from "model/selectors/MainMenu/getFavorites";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { Droppable } from "react-beautiful-dnd";
import S from "gui/connections/CFavorites.module.scss";
import { Icon } from "gui/Components/Icon/Icon";
import { SidebarSectionHeader } from "gui/Components/Sidebar/SidebarSectionHeader";
import { T } from "utils/translation";
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { IEditingState } from "model/entities/types/IMainMenu";

export const MobileFavoriteFolder = observer((props: {
  ctx: any;
  folder: FavoriteFolder;
  editingState: IEditingState;
}) => {
  const [isEditButtonVisible, setIsEditButtonVisible] = useState(false);
  const favorites = getFavorites(props.ctx);

  function renderHeader(){
    return (
      <Dropdowner
        trigger={({refTrigger, setDropped}) => (
          <Droppable droppableId={"favorite_folder_header_" + props.folder.id}>
            {(provided) =>
              <div
                className={S.favoritesFolderHeader}
                {...provided.droppableProps}
                ref={provided.innerRef}
              >
                <SidebarSectionHeader
                  isActive={false}
                  icon={<Icon src="./icons/favorites.svg" tooltip={props.folder.name}/>}
                  label={props.folder.name}
                  onClick={() => setIsEditButtonVisible(!isEditButtonVisible)}
                  refDom={refTrigger}
                  onContextMenu={(event) => {
                    setDropped(true, event);
                    event.preventDefault();
                    event.stopPropagation();
                  }}
                />
                {provided.placeholder}
              </div>}
          </Droppable>
        )}
        content={({setDropped}) => (
          <Dropdown>
            {canBeDeleted(props.folder, favorites) && (
              <DropdownItem
                onClick={(event: any) => {
                  setDropped(false);
                  runInFlowWithHandler({
                    ctx: props.ctx,
                    action: () => favorites.removeFolder(props.folder.id),
                  });
                }}
              >
                {T("Remove Folder", "remove_group")}
              </DropdownItem>
            )}
            <DropdownItem
              onClick={(event: any) => {
                setDropped(false);
                onCreateFavoritesFolderClick(props.ctx);
              }}
            >
              {T("Add Folder", "add_group")}
            </DropdownItem>
            <DropdownItem
              onClick={(event: any) => {
                setDropped(false);
                onFolderPropertiesClick(props.ctx, props.folder);
              }}
            >
              {T("Properties", "group_properties")}
            </DropdownItem>
          </Dropdown>
        )}
        style={{height: "auto"}}
      />
    );
  }

    
  return(
    <div>
      {renderHeader()}
      <FavoriteItemsList 
        folder={props.folder}
        editingState={props.editingState}
        ctx={props.ctx} 
      />
    </div>
  );
});
