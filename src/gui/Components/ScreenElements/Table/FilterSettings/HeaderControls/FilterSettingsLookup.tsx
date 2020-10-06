import { action, computed, observable, toJS } from "mobx";
import { MobXProviderContext, observer } from "mobx-react";
import { CancellablePromise } from "mobx/lib/api/flow";
import React, { useContext, useState } from "react";
import { Grid, GridCellProps } from "react-virtualized";
import Highlighter from "react-highlight-words";
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
import { IDropdownEditorApi } from "modules/Editors/DropdownEditor/DropdownEditorApi";
import { IDropdownEditorData } from "modules/Editors/DropdownEditor/DropdownEditorData";
import {
  DropdownColumnDrivers,
  DropdownDataTable,
} from "modules/Editors/DropdownEditor/DropdownTableModel";
import { DropdownEditorLookupListCache } from "modules/Editors/DropdownEditor/DropdownEditorLookupListCache";
import { DropdownEditorBehavior } from "modules/Editors/DropdownEditor/DropdownEditorBehavior";
import { TextCellDriver } from "modules/Editors/DropdownEditor/Cells/TextCellDriver";
import { DefaultHeaderCellDriver } from "modules/Editors/DropdownEditor/Cells/HeaderCell";
import { ILookup } from "model/entities/types/ILookup";
import { IProperty } from "model/entities/types/IProperty";

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
        draft.val1 = toJS(
          items.map((item) => {
            return { value: item };
          }),
          { recurseEverything: true }
        );
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
    const dropdownEditorData: IDropdownEditorData = new FilterEditorData(props.onChange);

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
      false
    );
    const lookupColumn = props.lookup.dropDownColumns[0];

    const drivers = new DropdownColumnDrivers();

    let identifierIndex = 0;
    const columnNameToIndex = new Map<string, number>([
      [props.property.identifier!, identifierIndex],
    ]);
    const visibleColumnNames: string[] = [];

    columnNameToIndex.set(props.property.name, 1);
    visibleColumnNames.push(props.property.name);

    const bodyCellDriver = new TextCellDriver(1, dropdownEditorDataTable, dropdownEditorBehavior);

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

  function onItemRemoved(event: any, item: any) {
    props.onChange(props.values);
  }

  const value = props.values;
  return (
    <CtxDropdownEditor.Provider value={dropdownEditorInfrastructure}>
      <DropdownEditor
        editor={
          <TagInputEditor
            value={value}
            isReadOnly={false}
            isInvalid={false}
            isFocused={false}
            refocuser={undefined}
            onChange={onItemRemoved}
            onClick={undefined}
          />
        }
      />
    </CtxDropdownEditor.Provider>
  );
}

export class FilterEditorData implements IDropdownEditorData {
  constructor(private onChange: (selectedItems: Array<any>) => void) {}

  @computed get value(): string | string[] | null {
    return this._value;
  }

  @observable
  _value: any[] = [];

  @computed get text(): string {
    return "";
  }

  get isResolving() {
    return false;
  }

  @action.bound chooseNewValue(value: any) {
    if (value !== null) {
      this._value = [...this._value, value];
      this.onChange(this._value);
    }
  }

  get idsInEditor() {
    return this._value as string[];
  }
}

class DropDownApi implements IDropdownEditorApi {
  constructor(private getOptions: (searchTerm: string) => CancellablePromise<Array<any>>) {}

  *getLookupList(searchTerm: string): any {
    return yield this.getOptions("");
  }
}
