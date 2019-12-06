import React, { useImperativeHandle } from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";
import S from "./FilterSettingsLookup.module.scss";

import { observable, computed, action, flow, runInAction } from "mobx";
import { observer, PropTypes } from "mobx-react";

import { Dropdowner } from "../../../../Dropdowner/Dropdowner";

import { Grid, GridCellProps } from "react-virtualized";
import {
  TagInput,
  TagInputItem,
  TagInputDeleteBtn,
  TagInputPlus,
  TagInputEdit,
  TagInputEditFake
} from "gui02/components/TagInput/TagInput";
import { CancellablePromise } from "mobx/lib/api/flow";
import { delay } from "utils/async";
import _ from "lodash";
import { IProperty } from "model/entities/types/IProperty";
import { IDataTable } from "model/entities/types/IDataTable";

export interface IStringFilterOp {}

interface IOp1 {
  type: "eq" | "neq" | "contains" | "ncontains";
  human: React.ReactNode;
  val1: string;
}

interface IOp0 {
  type: "null" | "nnull";
  human: React.ReactNode;
}

type ISetting = IOp1 | IOp0;

const OPERATORS: ISetting[] = [
  { human: <>=</>, type: "eq", val1: "" },
  { human: <>&ne;</>, type: "neq", val1: "" },
  { human: <>contain</>, type: "contains", val1: "" },
  { human: <>not contain</>, type: "ncontains", val1: "" },
  { human: <>is null</>, type: "null" },
  { human: <>is not null</>, type: "nnull" }
];

const OpCombo: React.FC<{
  setting: ISetting;
  onChange: (newSetting: ISetting) => void;
}> = props => {
  return (
    <FilterSettingsComboBox trigger={<>{props.setting.human}</>}>
      {OPERATORS.map(op => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() => props.onChange(op)}
        >
          {op.human}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
};

export interface ITagEditorItem {
  text: string;
  value: string;
}

@observer
export class OptionGrid extends React.Component<{
  items: Array<{ content: string; value: string }>;
  onCellClick?(event: any, rowIndex: number, columnIndex: number): void;
}> {
  render() {
    const rowHeight = 20;
    const rowCount = this.props.items.length;
    const columnWidths = [100];

    const totalHeight = rowHeight * rowCount;
    const totalWidth = columnWidths.reduce((a, b) => a + b, 0);
    const maxHeight = 400;
    const maxWidth = 300;
    const overflowX = totalWidth < maxWidth ? "hidden" : "scroll";
    const overflowY = totalHeight < maxHeight ? "hidden" : "scroll";

    return (
      <Grid
        style={{ overflowX, overflowY }}
        width={Math.min(totalWidth, maxWidth)}
        height={Math.min(totalHeight, maxHeight)}
        cellRenderer={this.renderCell}
        rowCount={rowCount}
        columnCount={columnWidths.length}
        rowHeight={rowHeight}
        columnWidth={({ index }) => columnWidths[index]}
      />
    );
  }

  renderCell = ({ style, key, rowIndex, columnIndex }: GridCellProps) => {
    return (
      <div
        style={style}
        key={key}
        className={S.optionGridCell + (rowIndex % 2 === 0 ? " a" : " b")}
        onClick={(event: any) =>
          this.props.onCellClick &&
          this.props.onCellClick(event, rowIndex, columnIndex)
        }
      >
        {this.props.items[rowIndex].content}
      </div>
    );
  };
}

@observer
export class TagInputStateful extends React.Component<{
  selectedItems: Array<{ value: any; content: any }>;
  onChange?(selectedItems: Array<{ value: any; content: any }>): void;
  getOptions(
    searchTerm: string
  ): CancellablePromise<Array<{ value: any; content: any }>>;
}> {
  @observable cursorAfterIndex = this.props.selectedItems.length - 1;
  @observable searchTerm = "";
  @observable availOptions: Array<{ content: any; value: any }> = [];

  @computed get offeredOptions() {
    const selectedIds = new Set(
      this.props.selectedItems.map(item => item.value)
    );
    return this.availOptions.filter(option => !selectedIds.has(option.value));
  }

  componentDidUpdate() {
    runInAction(() => {
      this.cursorAfterIndex = Math.min(
        this.cursorAfterIndex,
        this.props.selectedItems.length - 1
      );
      // TODO: detect that the component updated due to its own event
      // (otherwise there might be mess caused by a focus avalanche)
      if (this.cursorAfterIndex < this.props.selectedItems.length - 1) {
        setTimeout(() => this.elmFakeInput && this.elmFakeInput.focus());
      } else {
        setTimeout(() => this.elmInput && this.elmInput.focus());
      }
    });
  }

  @action.bound handleFakeEditKeyDown(event: any) {
    event.stopPropagation();
    switch (event.key) {
      case "ArrowLeft":
        if (this.cursorAfterIndex > -1) {
          this.cursorAfterIndex--;
          setTimeout(() => this.elmFakeInput && this.elmFakeInput.focus());
        }

        break;
      case "ArrowRight":
        if (this.cursorAfterIndex < this.props.selectedItems.length - 1) {
          this.cursorAfterIndex++;
        }
        if (this.cursorAfterIndex < this.props.selectedItems.length - 1) {
          setTimeout(() => this.elmFakeInput && this.elmFakeInput.focus());
        } else {
          setTimeout(() => this.elmInput && this.elmInput.focus());
        }
        break;
      case "Delete":
        if (this.cursorAfterIndex < this.props.selectedItems.length - 1) {
          if (this.props.onChange) {
            const newItems = [...this.props.selectedItems];
            newItems.splice(this.cursorAfterIndex + 1, 1);
            this.props.onChange(newItems);
          }
        }
        break;
      case "Backspace":
        if (this.cursorAfterIndex > -1) {
          if (this.props.onChange) {
            const newItems = [...this.props.selectedItems];
            newItems.splice(this.cursorAfterIndex, 1);
            this.cursorAfterIndex--;
            this.props.onChange(newItems);
          }
        }
        break;
    }
  }

  @action.bound handleEditKeyDown(event: any) {
    switch (event.key) {
      case "ArrowLeft":
        if (this.elmInput) {
          if (
            this.elmInput.selectionStart === 0 &&
            this.elmInput.selectionEnd === 0
          ) {
            this.cursorAfterIndex--;
            setTimeout(() => this.elmFakeInput && this.elmFakeInput.focus());
          }
        }
        break;
      case "Backspace":
        if (
          this.cursorAfterIndex > -1 &&
          this.elmInput &&
          this.elmInput.selectionStart === 0 &&
          this.elmInput.selectionEnd === 0
        ) {
          if (this.props.onChange) {
            const newItems = [...this.props.selectedItems];
            newItems.splice(this.cursorAfterIndex, 1);
            this.cursorAfterIndex--;
            this.props.onChange(newItems);
          }
        }
        break;
    }
  }

  @action.bound handleDeleteBtnClick(event: any, value: any) {
    const idx = this.props.selectedItems.findIndex(
      item => item.value === value
    );
    const newItems = [...this.props.selectedItems];
    newItems.splice(idx, 1);
    this.props.onChange && this.props.onChange(newItems);
  }

  @action.bound handleEditFocus(event: any) {
    this.cursorAfterIndex = this.props.selectedItems.length - 1;
    setTimeout(() => this.elmInput && this.elmInput.focus());
  }

  elmInput: HTMLInputElement | null = null;
  refInput = (elm: any) => (this.elmInput = elm);

  elmFakeInput: HTMLInputElement | null = null;
  refFakeInput = (elm: any) => (this.elmFakeInput = elm);

  elmDropdowner: Dropdowner | null = null;
  refDropdowner = (elm: any) => (this.elmDropdowner = elm);

  handlePlusClick = flow(
    function*(this: TagInputStateful, event: any) {
      yield* this.getOptions();
    }.bind(this)
  );

  @action.bound handleSearchTermChange(event: any) {
    this.searchTerm = event.target.value;
    this.handleSearchTermChangeDeb(event);
  }

  handleSearchTermChangeImm = flow(function*(
    this: TagInputStateful,
    event: any
  ) {
    yield* this.getOptions();
  });

  handleSearchTermChangeDeb = _.debounce(this.handleSearchTermChangeImm, 100);

  *getOptions() {
    const options = yield this.props.getOptions(this.searchTerm);
    this.availOptions = options;
    this.elmDropdowner && this.elmDropdowner.setDropped(true);
  }

  @action.bound handleOptionCellClick(
    event: AnalyserNode,
    rowIndex: number,
    columnIndex: number
  ) {
    const newSelectedItems = [...this.props.selectedItems];
    newSelectedItems.push(this.offeredOptions[rowIndex]);
    this.elmDropdowner && this.elmDropdowner.setDropped(true);
    this.props.onChange && this.props.onChange(newSelectedItems);
  }

  render() {
    return (
      <TagInput>
        {this.cursorAfterIndex === -1 && (
          <TagInputEditFake
            domRef={this.refFakeInput}
            onKeyDown={this.handleFakeEditKeyDown}
          />
        )}
        {this.props.selectedItems.map((item, idx) => {
          return (
            <React.Fragment key={item.value}>
              <TagInputItem key={item.value}>
                {item.content}
                <TagInputDeleteBtn
                  onClick={event =>
                    this.handleDeleteBtnClick(event, item.value)
                  }
                />
              </TagInputItem>
              {this.cursorAfterIndex === idx &&
                idx < this.props.selectedItems.length - 1 && (
                  <TagInputEditFake
                    domRef={this.refFakeInput}
                    onKeyDown={this.handleFakeEditKeyDown}
                  />
                )}
            </React.Fragment>
          );
        })}
        <Dropdowner
          ref={this.refDropdowner}
          trigger={({ refTrigger, setDropped }) => (
            <TagInputPlus domRef={refTrigger} onClick={this.handlePlusClick} />
          )}
          content={({ setDropped }) => (
            <div className={S.droppedPanel}>
              <OptionGrid
                items={this.offeredOptions}
                onCellClick={this.handleOptionCellClick}
              />
            </div>
          )}
        />

        <TagInputEdit
          domRef={this.refInput}
          onKeyDown={this.handleEditKeyDown}
          onFocus={this.handleEditFocus}
          onChange={this.handleSearchTermChange}
          value={this.searchTerm}
        />
      </TagInput>
    );
  }
}

@observer
class OpEditors extends React.Component<{
  setting: ISetting;
  onChange: (newSetting: ISetting) => void;
  getOptions: (
    searchTerm: string
  ) => CancellablePromise<Array<{ value: any; content: any }>>;
}> {
  @observable selectedItems = [
    { value: 1, content: "Item1" },
    { value: 2, content: "Item2" },
    { value: 3, content: "Item3" }
  ];

  @action.bound handleSelectedItemsChange(
    items: Array<{ value: any; content: any }>
  ) {
    this.selectedItems = items;
  }

  render() {
    const { setting } = this.props;
    switch (setting.type) {
      case "eq":
      case "neq":
      case "contains":
      case "ncontains":
        return (
          <TagInputStateful
            selectedItems={this.selectedItems}
            onChange={this.handleSelectedItemsChange}
            getOptions={this.props.getOptions}
          />
        );
      case "null":
      case "nnull":
      default:
        return null;
    }
  }
}

@observer
export class FilterSettingsLookup extends React.Component<{
  getOptions: (
    searchTerm: string
  ) => CancellablePromise<Array<{ value: any; content: any }>>;
}> {
  @observable selectedOperator: ISetting = OPERATORS[0];

  render() {
    return (
      <>
        <OpCombo
          setting={this.selectedOperator}
          onChange={newSetting => (this.selectedOperator = newSetting)}
        />
        <OpEditors
          setting={this.selectedOperator}
          onChange={newSetting => (this.selectedOperator = newSetting)}
          getOptions={this.props.getOptions}
        />

        {/*<input className={CS.input} />*/}
      </>
    );
  }
}
