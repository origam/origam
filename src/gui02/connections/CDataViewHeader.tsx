import { scopeFor } from "dic/Container";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { DataViewHeader } from "gui02/components/DataViewHeader/DataViewHeader";
import { DataViewHeaderAction } from "gui02/components/DataViewHeader/DataViewHeaderAction";
import { DataViewHeaderButton } from "gui02/components/DataViewHeader/DataViewHeaderButton";
import { DataViewHeaderButtonGroup } from "gui02/components/DataViewHeader/DataViewHeaderButtonGroup";
import { DataViewHeaderGroup } from "gui02/components/DataViewHeader/DataViewHeaderGroup";
import { DataViewHeaderPusher } from "gui02/components/DataViewHeader/DataViewHeaderPusher";
import { Dropdown } from "gui02/components/Dropdown/Dropdown";
import { DropdownItem } from "gui02/components/Dropdown/DropdownItem";
import { Icon } from "gui02/components/Icon/Icon";
import { MobXProviderContext, observer } from "mobx-react";
import uiActions from "model/actions-ui-tree";
import { onColumnConfigurationClick } from "model/actions-ui/DataView/onColumnConfigurationClick";
import { onCreateRowClick } from "model/actions-ui/DataView/onCreateRowClick";
import { onDeleteRowClick } from "model/actions-ui/DataView/onDeleteRowClick";
import { onFilterButtonClick } from "model/actions-ui/DataView/onFilterButtonClick";
import { onNextRowClick } from "model/actions-ui/DataView/onNextRowClick";
import { onPrevRowClick } from "model/actions-ui/DataView/onPrevRowClick";
import { onRecordAuditClick } from "model/actions-ui/RecordInfo/onRecordAuditClick";
import { onRecordInfoClick } from "model/actions-ui/RecordInfo/onRecordInfoClick";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";
import { getActivePanelView } from "model/selectors/DataView/getActivePanelView";
import { getDataViewLabel } from "model/selectors/DataView/getDataViewLabel";
import { getIsAddButtonVisible } from "model/selectors/DataView/getIsAddButtonVisible";
import { getIsCopyButtonVisible } from "model/selectors/DataView/getIsCopyButtonVisible";
import { getIsDelButtonVisible } from "model/selectors/DataView/getIsDelButtonVisible";
import { getMaxRowCountSeen } from "model/selectors/DataView/getMaxRowCountSeen";
import { getPanelViewActions } from "model/selectors/DataView/getPanelViewActions";
import { getSelectedRowIndex } from "model/selectors/DataView/getSelectedRowIndex";
import { getIsFilterControlsDisplayed } from "model/selectors/TablePanelView/getIsFilterControlsDisplayed";
import { SectionViewSwitchers } from "modules/DataView/DataViewTypes";
import { IDataViewToolbarUI } from "modules/DataView/DataViewUI";
import React from "react";



@observer
export class CDataViewHeader extends React.Component<{ isVisible: boolean }> {
  static contextType = MobXProviderContext;

  get dataView() {
    return this.context.dataView;
  }

  render() {
    const { dataView } = this;
    const selectedRowIndex = getSelectedRowIndex(dataView);
    const maxRowCountSeen = getMaxRowCountSeen(dataView);
    const activePanelView = getActivePanelView(dataView);
    const label = getDataViewLabel(dataView);
    const isFilterSettingsVisible = getIsFilterControlsDisplayed(dataView);
    const actions = getPanelViewActions(dataView);
    const onColumnConfigurationClickEvt = onColumnConfigurationClick(dataView);
    const onDeleteRowClickEvt = onDeleteRowClick(dataView);
    const onCreateRowClickEvt = onCreateRowClick(dataView);
    const onFilterButtonClickEvt = onFilterButtonClick(dataView);
    const onPrevRowClickEvt = onPrevRowClick(dataView);
    const onNextRowClickEvt = onNextRowClick(dataView);

    const isAddButton = getIsAddButtonVisible(dataView);
    const isDelButton = getIsDelButtonVisible(dataView);
    const isCopyButton = getIsCopyButtonVisible(dataView);

    const $cont = scopeFor(dataView);
    const uiToolbar = $cont && $cont.resolve(IDataViewToolbarUI);

    return (
      <DataViewHeader isVisible={this.props.isVisible}>
        {this.props.isVisible && (
          <>
            <span>
              <h2>{label}</h2>
            </span>

            <DataViewHeaderGroup>
              {isAddButton && (
                <DataViewHeaderAction
                  className="isGreenHover"
                  onClick={onCreateRowClickEvt}
                >
                  <Icon src="./icons/add.svg" />
                </DataViewHeaderAction>
              )}

              {isDelButton && (
                <DataViewHeaderAction
                  className="isRedHover"
                  onClick={onDeleteRowClickEvt}
                >
                  <Icon src="./icons/minus.svg" />
                </DataViewHeaderAction>
              )}

              {isCopyButton && (
                <DataViewHeaderAction
                  className="isOrangeHover"
                  onClick={undefined}
                >
                  <Icon src="./icons/duplicate.svg" />
                </DataViewHeaderAction>
              )}
            </DataViewHeaderGroup>
            <DataViewHeaderButtonGroup>
              {actions
                .filter(action => getIsEnabledAction(action))
                .map(action => (
                  <DataViewHeaderButton
                    onClick={event =>
                      uiActions.actions.onActionClick(action)(event, action)
                    }
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
                &nbsp;/&nbsp;{maxRowCountSeen}
              </span>
            </DataViewHeaderGroup>
            <DataViewHeaderGroup>
              
              {uiToolbar && uiToolbar.renderSection(SectionViewSwitchers)}
              
            </DataViewHeaderGroup>
            <DataViewHeaderGroup>
              <DataViewHeaderAction
                onClick={onFilterButtonClickEvt}
                isActive={isFilterSettingsVisible}
                className={"test-filter-button"}
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
                    <Icon src="./icons/dot-menu.svg" />
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
                    <DropdownItem
                      isDisabled={false}
                      onClick={(event: any) => {
                        setDropped(false);
                        onRecordAuditClick(dataView)(event);
                      }}
                    >
                      Show audit
                    </DropdownItem>
                    <DropdownItem isDisabled={true}>
                      Show attachments
                    </DropdownItem>
                    <DropdownItem
                      isDisabled={false}
                      onClick={(event: any) => {
                        setDropped(false);
                        onRecordInfoClick(dataView)(event);
                      }}
                    >
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
