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

import React from "react";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { MobileState } from "model/entities/MobileState/MobileState";
import { MobXProviderContext, observer } from "mobx-react";
import { DataViewHeaderAction } from "gui/Components/DataViewHeader/DataViewHeaderAction";
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import { onScreenTabHandleClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabHandleClick";
import S from "gui/connections/MobileComponents/TopToolBar/TabSelector.module.scss"
import { Icon } from "gui/Components/Icon/Icon";
import { getLabel } from "gui/connections/CScreenTabbedViewHandleRow";

@observer
export class TabSelector extends React.Component<{
  mobileState: MobileState
}> {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  render() {
    const openedScreens = getOpenedNonDialogScreenItems(this.workbench);
    const someScreenIsDirty = openedScreens.some(screen => screen.content?.formScreen?.isDirty)
    const activeItem = openedScreens.find(item => item.isActive);
    return (
      <Dropdowner
        trigger={({refTrigger, setDropped}) => (
          <DataViewHeaderAction
            refDom={refTrigger}
            onMouseDown={() => setDropped(true)}
            isActive={false}
          >
            <div className={S.titleContainer}>
              <div className={S.screenName + " " + (activeItem?.content?.formScreen?.isDirty ? S.dirtyScreenName : "")}>
                {getLabel(activeItem)}
              </div>
              {openedScreens.length > 1 &&
                <Icon
                  className={S.icon + " " + (someScreenIsDirty ? S.dirtyIcon : "")}
                  src={"./icons/noun-arrow-89829.svg"}
                />
              }
            </div>
          </DataViewHeaderAction>
        )}
        content={({setDropped}) =>
          <Dropdown>
            {openedScreens.map(openScreen => {
                const label = getLabel(openScreen);
                return <DropdownItem
                  className={openScreen.content.formScreen?.isDirty ? S.dirtyDropDownItem : ""}
                  key={label}
                  onClick={(event: any) => {
                    setDropped(false);
                    onScreenTabHandleClick(openScreen)(event)
                  }}
                >
                  {label}
                </DropdownItem>;
              }
            )}
          </Dropdown>
        }
      />
    );
  }
}

