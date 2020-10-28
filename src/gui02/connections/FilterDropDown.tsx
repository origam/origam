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

@observer
export class FilterDropDown extends React.Component<{ ctx: any }> {
  @observable
  selectedFilterGroupId: string | undefined;

  filterConfig: IFilterConfiguration;

  constructor(props: any) {
    super(props);
    this.filterConfig = getFilterConfiguration(props.ctx)
  }

  onDropItemClick(filterGroup: IFilterGroup) {
    this.selectedFilterGroupId = filterGroup.id;
    this.filterConfig.setFilterGroup(filterGroup);
  }

  onSaveFilterClick(){
    const formScreenLifecycle = getFormScreenLifecycle(this.props.ctx);
    const closeDialog = getDialogStack(formScreenLifecycle).pushDialog(
      "",
      <SaveFilterDialog
        onOkClick={(name: string, isGlobal: boolean) => {
          console.log("name: "+name+", isGlobal: "+isGlobal);
          closeDialog();
        }}
        onCancelClick={() => {
          closeDialog();
        }}
      />
    );
  }

  render() {
    const filterGroups = this.filterConfig.filterGroups ?? []
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
              isDisabled={true}
              onClick={(event: any) => {
                setDropped(false);
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
                isSelected={this.selectedFilterGroupId === filterGroup.id}
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