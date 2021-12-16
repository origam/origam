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
import S from "./MobileActionDropUp.module.scss";
import "./MobileActionDropUp.module.scss";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { Icon } from "@origam/components";
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import { MobXProviderContext, observer } from "mobx-react";
import { getActiveScreenActions } from "model/selectors/getActiveScreenActions";
import { IApplication } from "model/entities/types/IApplication";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";
import uiActions from "model/actions-ui-tree";

@observer
export class MobileActionDropUp extends React.Component<{}> {

  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }

  render() {
    let iActions = getActiveScreenActions(this.application)
      .flatMap(actionGroup => actionGroup.actions);
    const actions = iActions
      .filter(action => getIsEnabledAction(action));

    return (
      <div className={S.root}>
        <Dropdowner
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
              {actions.map(action =>
                <DropdownItem
                  key={action.id}
                  onClick={(event: any) => {
                    setDropped(false);
                    uiActions.actions.onActionClick(action)(event, action)}
                  }
                >
                  {action.caption}
                </DropdownItem>
              )}
            </Dropdown>
          )}
        />
      </div>
    );
  }
}



