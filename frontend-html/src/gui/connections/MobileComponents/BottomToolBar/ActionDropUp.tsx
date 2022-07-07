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
import S from "gui/connections/MobileComponents/BottomToolBar/ActionDropUp.module.scss";
import "gui/connections/MobileComponents/BottomToolBar/ActionDropUp.module.scss";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { Icon } from "@origam/components";
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import uiActions from "model/actions-ui-tree";
import { IAction } from "model/entities/types/IAction";
import { DropdownDivider } from "gui/Components/Dropdown/DropdownDivider";

export class ActionDropUp extends React.Component<{
  hidden?: boolean;
  actionGroups: IAction[][]
}> {

  render() {
    return (
      <div className={S.root}>
        {!this.props.hidden && this.props.actionGroups.length > 0
          ? <Dropdowner
            trigger={({refTrigger, setDropped}) => (
              <div
                className={S.clickArea}
                onMouseDown={() => setDropped(true)}
              >
                <div
                  ref={refTrigger}
                  className={S.iconContainer}>
                  <Icon src={"./icons/noun-right-1784045.svg"}/>
                </div>
              </div>
            )}
            content={({setDropped}) => (
              <Dropdown>
                <div className={S.dropDownContainer}>
                  {this.props.actionGroups
                    .filter(actions => actions.length > 0)
                    .map(actions =>
                      <>
                        {actions.map(action =>
                          <DropdownItem
                            key={action.id}
                            onClick={(event: any) => {
                              setDropped(false);
                              uiActions.actions.onActionClick(action)(event, action)
                            }
                            }
                          >
                            {action.caption}
                          </DropdownItem>
                        )}
                        {actions !== this.props.actionGroups[this.props.actionGroups.length - 1] &&
                          <DropdownDivider key={"group_" + actions[0].id}/>}
                      </>
                    )}
                </div>
              </Dropdown>
            )}
          />
          : <div className={S.placeholder}/>
        }
      </div>
    );
  }
}



