import React, { useImperativeHandle } from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.css";
import STagInput from "gui/Components/TagInputComp/TagInput.module.css";
import { observable, computed, action, flow, runInAction } from "mobx";
import { observer, PropTypes } from "mobx-react";

import { useObservable } from "mobx-react-lite";
import { Dropdowner } from "../../../../Dropdowner/Dropdowner";
/*import {
  TagInput,
  TagInputAddBtn,
  TagInputItem,
  TagInputRemoveBtn,
  TagInputTextbox
} from "gui/Components/TagInputComp/TagInput";*/
import { Grid, GridCellProps } from "react-virtualized";
import {
  TagInput,
  TagInputItem,
  TagInputDeleteBtn,
  TagInputPlus,
  TagInputEdit,
  TagInputEditFake
} from "gui02/components/TagInput/TagInput";

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
  items: Array<{ text: string; value: string }>;
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
        className={
          STagInput.optionGridCell + (rowIndex % 2 === 0 ? " a" : " b")
        }
        onClick={(event: any) =>
          this.props.onCellClick &&
          this.props.onCellClick(event, rowIndex, columnIndex)
        }
      >
        {this.props.items[rowIndex].text}
      </div>
    );
  };
}

/*
@observer
class TagEditor extends React.Component<{
  items: ITagEditorItem[];
  availableItems: ITagEditorItem[];
  onAdd?: (item: ITagEditorItem) => void;
  onDeleteClick?: (event: any, item: ITagEditorItem) => void;
}> {
  refDropdowner = (elm: Dropdowner | null) => (this.elmDropdowner = elm);
  elmDropdowner: Dropdowner | null = null;

  @action.bound
  setDropped(state: boolean) {
    this.elmDropdowner && this.elmDropdowner.setDropped(state);
  }

  handleAddBtnClick = flow(
    function*(this: TagEditor) {
      this.setDropped(true);
      yield 0;
    }.bind(this)
  );

  render() {
    return (
      <TagInput>
        {this.props.items.map(item => (
          <TagInputItem key={item.value}>
            <TagInputRemoveBtn onClick={undefined} />
            {item.text}
          </TagInputItem>
        ))}

        <div className={"asdf " + STagInput.textboxAndDropdownTrigger}>
          <Dropdowner
            ref={this.refDropdowner}
            trigger={({ refTrigger, setDropped }) => (
              <TagInputAddBtn
                domRef={refTrigger}
                onClick={this.handleAddBtnClick}
              />
            )}
            content={({ setDropped }) => (
              <div className={STagInput.optionGridContainer}>
                <OptionGrid
                  onCellClick={undefined}
                  items={[{ text: "dfsd", value: "fdasdf" }]}
                />
              </div>
            )}
          />
          <TagInputTextbox value={undefined} onChange={undefined} />
        </div>
      </TagInput>
    );
  }
}

const StatefulTagEditor: React.FC<{}> = observer(props => {
  const items = useObservable<ITagEditorItem[]>([]);
  return (
    <TagEditor
      items={items}
      availableItems={[
        { text: "Apple", value: "1" },
        { text: "Banana", value: "2" }
      ]}
      onAdd={item => items.push(item)}
      onDeleteClick={(event, item) =>
        items.splice(items.findIndex(ii => ii === item), 1)
      }
    />
  );
});*/

@observer
export class TagInputStateful extends React.Component<{
  selectedItems: Array<{ value: any; content: any }>;
  onChange?(selectedItems: Array<{ value: any; content: any }>): void;
}> {
  @observable cursorAfterIndex = this.props.selectedItems.length - 1;

  componentDidUpdate() {
    runInAction(() => {
      this.cursorAfterIndex = Math.min(
        this.cursorAfterIndex,
        this.props.selectedItems.length - 1
      );
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

  @action.bound handleEditFocus(event: any) {
    this.cursorAfterIndex = this.props.selectedItems.length - 1;
    setTimeout(() => this.elmInput && this.elmInput.focus());
  }

  elmInput: HTMLInputElement | null = null;
  refInput = (elm: any) => (this.elmInput = elm);

  elmFakeInput: HTMLInputElement | null = null;
  refFakeInput = (elm: any) => (this.elmFakeInput = elm);

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
                <TagInputDeleteBtn />
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
        <TagInputPlus />
        <TagInputEdit
          domRef={this.refInput}
          onKeyDown={this.handleEditKeyDown}
          onFocus={this.handleEditFocus}
        />

        {/*
        <TagInputItem>
          Item1
          <TagInputDeleteBtn />
        </TagInputItem>

        <TagInputItem>
          Item2
          <TagInputDeleteBtn />
        </TagInputItem>
        <TagInputItem>
          Item3
          <TagInputDeleteBtn />
        </TagInputItem>
        <TagInputPlus />
<TagInputEdit />*/}
      </TagInput>
    );
  }
}

@observer
class OpEditors extends React.Component<{
  setting: ISetting;
  onChange: (newSetting: ISetting) => void;
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
export class FilterSettingsLookup extends React.Component {
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
        />

        {/*<input className={CS.input} />*/}
      </>
    );
  }
}
