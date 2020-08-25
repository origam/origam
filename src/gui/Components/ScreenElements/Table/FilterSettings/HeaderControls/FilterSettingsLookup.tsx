import {
  TagInput,
  TagInputDeleteBtn,
  TagInputEdit,
  TagInputEditFake,
  TagInputItem,
  TagInputPlus,
} from "gui02/components/TagInput/TagInput";
import _ from "lodash";
import { action, computed, flow, observable, runInAction, toJS } from "mobx";
import { observer } from "mobx-react";
import { CancellablePromise } from "mobx/lib/api/flow";
import React from "react";
import { Grid, GridCellProps } from "react-virtualized";
import Highlighter from "react-highlight-words";
import { Dropdowner } from "../../../../Dropdowner/Dropdowner";
import { FilterSettingsComboBox, FilterSettingsComboBoxItem } from "../FilterSettingsComboBox";
import S from "./FilterSettingsLookup.module.scss";
import produce from "immer";
import { IFilterSetting } from "../../../../../../model/entities/types/IFilterSetting";
import { FilterSetting } from "./FilterSetting";

const OPERATORS: any[] = [
  { human: <>=</>, type: "in" },
  { human: <>&ne;</>, type: "nin" },
  { human: <>contain</>, type: "contains" },
  { human: <>not contain</>, type: "ncontains" },
  { human: <>is null</>, type: "null" },
  { human: <>is not null</>, type: "nnull" },
];

const OpCombo: React.FC<{
  setting: any;
  onChange: (newSetting: any) => void;
}> = (props) => {
  return (
    <FilterSettingsComboBox
      trigger={<>{(OPERATORS.find((op) => op.type === props.setting.type) || {}).human}</>}
    >
      {OPERATORS.map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() => {
            props.onChange(
              produce(props.setting, (draft: IFilterSetting) => {
                draft.type = op.type;
                draft.isComplete = op.type === "null" || op.type === "nnull";
                draft.val1 = undefined;
                draft.val2 = undefined;
              })
            );
          }}
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
  searchPhrase?: string;
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
          this.props.onCellClick && this.props.onCellClick(event, rowIndex, columnIndex)
        }
      >
        <Highlighter
          textToHighlight={this.props.items[rowIndex].content}
          searchWords={[this.props.searchPhrase].filter((item) => item) as string[]}
          autoEscape={true}
        />
        {}
      </div>
    );
  };
}

@observer
export class TagInputStateful extends React.Component<{
  selectedItems: Array<{ value: any; content: any }>;
  onChange?(selectedItems: Array<{ value: any; content: any }>): void;
  getOptions(searchTerm: string): CancellablePromise<Array<{ value: any; content: any }>>;
}> {
  @observable cursorAfterIndex = this.props.selectedItems.length - 1;
  @observable searchTerm = "";
  @observable.shallow availOptions: Array<{ content: any; value: any }> = [];

  @computed get offeredOptions() {
    const selectedIds = new Set(this.props.selectedItems.map((item) => item.value));
    return this.availOptions.filter((option) => !selectedIds.has(option.value));
  }

  componentDidUpdate() {
    runInAction(() => {
      this.cursorAfterIndex = Math.min(this.cursorAfterIndex, this.props.selectedItems.length - 1);
      // TODO: detect that the component updated due to its own event
      // (otherwise there might be mess caused by a focus avalanche)
      /*if (this.cursorAfterIndex < this.props.selectedItems.length - 1) {
        setTimeout(() => this.elmFakeInput && this.elmFakeInput.focus());
      } else {
        setTimeout(() => this.elmInput && this.elmInput.focus());
      }*/
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
          if (this.elmInput.selectionStart === 0 && this.elmInput.selectionEnd === 0) {
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
    const idx = this.props.selectedItems.findIndex((item) => item.value === value);
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
    function* (this: TagInputStateful, event: any) {
      yield* this.getOptions();
    }.bind(this)
  );

  @action.bound handleSearchTermChange(event: any) {
    this.searchTerm = event.target.value;
    this.handleSearchTermChangeDeb(event);
  }

  handleSearchTermChangeImm = flow(function* (this: TagInputStateful, event: any) {
    yield* this.getOptions();
  });

  handleSearchTermChangeDeb = _.debounce(this.handleSearchTermChangeImm, 100);

  *getOptions() {
    const options = yield this.props.getOptions(this.searchTerm);
    this.availOptions = options;
    this.elmDropdowner && this.elmDropdowner.setDropped(true);
  }

  @action.bound handleOptionCellClick(event: AnalyserNode, rowIndex: number, columnIndex: number) {
    const newSelectedItems = [...this.props.selectedItems];
    newSelectedItems.push(this.offeredOptions[rowIndex]);
    this.elmDropdowner && this.elmDropdowner.setDropped(false);
    this.props.onChange && this.props.onChange(newSelectedItems);
  }

  render() {
    return (
      <TagInput>
        {/*{this.cursorAfterIndex === -1 && (*/}
        {/*  <TagInputEditFake domRef={this.refFakeInput} onKeyDown={this.handleFakeEditKeyDown} />*/}
        {/*)}*/}
        {this.props.selectedItems.map((item, idx) => {
          return (
            <React.Fragment key={item.value}>
              <TagInputItem key={item.value}>
                {item.content}
                <TagInputDeleteBtn
                  onClick={(event) => this.handleDeleteBtnClick(event, item.value)}
                />
              </TagInputItem>
              {/*{this.cursorAfterIndex === idx && idx < this.props.selectedItems.length - 1 && (*/}
              {/*  <TagInputEditFake*/}
              {/*    domRef={this.refFakeInput}*/}
              {/*    onKeyDown={this.handleFakeEditKeyDown}*/}
              {/*  />*/}
              {/*)}*/}
            </React.Fragment>
          );
        })}
        <Dropdowner
          ref={this.refDropdowner}
          trigger={({ refTrigger, setDropped }) => (
            <>
              <TagInputPlus domRef={refTrigger} onClick={this.handlePlusClick} />
              <TagInputEdit
                domRef={this.refInput}
                onKeyDown={this.handleEditKeyDown}
                onFocus={this.handleEditFocus}
                onChange={this.handleSearchTermChange}
                value={this.searchTerm}
              />
            </>
          )}
          content={({ setDropped }) => (
            <div className={S.droppedPanel}>
              {this.offeredOptions.length > 0 ? (
                <OptionGrid
                  items={this.offeredOptions}
                  onCellClick={this.handleOptionCellClick}
                  searchPhrase={this.searchTerm}
                />
              ) : (
                <div className={S.noItemsFound}>No items found</div>
              )}
            </div>
          )}
        />
      </TagInput>
    );
  }
}

@observer
class OpEditors extends React.Component<{
  setting: any;
  onChange: (newSetting: any) => void;
  onChangeDebounced: (newSetting: any) => void;
  getOptions: (searchTerm: string) => CancellablePromise<Array<{ value: any; content: any }>>;
}> {
  @observable selectedItems: Array<{ value: any; content: any }> = [];

  @action.bound handleSelectedItemsChange(items: Array<{ value: any; content: any }>) {
    this.selectedItems = items;
    this.props.onChange(
      produce(this.props.setting, (draft: IFilterSetting) => {
        draft.val1 = toJS(items, { recurseEverything: true });
        draft.val2 = undefined;
        draft.isComplete = draft.val1 !== undefined && draft.val1.length > 0;
      })
    );
  }

  @action.bound handleTermChange(event: any) {
    this.props.onChangeDebounced(
      produce(this.props.setting, (draft: IFilterSetting) => {
        draft.val1 = undefined;
        draft.val2 = event.target.value;
        draft.isComplete = !!draft.val2;
      })
    );
  }

  render() {
    const { setting } = this.props;
    switch (setting.type) {
      case "in":
      case "nin":
        return (
          <TagInputStateful
            selectedItems={setting.val1 ? this.selectedItems : []}
            onChange={this.handleSelectedItemsChange}
            getOptions={this.props.getOptions}
          />
        );
      case "contains":
      case "ncontains":
        return <input onChange={this.handleTermChange} />;
      case "null":
      case "nnull":
      default:
        return null;
    }
  }
}

@observer
export class FilterSettingsLookup extends React.Component<{
  getOptions: (searchTerm: string) => CancellablePromise<Array<{ value: any; content: any }>>;
  lookupId: string;
  setting: any;
  onTriggerApplySetting?(setting: any): void;
}> {
  @observable.ref setting: FilterSetting = new LookupFilterSetting(
    OPERATORS[0].type,
    OPERATORS[0].human
  );

  @action.bound handleChange(newSetting: any) {
    newSetting.lookupId =
      newSetting.type === "contains" || newSetting.type === "ncontains"
        ? this.props.lookupId
        : undefined;
    this.setting = newSetting;

    this.props.onTriggerApplySetting && this.props.onTriggerApplySetting(this.setting);
  }


  render() {
    return (
      <>
        <OpCombo setting={this.setting} onChange={this.handleChange} />
        <OpEditors
          setting={this.setting}
          onChange={this.handleChange}
          onChangeDebounced={this.handleChange}
          getOptions={this.props.getOptions}
        />

        {/*<input className={CS.input} />*/}
      </>
    );
  }
}

export class LookupFilterSetting implements IFilterSetting {
  type: string;
  caption: string;
  val1?: any;
  val2?: any;
  isComplete: boolean;
  lookupId: string | undefined;

  get filterValue1() {
    if (!this.val1) {
      return this.val1;
    }
    switch (this.type) {
      case "contain":
      case "ncontain":
        return this.val1.map((item: any) => item.content);
      case "in":
      case "nin":
        return this.val1.map((item: any) => item.value);
      default:
        return undefined;
    }
  }

  get filterValue2() {
    return this.val2;
  }

  constructor(type: string, caption: string) {
    this.type = type;
    this.caption = caption;
    this.isComplete = false;
  }
}
