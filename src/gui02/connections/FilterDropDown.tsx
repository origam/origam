import { observer } from "mobx-react";
import React from "react";
import { observable } from "mobx";
import { IFilterGroup } from "model/entities/types/IFilterGroup";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { DataViewHeaderAction } from "gui02/components/DataViewHeader/DataViewHeaderAction";
import { Dropdown } from "gui02/components/Dropdown/Dropdown";
import { DropdownItem } from "gui02/components/Dropdown/DropdownItem";
import { T } from "utils/translation";
import {getFilterConfiguration} from "model/selectors/DataView/getFilterConfiguration";
import {IFilterConfiguration} from "model/entities/types/IFilterConfiguration";
import {getOpenedScreen} from "model/selectors/getOpenedScreen";
import {QuestionSaveData} from "gui/Components/Dialogs/QuestionSaveData";
import {getDialogStack} from "model/selectors/getDialogStack";
import { SaveFilterDialog } from "gui/Components/Dialogs/SaveFilterDialog";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import {IFilter} from "model/entities/types/IFilter";
import {
  IUIGridFilterCoreConfiguration,
  IUIGridFilterFieldConfiguration
} from "model/entities/types/IApi";
import { filterTypeToNumber } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operatots";
import { getApi } from "model/selectors/getApi";
import {getDataStructureEntityId} from "model/selectors/DataView/getDataStructureEntityId";
import {getDataView} from "model/selectors/DataView/getDataView";
import { getFilterGroupManager } from "model/selectors/DataView/getFilterGroupManager";
import {FilterGroupManager} from "model/entities/FilterGroupManager";

@observer
export class FilterDropDown extends React.Component<{ ctx: any }> {
  filterManager: FilterGroupManager;

  constructor(props: any) {
    super(props);
    this.filterManager = getFilterGroupManager(props.ctx)
  }

  onDropItemClick(filterGroup: IFilterGroup) {
    this.filterManager.setFilterGroup(filterGroup);
  }

  onSaveFilterClick(){
    const formScreenLifecycle = getFormScreenLifecycle(this.props.ctx);
    const closeDialog = getDialogStack(formScreenLifecycle).pushDialog(
      "",
      <SaveFilterDialog
        onOkClick={(name: string, isGlobal: boolean) => {
          this.filterManager.saveFilter(name, isGlobal);
          closeDialog();
        }}
        onCancelClick={() => {
          closeDialog();
        }}
      />
    );
  }

  async onDeleteFilterClick(){
    this.filterManager.deleteFilterGroup();
  }

  render() {
    const filterGroups = this.filterManager.filterGroups ?? []
    const onDropItemClick = (filterGroup: IFilterGroup) => this.onDropItemClick(filterGroup);

    return (
      <Dropdowner
        trigger={({ refTrigger, setDropped }) => (
          <DataViewHeaderAction
            refDom={refTrigger}
            onMouseDown={() => setDropped(true)}
            isActive={false}
          >
            <i className="fas fa-caret-down" />
          </DataViewHeaderAction>
        )}
        content={({ setDropped }) => (
          <Dropdown>
            <DropdownItem
              isDisabled={true}
              onClick={(event: any) => {
                setDropped(false);
                // onColumnConfigurationClickEvt(event);
              }}
            >
              {T("Cancel and Hide Filter", "filter_menu_filter_off")}
            </DropdownItem>
            <DropdownItem
              isDisabled={true}
              onClick={(event: any) => {
                setDropped(false);
                // onColumnConfigurationClickEvt(event);
              }}
            >
              {T("Remember The Current Filter", "filter_menu_set_default_filter")}
            </DropdownItem>
            <DropdownItem
              isDisabled={true}
              onClick={(event: any) => {
                setDropped(false);
                // onColumnConfigurationClickEvt(event);
              }}
            >
              {T("Cancel Default Filter", "filter_menu_cancel_default_filter")}
            </DropdownItem>
            <DropdownItem
              isDisabled={false}
              onClick={(event: any) => {
                setDropped(false);
                this.onSaveFilterClick();
              }}
            >
              {T("Save Current Filter", "filter_menu_save_filter")}
            </DropdownItem>
            <DropdownItem
              isDisabled={
                !this.filterManager.selectedFilterGroupId ||
                this.filterManager.isSelectedFilterGroupDefault}
              onClick={(event: any) => {
                setDropped(false);
                this.onDeleteFilterClick();
              }}
            >
              {T("Delete", "filter_menu_delete")}
            </DropdownItem>
            <DropdownItem
              isDisabled={true}
              onClick={(event: any) => {
                setDropped(false);
                // onColumnConfigurationClickEvt(event);
              }}
            >
              {T("Cancel Filter", "filter_menu_cancel")}
            </DropdownItem>
            {filterGroups.map((filterGroup) => (
              <DropdownItem
                isDisabled={false}
                isSelected={this.filterManager.selectedFilterGroupId === filterGroup.id}
                onClick={(event: any) => {
                  setDropped(false);
                  onDropItemClick(filterGroup);
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