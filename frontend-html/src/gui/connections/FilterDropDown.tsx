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

import { observer } from "mobx-react";
import React from "react";
import { IFilterGroup } from "model/entities/types/IFilterGroup";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { DataViewHeaderAction } from "gui/Components/DataViewHeader/DataViewHeaderAction";
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import { T } from "utils/translation";
import { showDialog } from "model/selectors/getDialogStack";
import { SaveFilterDialog } from "gui/Components/Dialogs/SaveFilterDialog";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getFilterGroupManager } from "model/selectors/DataView/getFilterGroupManager";
import { FilterGroupManager } from "model/entities/FilterGroupManager";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { FilterSwitch } from "gui/connections/FilterSwitch";
import {
  IConfigurationManager
} from "model/entities/TablePanelView/types/IConfigurationManager";
import {
  getConfigurationManager
} from "model/selectors/TablePanelView/getConfigurationManager";

@observer
export class FilterDropDown extends React.Component<{ ctx: any }> {
  filterManager: FilterGroupManager;
  configurationManager: IConfigurationManager;

  constructor(props: any) {
    super(props);
    this.filterManager = getFilterGroupManager(props.ctx)
    this.configurationManager = getConfigurationManager(props.ctx);
  }

  onDropItemClick(filterGroup: IFilterGroup) {
    this.filterManager.setFilterGroup(filterGroup);
  }

  onSaveFilterClick() {
    const formScreenLifecycle = getFormScreenLifecycle(this.props.ctx);
    const closeDialog = showDialog(formScreenLifecycle,
      "",
      <SaveFilterDialog
        onOkClick={(name: string, isGlobal: boolean) => {
          runInFlowWithHandler({
            ctx: this.filterManager,
            action: () => this.filterManager.saveActiveFiltersAsNewFilterGroup(name, isGlobal)
          });
          closeDialog();
        }}
        onCancelClick={() => {
          closeDialog();
        }}
      />
    );
  }

  render() {
    const filterGroups = this.filterManager.filterGroups ?? []

    return (
      <Dropdowner
        trigger={({refTrigger, setDropped}) => (
          <DataViewHeaderAction
            refDom={refTrigger}
            onMouseDown={() => setDropped(true)}
            isActive={false}
          >
            <i className="fas fa-caret-down"/>
          </DataViewHeaderAction>
        )}
        content={({setDropped}) => (
          <Dropdown>
            <DropdownItem>
              <FilterSwitch configurationManager={this.configurationManager}/>
            </DropdownItem>
            <DropdownItem
              isDisabled={this.filterManager.filtersHidden}
              onClick={(event: any) => {
                setDropped(false);
                runInFlowWithHandler({
                  ctx: this.filterManager,
                  action: () => this.filterManager.clearFiltersAndClose(event)
                });
              }}
            >
              {T("Cancel and Hide Filter", "filter_menu_filter_off")}
            </DropdownItem>
            <DropdownItem
              isDisabled={
                this.filterManager.filtersHidden ||
                this.filterManager.isSelectedFilterGroupDefault ||
                this.filterManager.noFilterActive}
              onClick={(event: any) => {
                setDropped(false);
                runInFlowWithHandler({
                  ctx: this.filterManager,
                  action: () => this.filterManager.cancelSelectedFilter()
                });
              }}
            >
              {T("Cancel Filter", "filter_menu_cancel")}
            </DropdownItem>
            <DropdownItem
              isDisabled={
                this.filterManager.filtersHidden ||
                this.filterManager.noFilterActive}
              onClick={(event: any) => {
                setDropped(false);
                runInFlowWithHandler({
                  ctx: this.filterManager,
                  action: () => this.filterManager.setSelectedFilterGroupAsDefault()
                });
              }}
            >
              {T("Set the Current Filter as Default", "filter_menu_set_default_filter")}
            </DropdownItem>
            <DropdownItem
              isDisabled={!this.filterManager.defaultFilter || this.filterManager.filtersHidden}
              onClick={(event: any) => {
                setDropped(false);
                runInFlowWithHandler({
                  ctx: this.filterManager,
                  action: () => this.filterManager.resetDefaultFilterGroup()
                });
              }}
            >
              {T("Cancel Default Filter", "filter_menu_cancel_default_filter")}
            </DropdownItem>
            <DropdownItem
              isDisabled={
                this.filterManager.filtersHidden ||
                this.filterManager.noFilterActive}
              onClick={(event: any) => {
                setDropped(false);
                this.onSaveFilterClick();
              }}
            >
              {T("Save Current Filter", "filter_menu_save_filter")}
            </DropdownItem>
            <DropdownItem
              isDisabled={
                !this.filterManager.selectedFilterGroup ||
                this.filterManager.isSelectedFilterGroupDefault}
              onClick={(event: any) => {
                setDropped(false);
                runInFlowWithHandler({
                  ctx: this.filterManager,
                  action: () => this.filterManager.deleteFilterGroup()
                });
              }}
            >
              {T("Delete", "filter_menu_delete")}
            </DropdownItem>
            {filterGroups.map((filterGroup) => (
              <DropdownItem
                key={filterGroup.id}
                isDisabled={false}
                isSelected={this.filterManager.selectedFilterGroup?.id === filterGroup.id}
                onClick={(event: any) => {
                  setDropped(false);
                  runInFlowWithHandler({
                    ctx: this.filterManager,
                    action: () => this.onDropItemClick(filterGroup)
                  });
                }}
              >
                {filterGroup.name}
              </DropdownItem>
            ))}
          </Dropdown>
        )}
      />
    );
  }
}