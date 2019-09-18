import React from "react";
import { observer, inject, Provider } from "mobx-react";
import S from "./DataViewToolbar.module.css";
import { IDataView } from "../../../model/entities/types/IDataView";
import { IPanelViewType } from "../../../model/entities/types/IPanelViewType";
import { getActivePanelView } from "../../../model/selectors/DataView/getActivePanelView";
import { getDataViewLabel } from "../../../model/selectors/DataView/getDataViewLabel";
import { action, observable } from "mobx";
import { getTablePanelView } from "../../../model/selectors/TablePanelView/getTablePanelView";
import { IAction } from "../../../model/entities/types/IAction";
import { getPanelViewActions } from "model/selectors/DataView/getPanelViewActions";
import { onAddRowClick } from "model/actions/DataView/onAddRowClick";
import { onDeleteRowClick } from "../../../model/actions/DataView/onDeleteRowClick";
import { onNextRowClick } from "model/actions/DataView/onNextRowClick";
import { onPrevRowClick } from "model/actions/DataView/onPrevRowClick";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";
import { onActionClick } from "model/actions/Actions/onActionClick";

@inject(({ dataView }: { dataView: IDataView }) => {
  const ctx = dataView;
  return {
    activePanelView: getActivePanelView(dataView),
    label: getDataViewLabel(dataView),
    isFilterSettingsVisible:
      dataView.tablePanelView.filterConfiguration.isFilterControlsDisplayed,
    actions: getPanelViewActions(dataView),
    onFormViewButtonClick: dataView.onFormPanelViewButtonClick,
    onTableViewButtonClick: dataView.onTablePanelViewButtonClick,
    onColumnConfClick: getTablePanelView(dataView).columnConfigurationDialog
      .onColumnConfClick,
    onDeleteRowClick: onDeleteRowClick(dataView),
    onCreateRowClick: onAddRowClick(dataView),
    onToggleFilterClick:
      dataView.tablePanelView.filterConfiguration.onFilterDisplayClick,
    onPrevRowClick: onPrevRowClick(ctx),
    onNextRowClick: onNextRowClick(ctx)
  };
})
@observer
export class Toolbar extends React.Component<{
  activePanelView?: IPanelViewType;
  label?: string;
  isFilterSettingsVisible?: boolean;
  actions?: IAction[];
  onFormViewButtonClick?: (event: any) => void;
  onTableViewButtonClick?: (event: any) => void;
  onColumnConfClick?: (event: any) => void;
  onDeleteRowClick?: (event: any) => void;
  onCreateRowClick?: (event: any) => void;
  onToggleFilterClick?: (event: any) => void;
  onPrevRowClick?: (event: any) => void;
  onNextRowClick?: (event: any) => void;
}> {
  render() {
    return (
      <div className={S.dataViewToolbar}>
        <div className={S.section}>
          {false && (
            <div className={S.loading}>
              <i className="fas fa-sync-alt fa-spin" />
            </div>
          )}
          {false && <i className="fas fa-exclamation-circle red" />}
          <div className={S.label}>{this.props.label}</div>
          {false && <i className="fas fa-filter red" />}
        </div>
        <div className={S.section}>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-chevron-circle-up" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-chevron-circle-down" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className={S.section}>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={this.props.onCreateRowClick}
          >
            <i className="fas fa-plus-circle" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={this.props.onDeleteRowClick}
          >
            <i className="fas fa-minus-circle" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-copy" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className={S.section}>
          {this.props.actions!.map(action => (
            <PanelViewAction
              disabled={!getIsEnabledAction(action)}
              onClick={event => onActionClick(action)(event, action)}
            >
              {action.caption}
            </PanelViewAction>
          ))}
        </div>
        <div className={S.section + " " + S.pusher} />
        <div className={S.section}>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-step-backward" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={this.props.onPrevRowClick}
          >
            <i className="fas fa-caret-left" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={this.props.onNextRowClick}
          >
            <i className="fas fa-caret-right" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={false}
            isEnabled={true}
            onClick={undefined}
          >
            <i className="fas fa-step-forward" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className={S.section}>
          <span>
            {0} / {0}
          </span>
        </div>
        <div className={S.section}>
          <ToolbarButton
            isVisible={true}
            isActive={this.props.activePanelView === IPanelViewType.Table}
            isEnabled={true}
            onClick={this.props.onTableViewButtonClick}
          >
            <i className="fa fa-table" aria-hidden="true" />
          </ToolbarButton>
          <ToolbarButton
            isVisible={true}
            isActive={this.props.activePanelView === IPanelViewType.Form}
            isEnabled={true}
            onClick={this.props.onFormViewButtonClick}
          >
            <i className="fas fa-list-alt" aria-hidden="true" />
          </ToolbarButton>
        </div>
        <div className={S.section}>
          <ToolbarButton
            isVisible={true}
            isActive={!!this.props.isFilterSettingsVisible}
            isEnabled={true}
            onClick={this.props.onToggleFilterClick}
          >
            <i className="fas fa-filter" aria-hidden="true" />
          </ToolbarButton>

          <div className={S.dropDownMenuRoot}>
            <ToolbarDropDownMenu
              trigger={onTriggerClick => (
                <ToolbarButton
                  isVisible={true}
                  isActive={false}
                  isEnabled={true}
                  onClick={onTriggerClick}
                >
                  <i className="fas fa-caret-down" aria-hidden="true" />
                </ToolbarButton>
              )}
            >
              <ToolbarDropDownMenuItem onClick={this.props.onToggleFilterClick}>
                {this.props.isFilterSettingsVisible
                  ? "Cancel and Hide Filter"
                  : "Show Filter"}
              </ToolbarDropDownMenuItem>
              <ToolbarDropDownMenuItem isDisabled={true}>
                Remember The Current Filter
              </ToolbarDropDownMenuItem>
              <ToolbarDropDownMenuItem isDisabled={true}>
                Cancel Default Filter
              </ToolbarDropDownMenuItem>
              <div className={S.dropDownMenuDivider} />
              <ToolbarDropDownMenuItem isDisabled={true}>
                Save Current Filter
              </ToolbarDropDownMenuItem>
              <ToolbarDropDownMenuItem isDisabled={true}>
                Delete
              </ToolbarDropDownMenuItem>
              <ToolbarDropDownMenuItem isDisabled={true}>
                Cancel Filter
              </ToolbarDropDownMenuItem>
            </ToolbarDropDownMenu>
          </div>
        </div>
        <div className={S.section}>
          <div className={S.dropDownMenuRoot}>
            <ToolbarDropDownMenu
              trigger={onTriggerClick => (
                <ToolbarButton
                  isVisible={true}
                  isActive={false}
                  isEnabled={true}
                  onClick={onTriggerClick}
                >
                  <i className="fas fa-cog" aria-hidden="true" />
                  <i className="fas fa-caret-down" aria-hidden="true" />
                </ToolbarButton>
              )}
            >
              <ToolbarDropDownMenuItem isDisabled={true}>
                <div className={S.dropDownItemIcon}>
                  <i className="far fa-file-excel" />
                </div>
                Export to Excel
              </ToolbarDropDownMenuItem>
              {this.props.activePanelView === IPanelViewType.Table && (
                <ToolbarDropDownMenuItem onClick={this.props.onColumnConfClick}>
                  <div className={S.dropDownItemIcon}>
                    <i className="fas fa-cog" aria-hidden="true" />
                  </div>
                  Column Configuration
                </ToolbarDropDownMenuItem>
              )}
              <ToolbarDropDownMenuItem isDisabled={true}>
                <div className={S.dropDownItemIcon}>
                  <i className="fas fa-shield-alt" />
                </div>
                Show Audit
              </ToolbarDropDownMenuItem>
              <ToolbarDropDownMenuItem isDisabled={true}>
                <div className={S.dropDownItemIcon}>
                  <i className="fas fa-paperclip" />
                </div>
                Show Attachments
              </ToolbarDropDownMenuItem>
              <ToolbarDropDownMenuItem isDisabled={true}>
                <div className={S.dropDownItemIcon}>
                  <i className="fas fa-info-circle" />
                </div>
                Show RecordInformation
              </ToolbarDropDownMenuItem>
            </ToolbarDropDownMenu>
          </div>
        </div>
      </div>
    );
  }
}

@observer
export class ToolbarButton extends React.Component<{
  isActive: boolean;
  isEnabled: boolean;
  isVisible: boolean;
  onClick?: (event: any) => void;
}> {
  render() {
    const { isActive, isEnabled, isVisible, onClick } = this.props;
    return isVisible ? (
      <button
        className={
          S.toolbarBtn +
          (isActive ? " active" : "") +
          (isEnabled ? "" : " disabled")
        }
        onClick={onClick}
      >
        {this.props.children}
      </button>
    ) : null;
  }
}

@observer
export class ToolbarDropDownMenu extends React.Component<{
  trigger?: (onTriggerClick: (event: any) => void) => React.ReactNode;
}> {
  @observable isDropped = false;

  @action.bound handleTriggerClick(event: any) {
    this.setDroppedDown(true);
  }

  @action.bound handleWindowMouseDown(event: any) {
    if (!this.refMenu.current!.contains(event.target)) {
      this.setDroppedDown(false);
    }
  }

  @action.bound setDroppedDown(state: boolean) {
    this.isDropped = state;
    if (state) {
      window.addEventListener("mousedown", this.handleWindowMouseDown, true);
    } else {
      window.removeEventListener("mousedown", this.handleWindowMouseDown, true);
    }
  }

  refMenu = React.createRef<HTMLDivElement>();

  render() {
    return (
      <Provider dropDownMenu={this}>
        <>
          {this.props.trigger && this.props.trigger(this.handleTriggerClick)}
          {this.isDropped && (
            <div ref={this.refMenu} className={S.dropDownMenu}>
              {this.props.children}
            </div>
          )}
        </>
      </Provider>
    );
  }
}

@inject("dropDownMenu")
@observer
export class ToolbarDropDownMenuItem extends React.Component<{
  isDisabled?: boolean;
  onClick?: (event: any) => void;
  dropDownMenu?: ToolbarDropDownMenu;
}> {
  @action.bound handleClick(event: any) {
    if (!this.props.isDisabled) {
      this.props.onClick && this.props.onClick(event);
      this.props.dropDownMenu!.setDroppedDown(false);
    }
  }

  render() {
    return (
      <div
        onClick={this.handleClick}
        className={
          S.dropDownMenuItem + (this.props.isDisabled ? " disabled" : "")
        }
      >
        {this.props.children}
      </div>
    );
  }
}

const PanelViewAction: React.FC<{
  disabled?: boolean;
  onClick?: (event: any) => void;
}> = props => (
  <div
    onClick={props.onClick}
    className={S.panelViewAction + (props.disabled ? " disabled" : "")}
  >
    {props.children}
  </div>
);
