import { DataViewHeader } from "gui02/components/DataViewHeader/DataViewHeader";
import { DataViewHeaderAction } from "gui02/components/DataViewHeader/DataViewHeaderAction";
import { DataViewHeaderButton } from "gui02/components/DataViewHeader/DataViewHeaderButton";
import { DataViewHeaderButtonGroup } from "gui02/components/DataViewHeader/DataViewHeaderButtonGroup";
import { DataViewHeaderGroup } from "gui02/components/DataViewHeader/DataViewHeaderGroup";
import { Icon } from "gui02/components/Icon/Icon";
import { MobXProviderContext, observer } from "mobx-react";
import { onColumnConfigurationClick } from "model/actions-ui/DataView/onColumnConfigurationClick";
import { onCreateRowClick } from "model/actions-ui/DataView/onCreateRowClick";
import { onDeleteRowClick } from "model/actions-ui/DataView/onDeleteRowClick";
import { onFilterButtonClick } from "model/actions-ui/DataView/onFilterButtonClick";
import { onFormViewButtonClick } from "model/actions-ui/DataView/onFormViewButtonClick";
import { onNextRowClick } from "model/actions-ui/DataView/onNextRowClick";
import { onPrevRowClick } from "model/actions-ui/DataView/onPrevRowClick";
import { onTableViewButtonClick } from "model/actions-ui/DataView/onTableViewButtonClick";
import { getActivePanelView } from "model/selectors/DataView/getActivePanelView";
import { getDataViewLabel } from "model/selectors/DataView/getDataViewLabel";
import { getPanelViewActions } from "model/selectors/DataView/getPanelViewActions";
import { getSelectedRowIndex } from "model/selectors/DataView/getSelectedRowIndex";
import { getVisibleRowCount } from "model/selectors/DataView/getVisibleRowCount";
import { getIsFilterControlsDisplayed } from "model/selectors/TablePanelView/getIsFilterControlsDisplayed";
import React from "react";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";
import { onActionClick } from "model/actions-ui/Actions/onActionClick";
import { DataViewHeaderPusher } from "gui02/components/DataViewHeader/DataViewHeaderPusher";
import { IPanelViewType } from "model/entities/types/IPanelViewType";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { Dropdown } from "gui02/components/Dropdown/Dropdown";
import { DropdownItem } from "gui02/components/Dropdown/DropdownItem";

@observer
export class CDataViewHeader extends React.Component<{ isVisible: boolean }> {
  static contextType = MobXProviderContext;

  get dataView() {
    return this.context.dataView;
  }

  render() {
    const { dataView } = this;
    const selectedRowIndex = getSelectedRowIndex(dataView);
    const visibleRowCount = getVisibleRowCount(dataView);
    const activePanelView = getActivePanelView(dataView);
    const label = getDataViewLabel(dataView);
    const isFilterSettingsVisible = getIsFilterControlsDisplayed(dataView);
    const actions = getPanelViewActions(dataView);
    const onFormViewButtonClickEvt = onFormViewButtonClick(dataView);
    const onTableViewButtonClickEvt = onTableViewButtonClick(dataView);
    const onColumnConfigurationClickEvt = onColumnConfigurationClick(dataView);
    const onDeleteRowClickEvt = onDeleteRowClick(dataView);
    const onCreateRowClickEvt = onCreateRowClick(dataView);
    const onFilterButtonClickEvt = onFilterButtonClick(dataView);
    const onPrevRowClickEvt = onPrevRowClick(dataView);
    const onNextRowClickEvt = onNextRowClick(dataView);

    return (
      <DataViewHeader isVisible={this.props.isVisible}>
        {this.props.isVisible && (
          <>
            <span>
              <h2>{label}</h2>
            </span>

            <DataViewHeaderGroup>
              <DataViewHeaderAction
                className="isGreenHover"
                onClick={onCreateRowClickEvt}
              >
                <Icon src="./icons/add.svg" />
              </DataViewHeaderAction>
              <DataViewHeaderAction
                className="isRedHover"
                onClick={onDeleteRowClickEvt}
              >
                <Icon src="./icons/minus.svg" />
              </DataViewHeaderAction>
              <DataViewHeaderAction
                className="isOrangeHover"
                onClick={undefined}
              >
                <Icon src="./icons/duplicate.svg" />
              </DataViewHeaderAction>
            </DataViewHeaderGroup>
            <DataViewHeaderButtonGroup>
              {actions
                .filter(action => getIsEnabledAction(action))
                .map(action => (
                  <DataViewHeaderButton
                    onClick={event => onActionClick(action)(event, action)}
                  >
                    {action.caption}
                  </DataViewHeaderButton>
                ))}
            </DataViewHeaderButtonGroup>
            <DataViewHeaderPusher />
            <DataViewHeaderGroup>
              <DataViewHeaderAction onClick={undefined}>
                <Icon src="./icons/list-arrow-first.svg" />
              </DataViewHeaderAction>
              <DataViewHeaderAction onClick={onPrevRowClickEvt}>
                <Icon src="./icons/list-arrow-previous.svg" />
              </DataViewHeaderAction>
              <DataViewHeaderAction onClick={onNextRowClickEvt}>
                <Icon src="./icons/list-arrow-next.svg" />
              </DataViewHeaderAction>
              <DataViewHeaderAction onClick={undefined}>
                <Icon src="./icons/list-arrow-last.svg" />
              </DataViewHeaderAction>
            </DataViewHeaderGroup>
            <DataViewHeaderGroup>
              {" "}
              <span>
                {selectedRowIndex !== undefined ? selectedRowIndex + 1 : " - "}
                &nbsp;/&nbsp;{visibleRowCount}
              </span>
            </DataViewHeaderGroup>
            <DataViewHeaderGroup>
              <DataViewHeaderAction
                onClick={onTableViewButtonClickEvt}
                isActive={activePanelView === IPanelViewType.Table}
              >
                <Icon src="./icons/table-view.svg" />
              </DataViewHeaderAction>
              <DataViewHeaderAction
                onClick={onFormViewButtonClickEvt}
                isActive={activePanelView === IPanelViewType.Form}
              >
                <Icon src="./icons/detail-view.svg" />
              </DataViewHeaderAction>
            </DataViewHeaderGroup>
            <DataViewHeaderGroup>
              <DataViewHeaderAction
                onClick={onFilterButtonClickEvt}
                isActive={isFilterSettingsVisible}
              >
                <Icon src="./icons/search-filter.svg" />
              </DataViewHeaderAction>
            </DataViewHeaderGroup>
            <DataViewHeaderGroup>
              <Dropdowner
                trigger={({ refTrigger, setDropped }) => (
                  <DataViewHeaderAction
                    refDom={refTrigger}
                    onClick={() => setDropped(true)}
                    isActive={false}
                  >
                    <Icon src="./icons/settings.svg" />
                  </DataViewHeaderAction>
                )}
                content={({ setDropped }) => (
                  <Dropdown>
                    <DropdownItem isDisabled={true}>
                      Export to Excel
                    </DropdownItem>
                    <DropdownItem
                      onClick={(event: any) => {
                        setDropped(false);
                        onColumnConfigurationClickEvt(event);
                      }}
                    >
                      Column configuration
                    </DropdownItem>
                    <DropdownItem isDisabled={true}>Show audit</DropdownItem>
                    <DropdownItem isDisabled={true}>
                      Show attachments
                    </DropdownItem>
                    <DropdownItem isDisabled={true}>
                      Show record information
                    </DropdownItem>
                  </Dropdown>
                )}
              />
            </DataViewHeaderGroup>
          </>
        )}
      </DataViewHeader>
    );
  }
}
