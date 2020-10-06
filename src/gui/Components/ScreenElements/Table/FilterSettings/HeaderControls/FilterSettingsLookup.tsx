import {
  TagInput,
  TagInputDeleteBtn,
  TagInputEdit,
  TagInputItem,
  TagInputPlus,
} from "gui02/components/TagInput/TagInput";
import _ from "lodash";
import { action, computed, flow, observable, runInAction, toJS } from "mobx";
import { MobXProviderContext, observer } from "mobx-react";
import { CancellablePromise } from "mobx/lib/api/flow";
import React, { useContext, useState } from "react";
import { Grid, GridCellProps } from "react-virtualized";
import Highlighter from "react-highlight-words";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";
import S from "./FilterSettingsLookup.module.scss";
import produce from "immer";
import { IFilterSetting } from "model/entities/types/IFilterSetting";
import { FilterSetting } from "./FilterSetting";
import { rowHeight } from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";
import { T } from "utils/translation";
import {
  CtxDropdownEditor,
  DropdownEditor,
  DropdownEditorSetup,
  IDropdownEditorContext,
} from "modules/Editors/DropdownEditor/DropdownEditor";
import { TagInputEditor } from "gui/Components/ScreenElements/Editors/TagInputEditor";
import { IDataView } from "model/entities/types/IDataView";
import {
  DropdownEditorApi,
  IDropdownEditorApi
} from "modules/Editors/DropdownEditor/DropdownEditorApi";
import {
  DropdownEditorData,
  IDropdownEditorData,
} from "modules/Editors/DropdownEditor/DropdownEditorData";
import {
  DropdownColumnDrivers,
  DropdownDataTable,
} from "modules/Editors/DropdownEditor/DropdownTableModel";
import { DropdownEditorLookupListCache } from "modules/Editors/DropdownEditor/DropdownEditorLookupListCache";
import { DropdownEditorBehavior } from "modules/Editors/DropdownEditor/DropdownEditorBehavior";
import { TextCellDriver } from "modules/Editors/DropdownEditor/Cells/TextCellDriver";
import { NumberCellDriver } from "modules/Editors/DropdownEditor/Cells/NumberCellDriver";
import { BooleanCellDriver } from "modules/Editors/DropdownEditor/Cells/BooleanCellDriver";
import { DefaultHeaderCellDriver } from "modules/Editors/DropdownEditor/Cells/HeaderCell";
import { DataViewData } from "modules/DataView/DataViewData";
import { RowCursor } from "modules/DataView/TableCursor";
import { ILookup } from "model/entities/types/ILookup";
import { IProperty } from "model/entities/types/IProperty";
import {DataViewAPI} from "modules/DataView/DataViewAPI";
import {TagInputEditorData} from "modules/Editors/DropdownEditor/TagInputEditorData";

const OPERATORS = () =>
  [
    { human: <>=</>, type: "in" },
    { human: <>&ne;</>, type: "nin" },
    { human: <>{T("contains", "filter_operator_contains")}</>, type: "contains" },
    {
      human: <>{T("not contains", "filter_operator_not_contains")}</>,
      type: "ncontains",
    },
    { human: <>{T("is null", "filter_operator_is_null")}</>, type: "null" },
    {
      human: <>{T("is not null", "filter_operator_not_is_null")}</>,
      type: "nnull",
    },
  ] as any[];

const OpCombo: React.FC<{
  setting: any;
  onChange: (newSetting: any) => void;
}> = (props) => {
  return (
    <FilterSettingsComboBox
      trigger={<>{(OPERATORS().find((op) => op.type === props.setting.type) || {}).human}</>}
    >
      {OPERATORS().map((op) => (
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
        {this.props.selectedItems.map((item, idx) => {
          return (
            <React.Fragment key={item.value}>
              <TagInputItem key={item.value}>
                {item.content}
                <TagInputDeleteBtn
                  onClick={(event) => this.handleDeleteBtnClick(event, item.value)}
                />
              </TagInputItem>
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
  setting: IFilterSetting | undefined;
  onChange: (newSetting: any) => void;
  onChangeDebounced: (newSetting: any) => void;
  getOptions: (searchTerm: string) => CancellablePromise<Array<any>>;
  lookup: ILookup;
  property: IProperty;
}> {
  @observable selectedItems: Array<Array<any>> = [];

  @action.bound handleSelectedItemsChange(items: Array<any>) {
    this.selectedItems = items;
    this.props.onChange(
      produce(this.props.setting, (draft: IFilterSetting) => {
        draft.val1 = toJS(items.map(item => {return {value: item}}), { recurseEverything: true });
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
    switch (setting?.type) {
      case "in":
      case "nin":
        return (
          <FilterBuildDropdownEditor
            lookup={this.props.lookup}
            property={this.props.property}
            getOptions={this.props.getOptions}
            onChange={this.handleSelectedItemsChange}
            values={this.selectedItems}
          />
          // <TagInputStateful
          //   selectedItems={setting.val1 ? this.selectedItems : []}
          //   onChange={this.handleSelectedItemsChange}
          //   getOptions={this.props.getOptions}
          // />
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
  getOptions: (searchTerm: string) => CancellablePromise<Array<any>>;
  lookup: ILookup;
  property: IProperty;
  setting: IFilterSetting | undefined;
  onTriggerApplySetting?(setting: any): void;
}> {
  @observable.ref setting: FilterSetting = new LookupFilterSetting(
    OPERATORS()[0].type,
    OPERATORS()[0].human
  );

  @action.bound handleChange(newSetting: any) {
    newSetting.lookupId =
      newSetting.type === "contains" || newSetting.type === "ncontains"
        ? this.props.lookup.lookupId
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
          lookup={this.props.lookup}
          property={this.props.property}
        />
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


export function FilterBuildDropdownEditor(props: {
  // xmlNode: any;
  // isReadOnly: boolean;
  // isInvalid?: boolean;
  // invalidMessage?: string;
  // backgroundColor?: string;
  // foregroundColor?: string;
  // customStyle?: any;
  // tagEditor?: JSX.Element;
  // onDoubleClick?: (event: any) => void;
  // subscribeToFocusManager?: (obj: IFocusable) => () => void;
  // onKeyDown?(event: any): void;
  lookup: ILookup;
  property: IProperty;
  getOptions: (searchTerm: string) => CancellablePromise<Array<any>>;
  onChange(selectedItems: Array<any>): void;
  values: Array<any>;
}) {
  const mobxContext = useContext(MobXProviderContext);
  const dataView = mobxContext.dataView as IDataView;
  const { dataViewRowCursor, dataViewApi, dataViewData } = dataView;
  const workbench = mobxContext.workbench;
  const { lookupListCache } = workbench;

  const [dropdownEditorInfrastructure] = useState<IDropdownEditorContext>(() => {
    const dropdownEditorApi: IDropdownEditorApi = new DropDownApi(props.getOptions);
    // const dropdownEditorApi: DropdownEditorApi = new DropdownEditorApi(
    //   () => dropdownEditorSetup,
    //   dataViewRowCursor,
    //   dataViewApi,
    //   () => dropdownEditorBehavior
    // );
    const dropdownEditorData: IDropdownEditorData =
      new FilterEditorData(dataViewData, dataViewRowCursor, () => dropdownEditorSetup, props.onChange)

    const dropdownEditorDataTable = new DropdownDataTable(
      () => dropdownEditorSetup,
      dropdownEditorData
    );
    const dropdownEditorLookupListCache = new DropdownEditorLookupListCache(
      () => dropdownEditorSetup,
      lookupListCache
    );
    const dropdownEditorBehavior = new DropdownEditorBehavior(
      dropdownEditorApi,
      dropdownEditorData,
      dropdownEditorDataTable,
      () => dropdownEditorSetup,
      dropdownEditorLookupListCache,
      false,
    );


    // const rat = props.xmlNode.attributes;
    // const lookupId = rat.LookupId;
    // const propertyId = props.propertyId;
    // const showUniqueValues = true;
    // const identifier = rat.Identifier;
    // let identifierIndex = 0;
    // const dropdownType = rat.DropDownType;
    // const cached = rat.Cached === "true";
    // const searchByFirstColumnOnly = rat.SearchByFirstColumnOnly === "true";
    //
    // const columnNames: string[] = [identifier];
    // const visibleColumnNames: string[] = [];
    // const columnNameToIndex = new Map<string, number>([[identifier, identifierIndex]]);
    // let index = 0;
    // const drivers = new DropdownColumnDrivers();
    // for (let ddp of findStopping(props.xmlNode, (n) => n.name === "Property")) {
    //   index++;
    //   const pat = ddp.attributes;
    //   const id = pat.Id;
    //   columnNames.push(id);
    //   columnNameToIndex.set(id, index);
    //
    //   visibleColumnNames.push(id);
    //   const name = pat.Name;
    //   const column = pat.Column;
    //
    //   let bodyCellDriver;
    //   switch (column) {
    //     case "Text":
    //       bodyCellDriver = new TextCellDriver(
    //         index,
    //         dropdownEditorDataTable,
    //         dropdownEditorBehavior
    //       );
    //       break;
    //     case "Number":
    //       bodyCellDriver = new NumberCellDriver(
    //         index,
    //         dropdownEditorDataTable,
    //         dropdownEditorBehavior
    //       );
    //       break;
    //     case "CheckBox":
    //       bodyCellDriver = new BooleanCellDriver(
    //         index,
    //         dropdownEditorDataTable,
    //         dropdownEditorBehavior
    //       );
    //       break;
    //     default:
    //       throw new Error("Unknown column type " + column);
    //   }
    //
    //   drivers.drivers.push({
    //     headerCellDriver: new DefaultHeaderCellDriver(name),
    //     bodyCellDriver,
    //   });
    // }

    // const parameters: { [k: string]: any } = {};
    //
    // for (let ddp of findStopping(props.xmlNode, (n) => n.name === "ComboBoxParameterMapping")) {
    //   const pat = ddp.attributes;
    //   parameters[pat.ParameterName] = pat.FieldName;
    // }

    const lookupColumn = props.lookup.dropDownColumns[0];

    const drivers = new DropdownColumnDrivers();

    // const columnNames = [props.property.identifier!];
    let identifierIndex = 0;
    const columnNameToIndex = new Map<string, number>([[props.property.identifier!, identifierIndex]]);
    const visibleColumnNames: string[]=[];

    // let index=0;
    // for (let dropDownColumn of props.lookup.dropDownColumns) {
    //   index++;
    //   // columnNames.push(dropDownColumn.id);
    //   // columnNameToIndex.set(dropDownColumn.id, index);
    //
    //   // visibleColumnNames.push(dropDownColumn.id);
    //
    //   let bodyCellDriver;
    //   switch (dropDownColumn.column) {
    //     case "Text":
    //       bodyCellDriver = new TextCellDriver(
    //         index,
    //         dropdownEditorDataTable,
    //         dropdownEditorBehavior
    //       );
    //       break;
    //     case "Number":
    //       bodyCellDriver = new NumberCellDriver(
    //         index,
    //         dropdownEditorDataTable,
    //         dropdownEditorBehavior
    //       );
    //       break;
    //     case "CheckBox":
    //       bodyCellDriver = new BooleanCellDriver(
    //         index,
    //         dropdownEditorDataTable,
    //         dropdownEditorBehavior
    //       );
    //       break;
    //     default:
    //       throw new Error("Unknown column type " + dropDownColumn.column);
    //   }
    // }

    // columnNames.push(props.property.name);
    columnNameToIndex.set(props.property.name, 1);
    visibleColumnNames.push(props.property.name);

    const bodyCellDriver = new TextCellDriver(
      1,
      dropdownEditorDataTable,
      dropdownEditorBehavior
    );

    drivers.drivers.push({
      headerCellDriver: new DefaultHeaderCellDriver(props.property.name),
      bodyCellDriver,
    });

    const showUniqueValues = true;

    const dropdownEditorSetup = new DropdownEditorSetup(
      props.property.id,
      props.lookup.lookupId,
      [],
      visibleColumnNames,
      columnNameToIndex,
      showUniqueValues,
      props.property.identifier!,
      identifierIndex,
      props.property.parameters,
      props.property.lookup?.dropDownType!,
      props.property.lookup?.cached!,
      !props.property.lookup?.searchByFirstColumnOnly
    );

    return {
      behavior: dropdownEditorBehavior,
      editorData: dropdownEditorData,
      columnDrivers: drivers,
      editorDataTable: dropdownEditorDataTable,
    };
  });

  // useEffect(() => {
  //   dropdownEditorInfrastructure.behavior.isReadOnly = props.isReadOnly;
  // }, [props.isReadOnly]);

  function onItemRemoved(event: any, item: any){
    props.onChange(props.values);
  }

  const value = props.values;
  return (
    <CtxDropdownEditor.Provider value={dropdownEditorInfrastructure}>
      <DropdownEditor
        editor={<TagInputEditor
          value={value}
          isReadOnly={false}
          isInvalid={false}
          // invalidMessage={invalidMessage}
          isFocused={false}
          // backgroundColor={backgroundColor}
          // foregroundColor={foregroundColor}
          // customStyle={this.props.property?.style}
          refocuser={undefined}
          onChange={onItemRemoved}
          // onKeyDown={this.MakeOnKeyDownCallBack()}
          onClick={undefined}
          // onEditorBlur={this.props.onEditorBlur}
        />}
        // isInvalid={props.isInvalid}
        // invalidMessage={props.invalidMessage}
        // backgroundColor={props.backgroundColor}
        // foregroundColor={props.foregroundColor}
        // customStyle={props.customStyle}
      />
    </CtxDropdownEditor.Provider>
  );
}


// @bind
export class FilterEditorData implements IDropdownEditorData {
  // dropdownEditorData: IDropdownEditorData;

  constructor(
    private dataTable: DataViewData,
    private rowCursor: RowCursor,
    private setup: () => DropdownEditorSetup,
    private onChange: (selectedItems: Array<any>) => void
  ) {
     //this.dropdownEditorData = new DropdownEditorData(dataTable, rowCursor, setup);
  }

  @computed get value(): string | string[] | null {
    return this._value;
  }

  @observable
  _value:  any[] = [];

  @computed get text(): string {
    return ""; //this.dropdownEditorData.text;
  }

  get isResolving() {
    return false; //this.dropdownEditorData.isResolving;
  }

  @action.bound chooseNewValue(value: any) {
    if(value !== null){
      this._value = [ ...this._value, value];
      this.onChange(this._value);
    }
    // if (this.rowCursor.selectedId) {
    //   this.dataTable.setNewValue(this.rowCursor.selectedId, this.setup().propertyId, newArray);
    // }
  }

  get idsInEditor() {
    return  this._value as string[];
  }
}

class DropDownApi implements IDropdownEditorApi{
    constructor(private getOptions: (searchTerm: string) => CancellablePromise<Array<any>>){
    }

    *getLookupList(searchTerm: string): any {
      return yield this.getOptions("");
    }
}
