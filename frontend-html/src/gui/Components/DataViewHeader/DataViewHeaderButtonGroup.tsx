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
import S from "gui/Components/DataViewHeader/DataViewHeaderButtonGroup.module.scss";
import { IAction, IActionType } from "model/entities/types/IAction";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { Observer } from "mobx-react";
import { DataViewHeaderButton } from "gui/Components/DataViewHeader/DataViewHeaderButton";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import { DataViewHeaderDropDownItem } from "gui/Components/DataViewHeader/DataViewHeaderDropDownItem";
import uiActions from "model/actions-ui-tree";

export class DataViewHeaderButtonGroup extends React.Component<{
  actions: IAction[];
}> {

  renderActions() {
    return this.props.actions.map((action, idx) =>
      this.renderAction(action, this.props.actions)
    );
  }

  renderAction(action: IAction, actionsToRender: IAction[]) {
    if (action.type === IActionType.Dropdown) {
      const childActions = actionsToRender.filter(
        (otherAction) => otherAction.groupId === action.id
      );
      return (
        <Dropdowner
          style={{width: "auto"}}
          trigger={({refTrigger, setDropped}) => (
            <Observer key={action.id}>
              {() => (
                <DataViewHeaderButton
                  id={action.id}
                  title={action.caption}
                  disabled={!getIsEnabledAction(action)}
                  onClick={() => setDropped(true)}
                  domRef={refTrigger}
                >
                  {action.caption}
                </DataViewHeaderButton>
              )}
            </Observer>
          )}
          content={() => (
            <Dropdown>
              {childActions.map((action) => (
                <Observer key={action.id}>
                  {() => (
                    <DropdownItem isDisabled={!getIsEnabledAction(action)}>
                      <DataViewHeaderDropDownItem
                        onClick={(event) => uiActions.actions.onActionClick(action)(event, action)}
                      >
                        {action.caption}
                      </DataViewHeaderDropDownItem>
                    </DropdownItem>
                  )}
                </Observer>
              ))}
            </Dropdown>
          )}
        />
      );
    }
    return (
      <Observer key={action.id}>
        {() => (
          <DataViewHeaderButton
            id={action.id}
            title={action.caption}
            onClick={(event) => uiActions.actions.onActionClick(action)(event, action)}
            disabled={!getIsEnabledAction(action)}
          >
            {action.caption}
          </DataViewHeaderButton>
        )}
      </Observer>
    );
  }

  render(){
    return (
      <div className={S.root}>
        {this.renderActions()}
      </div>
    );
  }
}
