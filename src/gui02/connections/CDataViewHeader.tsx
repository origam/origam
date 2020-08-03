import {scopeFor} from "dic/Container";
import {Dropdowner} from "gui/Components/Dropdowner/Dropdowner";
import {DataViewHeader} from "gui02/components/DataViewHeader/DataViewHeader";
import {DataViewHeaderAction} from "gui02/components/DataViewHeader/DataViewHeaderAction";
import {DataViewHeaderButton} from "gui02/components/DataViewHeader/DataViewHeaderButton";
import {DataViewHeaderButtonGroup} from "gui02/components/DataViewHeader/DataViewHeaderButtonGroup";
import {DataViewHeaderGroup} from "gui02/components/DataViewHeader/DataViewHeaderGroup";
import {Dropdown} from "gui02/components/Dropdown/Dropdown";
import {DropdownItem} from "gui02/components/Dropdown/DropdownItem";
import {Icon} from "gui02/components/Icon/Icon";
import {MobXProviderContext, observer} from "mobx-react";
import uiActions from "model/actions-ui-tree";
import {onColumnConfigurationClick} from "model/actions-ui/DataView/onColumnConfigurationClick";
import {onCopyRowClick, onCreateRowClick} from "model/actions-ui/DataView/onCreateRowClick";
import {onDeleteRowClick} from "model/actions-ui/DataView/onDeleteRowClick";
import {onFilterButtonClick} from "model/actions-ui/DataView/onFilterButtonClick";
import {onNextRowClick} from "model/actions-ui/DataView/onNextRowClick";
import {onPrevRowClick} from "model/actions-ui/DataView/onPrevRowClick";
import {onRecordAuditClick} from "model/actions-ui/RecordInfo/onRecordAuditClick";
import {onRecordInfoClick} from "model/actions-ui/RecordInfo/onRecordInfoClick";
import {getIsEnabledAction} from "model/selectors/Actions/getIsEnabledAction";
import {getDataViewLabel} from "model/selectors/DataView/getDataViewLabel";
import {getIsAddButtonVisible} from "model/selectors/DataView/getIsAddButtonVisible";
import {getIsCopyButtonVisible} from "model/selectors/DataView/getIsCopyButtonVisible";
import {getIsDelButtonVisible} from "model/selectors/DataView/getIsDelButtonVisible";
import {getMaxRowCountSeen} from "model/selectors/DataView/getMaxRowCountSeen";
import {getPanelViewActions} from "model/selectors/DataView/getPanelViewActions";
import {getSelectedRowIndex} from "model/selectors/DataView/getSelectedRowIndex";
import {getIsFilterControlsDisplayed} from "model/selectors/TablePanelView/getIsFilterControlsDisplayed";
import {SectionViewSwitchers} from "modules/DataView/DataViewTypes";
import {IDataViewToolbarUI} from "modules/DataView/DataViewUI";
import React from "react";
import {
  CtxResponsiveToolbar,
  ResponsiveBlock,
  ResponsiveChild,
  ResponsiveContainer,
} from "gui02/components/ResponsiveBlock/ResponsiveBlock";
import {onFirstRowClick} from "../../model/actions-ui/DataView/onFirstRowClick";
import {onLastRowClick} from "../../model/actions-ui/DataView/onLastRowClick";

@observer
export class CDataViewHeader extends React.Component<{ isVisible: boolean }> {
  static contextType = MobXProviderContext;

  get dataView() {
    return this.context.dataView;
  }

  state = {
    hiddenActionIds: new Set<string>(),
  };
  responsiveToolbar = new ResponsiveBlock((ids) => {
    this.setState({ ...this.state, hiddenActionIds: ids });
  });

  render() {
    const { dataView } = this;
    const selectedRowIndex = getSelectedRowIndex(dataView);
    const maxRowCountSeen = getMaxRowCountSeen(dataView);
    const label = getDataViewLabel(dataView);
    const isFilterSettingsVisible = getIsFilterControlsDisplayed(dataView);
    const actions = getPanelViewActions(dataView);
    const onColumnConfigurationClickEvt = onColumnConfigurationClick(dataView);
    const onDeleteRowClickEvt = onDeleteRowClick(dataView);
    const onCreateRowClickEvt = onCreateRowClick(dataView);
    const onCopyRowClickEvt = onCopyRowClick(dataView);
    const onFilterButtonClickEvt = onFilterButtonClick(dataView);
    const onFirstRowClickEvt = onFirstRowClick(dataView);
    const onPrevRowClickEvt = onPrevRowClick(dataView);
    const onNextRowClickEvt = onNextRowClick(dataView);
    const onLastRowClickEvt = onLastRowClick(dataView);

    const isAddButton = getIsAddButtonVisible(dataView);
    const isDelButton = getIsDelButtonVisible(dataView);
    const isCopyButton = getIsCopyButtonVisible(dataView);

    const $cont = scopeFor(dataView);
    const uiToolbar = $cont && $cont.resolve(IDataViewToolbarUI);

    return (
      <CtxResponsiveToolbar.Provider value={this.responsiveToolbar}>
        <DataViewHeader isVisible={this.props.isVisible}>
          {this.props.isVisible && (
            <>
              <span>
                <h2>{label}</h2>
              </span>
              <ResponsiveContainer compensate={50}>
                {({ refChild }) => (
                  <div className="fullspaceBlock" ref={refChild}>
                    <ResponsiveChild childKey={"add-del-cpy"} order={1}>
                      {({ refChild, isHidden }) => (
                        <DataViewHeaderGroup domRef={refChild} isHidden={isHidden}>
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
                            <DataViewHeaderAction className="isOrangeHover" onClick={onCopyRowClickEvt}>
                              <Icon src="./icons/duplicate.svg" />
                            </DataViewHeaderAction>
                          )}
                        </DataViewHeaderGroup>
                      )}
                    </ResponsiveChild>
                    <DataViewHeaderGroup grovable={true}>
                      <DataViewHeaderButtonGroup>
                        {actions
                          .filter((action) => getIsEnabledAction(action))
                          .map((action) => (
                            <DataViewHeaderButton
                              onClick={(event) =>
                                uiActions.actions.onActionClick(action)(event, action)
                              }
                            >
                              {action.caption}
                            </DataViewHeaderButton>
                          ))}
                        {/*<ResponsiveChild childKey={"action-1"} order={20}>
                          {({ refChild, isHidden }) => (
                            <DataViewHeaderButton
                              domRef={refChild}
                              isHidden={isHidden}
                              onClick={undefined}
                            >
                              Action 1
                            </DataViewHeaderButton>
                          )}
                        </ResponsiveChild>
                        <ResponsiveChild childKey={"action-2"} order={20}>
                          {({ refChild, isHidden }) => (
                            <DataViewHeaderButton
                              domRef={refChild}
                              isHidden={isHidden}
                              onClick={undefined}
                            >
                              Action 2
                            </DataViewHeaderButton>
                          )}
                        </ResponsiveChild>
                        <ResponsiveChild childKey={"action-3"} order={20}>
                          {({ refChild, isHidden }) => (
                            <DataViewHeaderButton
                              domRef={refChild}
                              isHidden={isHidden}
                              onClick={undefined}
                            >
                              Action 3
                            </DataViewHeaderButton>
                          )}
                        </ResponsiveChild>
                        <ResponsiveChild childKey={"action-4"} order={20}>
                          {({ refChild, isHidden }) => (
                            <DataViewHeaderButton
                              domRef={refChild}
                              isHidden={isHidden}
                              onClick={undefined}
                            >
                              Action 4
                            </DataViewHeaderButton>
                          )}
                          </ResponsiveChild>*/}
                      </DataViewHeaderButtonGroup>
                    </DataViewHeaderGroup>
                    <ResponsiveChild childKey={"cursor-move"} order={5}>
                      {({ refChild, isHidden }) => (
                        <DataViewHeaderGroup domRef={refChild} isHidden={isHidden}>
                          <DataViewHeaderAction onClick={onFirstRowClickEvt}>
                            <Icon src="./icons/list-arrow-first.svg" />
                          </DataViewHeaderAction>
                          <DataViewHeaderAction onClick={onPrevRowClickEvt}>
                            <Icon src="./icons/list-arrow-previous.svg" />
                          </DataViewHeaderAction>
                          <DataViewHeaderAction onClick={onNextRowClickEvt}>
                            <Icon src="./icons/list-arrow-next.svg" />
                          </DataViewHeaderAction>
                          <DataViewHeaderAction onClick={onLastRowClickEvt}>
                            <Icon src="./icons/list-arrow-last.svg" />
                          </DataViewHeaderAction>
                        </DataViewHeaderGroup>
                      )}
                    </ResponsiveChild>
                    <ResponsiveChild childKey={"row-cnt-label"} order={10}>
                      {({ refChild, isHidden }) => (
                        <DataViewHeaderGroup domRef={refChild} isHidden={isHidden}>
                          {selectedRowIndex !== undefined ? selectedRowIndex + 1 : " - "}
                          &nbsp;/&nbsp;
                          {maxRowCountSeen}
                        </DataViewHeaderGroup>
                      )}
                    </ResponsiveChild>
                    <ResponsiveChild childKey={"view-switchers"} order={1}>
                      {({ refChild, isHidden }) => (
                        <DataViewHeaderGroup domRef={refChild} isHidden={isHidden}>
                          {uiToolbar && uiToolbar.renderSection(SectionViewSwitchers)}
                        </DataViewHeaderGroup>
                      )}
                    </ResponsiveChild>
                    <ResponsiveChild childKey={"filter-control"} order={1}>
                      {({ refChild, isHidden }) => (
                        <DataViewHeaderGroup domRef={refChild} isHidden={isHidden}>
                          <DataViewHeaderAction
                            onClick={onFilterButtonClickEvt}
                            isActive={isFilterSettingsVisible}
                            className={"test-filter-button"}
                          >
                            <Icon src="./icons/search-filter.svg" />
                          </DataViewHeaderAction>
                        </DataViewHeaderGroup>
                      )}
                    </ResponsiveChild>
                  </div>
                )}
              </ResponsiveContainer>

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
                      <DropdownItem isDisabled={true}>Export to Excel</DropdownItem>
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
                      <DropdownItem isDisabled={true}>Show attachments</DropdownItem>
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
      </CtxResponsiveToolbar.Provider>
    );
  }
}
