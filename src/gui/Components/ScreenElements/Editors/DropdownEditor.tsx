import * as React from "react";
import {inject, Observer, observer} from "mobx-react";
import {action, computed, observable, runInAction} from "mobx";
import S from "./DropdownEditor.module.scss";
import {Tooltip} from "react-tippy";
import cx from "classnames";

import _ from "lodash";
import {AutoSizer, MultiGrid} from "react-virtualized";
import Highlighter from "react-highlight-words";
import {IApi} from "../../../../model/entities/types/IApi";
import {getApi} from "../../../../model/selectors/getApi";
import {getDataTable} from "../../../../model/selectors/DataView/getDataTable";
import {getDataStructureEntityId} from "../../../../model/selectors/DataView/getDataStructureEntityId";
import {IProperty} from "../../../../model/entities/types/IProperty";
import {getSelectedRowId} from "../../../../model/selectors/TablePanelView/getSelectedRowId";
import {getMenuItemId} from "../../../../model/selectors/getMenuItemId";
import {Dropdowner} from "gui/Components/Dropdowner/Dropdowner";
import {getEntity} from "../../../../model/selectors/DataView/getEntity";
import {getSessionId} from "model/selectors/getSessionId";

export interface IDropdownEditorProps {
  value: string | null;
  textualValue?: string;
  isReadOnly: boolean;
  isInvalid: boolean;
  invalidMessage?: string;
  isFocused: boolean;
  foregroundColor?: string;
  backgroundColor?: string;

  Entity?: string;
  SessionFormIdentifier?: string;
  DataStructureEntityId?: string;
  ColumnNames?: string[];
  Property?: string;
  Identifier?: string;
  RowId?: string;
  LookupId?: string;
  Parameters?: { [key: string]: any };
  menuItemId?: string;

  refocuser?: (cb: () => void) => () => void;
  onTextChange?(event: any, value: string): void;
  onItemSelect?(event: any, value: string | null): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  api?: IApi;

  onEditorBlur?(event: any): void;
}

// TODO: Change connection for FormView - e.g. RowId may be found differently for different panel views.
@inject(({ property }: { property: IProperty }, { value }) => {
  const dataTable = getDataTable(property);
  const lookup = property.lookup!;
  return {
    api: getApi(property),
    textualValue: dataTable.resolveCellText(property, value),
    DataStructureEntityId: getDataStructureEntityId(property),
    ColumnNames: lookup.dropDownColumns.map(column => column.id),
    Property: property.id,
    Parameters: lookup.parameters,
    RowId: getSelectedRowId(property),
    Identifier: property.identifier,
    LookupId: lookup.lookupId,
    menuItemId: getMenuItemId(property),
    Entity: getEntity(property),
    SessionFormIdentifier: getSessionId(property)
  };
})
@observer
export class DropdownEditor extends React.Component<IDropdownEditorProps> {
  constructor(props: IDropdownEditorProps) {
    super(props);
  }

  disposers: any[] = [];

  @observable isDroppedDown = false;
  @observable dirtyTextualValue: string | undefined;
  @observable.ref lookupItems: any[] = [];
  @observable willReload = false;
  @observable isLoading = false;
  @observable wasTextEdited = false;
  @observable initialTextValue = "";

  componentDidMount() {
    runInAction(() => {
      this.initialTextValue = this.props.textualValue || "";
      this.props.refocuser &&
        this.disposers.push(this.props.refocuser(this.makeFocusedIfNeeded));
      this.makeFocusedIfNeeded();
      this.selectedItemId = this.props.value || undefined;
    });
  }

  componentWillUnmount() {
    this.disposers.forEach(d => d());
  }

  componentDidUpdate(prevProps: {
    isFocused: boolean;
    textualValue?: string;
    value: string | null;
  }) {
    runInAction(() => {
      if (!prevProps.isFocused && this.props.isFocused) {
        this.makeFocusedIfNeeded();
      }
      if (prevProps.textualValue !== this.props.textualValue) {
        this.dirtyTextualValue = undefined;
        this.initialTextValue = this.props.textualValue || "";
        this.makeFocusedIfNeeded();
      }
      if (prevProps.value !== null && this.props.value === null) {
        this.dirtyTextualValue = "";
        this.lookupItems = [];
      }
      if (prevProps.value !== this.props.value) {
        this.selectedItemId = this.props.value || undefined;
      }
    });
  }

  @action.bound
  makeFocusedIfNeeded() {
    if (this.props.isFocused) {
      console.log("--- MAKE FOCUSED ---");
      this.elmInput && this.elmInput.focus();
      setTimeout(() => {
        if (this.elmInput) {
          this.elmInput.select();
          this.elmInput.scrollLeft = 0;
        }
      }, 10);
    }
  }

  @action.bound
  handleFocus(event: any) {
    setTimeout(() => {
      if (this.elmInput) {
        this.elmInput.select();
        this.elmInput.scrollLeft = 0;
      }
    }, 10);
  }

  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };

  elmDropdowner: Dropdowner | null = null;
  refDropdowner = (elm: Dropdowner | null) => (this.elmDropdowner = elm);

  get isDropped() {
    return this.elmDropdowner && this.elmDropdowner.isDropped;
  }

  @action.bound
  handleTextChange(event: any) {
    this.wasTextEdited = true;
    this.dirtyTextualValue = event.target.value;
    if (this.dirtyTextualValue !== "") {
      this.elmDropdowner && this.elmDropdowner.setDropped(true);
      this.loadItems();
    }
  }

  @action.bound loadItems() {
    this.willReload = true;
    this.loadItemsDebounced();
  }

  loadItemsDebounced = _.debounce(this.loadItemsImmediately, 300);

  @action.bound loadItemsImmediately() {
    if (!this.api) {
      return;
    }
    this.lookupItems = [];
    this.willReload = false;
    this.isLoading = true;
    this.api
      .getLookupList({
        Entity: this.props.Entity,
        SessionFormIdentifier: this.props.SessionFormIdentifier,
        DataStructureEntityId: this.props.DataStructureEntityId, // Data view entity identifier
        ColumnNames: [this.props.Identifier || "Id", ...this.props.ColumnNames], // Columns to download
        Property: this.props.Property!, // Columnn Id
        Id: this.props.RowId!, // Id of the selected row
        LookupId: this.props.LookupId!, // Id of the lookup object
        Parameters: this.props.Parameters!,
        MenuId: this.props.menuItemId!,
        ShowUniqueValues: false,
        SearchText:
          this.dirtyTextualValue !== undefined ? this.dirtyTextualValue : "",
        PageSize: 10000,
        PageNumber: 1
      })
      .then(
        action((lookupItems: any) => {
          console.log("Loaded lookup list items:", lookupItems);
          this.lookupItems = lookupItems;
          this.isLoading = false;
        })
      )
      .catch(
        action((error: any) => {
          this.isLoading = false;
        })
      );
  }

  @action.bound handleDroppedDown() {
    this.lookupItems = [];
    this.loadItems();
  }

  @computed get value() {
    return this.dirtyTextualValue !== undefined
      ? this.dirtyTextualValue
      : this.props.textualValue;
  }

  @action.bound handleContainerMouseDown(event: any) {
    event.preventDefault();
    this.elmInput && this.elmInput.focus();
  }

  @action.bound handleInputBlur(event: any) {
    if (this.value === "" && this.initialTextValue !== "") {
      this.elmDropdowner && this.elmDropdowner.setDropped(false);
      this.props.onItemSelect && this.props.onItemSelect(event, null);
      return;
    }
    this.elmDropdowner && this.elmDropdowner.setDropped(false);
    this.props.onEditorBlur && this.props.onEditorBlur(event);
  }

  cellRenderer = (args: {
    rowIndex: number;
    columnIndex: number;
    key: string;
    style: any;
  }) => {
    const handleClick = action((event: any) => {
      // this.dirtyTextualValue = this.lookupItems[args.rowIndex - 1][1];
      if (args.rowIndex > 0) {
        this.dirtyTextualValue = undefined;
        this.props.onItemSelect &&
          this.props.onItemSelect(
            event,
            this.lookupItems[args.rowIndex - 1][0]
          );
        this.makeFocusedIfNeeded();
        this.elmDropdowner && this.elmDropdowner.setDropped(false);
      }
    });
    return (
      <Observer>
        {() => (
          <div
            style={args.style}
            className={cx(
              args.rowIndex === 0
                ? S.lookupListHeaderCell
                : S.lookupListItemCell,
              args.rowIndex % 2 === 0 ? S.evenItem : S.oddItem,
              {
                selected:
                  args.rowIndex > 0 &&
                  this.lookupItems[args.rowIndex - 1][0] ===
                    this.selectedItemId,
                choosen:
                  args.rowIndex > 0 &&
                  this.lookupItems[args.rowIndex - 1][0] === this.props.value
              }
            )}
            onClick={handleClick}
          >
            {args.rowIndex === 0 ? (
              this.props.ColumnNames![args.columnIndex]
            ) : (
              <Highlighter
                textToHighlight={
                  this.lookupItems[args.rowIndex - 1][args.columnIndex + 1]
                }
                searchWords={
                  [this.dirtyTextualValue].filter(item => item) as string[]
                }
                autoEscape={true}
              />
            )}
          </div>
        )}
      </Observer>
    );
  };

  @observable selectedItemId: string | undefined;

  @computed get selectedItemValue(): string | undefined {
    return;
  }

  findNextId() {
    const idx = this.lookupItems.findIndex(
      item => item[0] === this.selectedItemId
    );
    const newIndex =
      idx > -1 ? Math.min(idx + 1, this.lookupItems.length - 1) : undefined;
    const newId =
      newIndex !== undefined ? this.lookupItems[newIndex][0] : undefined;
    return newId;
  }

  findPrevId() {
    const idx = this.lookupItems.findIndex(
      item => item[0] === this.selectedItemId
    );
    const newIndex = idx > -1 ? Math.max(idx - 1, 0) : undefined;
    const newId =
      newIndex !== undefined ? this.lookupItems[newIndex][0] : undefined;
    return newId;
  }

  findFirstId() {
    const newId =
      this.lookupItems.length > 0 ? this.lookupItems[0][0] : undefined;
    return newId;
  }

  @action.bound handleKeyDown(event: any) {
    if (this.isDropped) {
      switch (event.key) {
        case "Escape":
          this.dirtyTextualValue = undefined;
          this.initialTextValue = this.props.textualValue || "";
          this.elmDropdowner && this.elmDropdowner.setDropped(false);
          break;
        case "ArrowUp":
          if (!this.selectedItemId) {
            this.selectedItemId = this.findFirstId();
          } else {
            this.selectedItemId = this.findPrevId();
          }
          break;
        case "ArrowDown":
          if (!this.selectedItemId) {
            this.selectedItemId = this.findFirstId();
          } else {
            this.selectedItemId = this.findNextId();
          }
          break;
        case "Enter":
        case "Tab":
          if (this.selectedItemId) {
            this.dirtyTextualValue = undefined;
            this.props.onItemSelect &&
              this.props.onItemSelect(event, this.selectedItemId);
            this.makeFocusedIfNeeded();
            this.elmDropdowner && this.elmDropdowner.setDropped(false);
            break;
          }
      }
    } else {
      switch (event.key) {
        case "ArrowDown":
          if (event.altKey) {
            this.elmDropdowner && this.elmDropdowner.setDropped(true);
          }
          break;
        case "Enter":
          if (this.dirtyTextualValue === "") {
            this.props.onItemSelect && this.props.onItemSelect(event, null);
          }
          break;
      }
    }
    this.props.onKeyDown && this.props.onKeyDown(event);
  }

  render() {
    return (
      <Dropdowner
        className={S.dropdownerContainer}
        ref={this.refDropdowner}
        onDroppedDown={this.handleDroppedDown}
        onContainerMouseDown={this.handleContainerMouseDown}
        trigger={({ refTrigger, setDropped }) => (
          <div
            className={S.editorContainer}
            ref={refTrigger}
            style={{
              zIndex: this.isDroppedDown ? 1000 : undefined
            }}
          >
            <input
              style={{
                color: this.props.foregroundColor,
                backgroundColor: this.props.backgroundColor
              }}
              className={S.input}
              type="text"
              value={this.value}
              readOnly={this.props.isReadOnly}
              ref={this.refInput}
              onChange={this.handleTextChange}
              onKeyDown={this.handleKeyDown}
              onClick={this.props.onClick}
              onBlur={this.handleInputBlur}
              onFocus={this.handleFocus}
            />
            {this.props.isInvalid && (
              <div className={S.notification}>
                <Tooltip html={this.props.invalidMessage} arrow={true}>
                  <i className="fas fa-exclamation-circle red" />
                </Tooltip>
              </div>
            )}
            {!this.props.isReadOnly && (
              <div
                className={S.dropdownSymbol}
                onClick={() => setDropped(true)}
              >
                {!(this.willReload || this.isLoading) && (
                  <i className="fas fa-caret-down" />
                )}
                {(this.willReload || this.isLoading) && (
                  <i
                    className={
                      "fas fa-sync" + (this.isLoading ? " fa-spin" : "")
                    }
                  />
                )}
              </div>
            )}
          </div>
        )}
        content={({}) => (
          <div
            className={S.droppedPanelContainer}
            style={{
              width: Math.min(200, 200 * this.props.ColumnNames!.length),
              height: Math.min(300, 20 * (this.lookupItems.length + 1))
            }}
          >
            <AutoSizer>
              {({ width, height }) => (
                <Observer>
                  {() => (
                    <MultiGrid
                      fixedRowCount={1}
                      width={width}
                      height={height}
                      rowCount={this.lookupItems.length + 1}
                      columnCount={this.props.ColumnNames!.length}
                      rowHeight={20}
                      columnWidth={200}
                      cellRenderer={this.cellRenderer}
                    />
                  )}
                </Observer>
              )}
            </AutoSizer>
          </div>
        )}
      />
    );
  }

  get api() {
    return this.props.api!;
  }
}
