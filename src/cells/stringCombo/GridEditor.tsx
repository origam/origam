import * as React from "react";
import { action, observable, runInAction, computed } from "mobx";
import { observer, Observer, inject } from "mobx-react";
import {
  ICellValue,
  IRecordId,
  IFieldId,
  IDataTableRecord,
  IDataTableFieldStruct
} from "../../DataTable/types";
import Measure from "react-measure";
import { AutoSizer, MultiGrid, GridCellProps } from "react-virtualized";
import { IGridPanelBacking } from "../../GridPanel/types";
import * as _ from "lodash";
import Highlighter from "react-highlight-words";

interface ILookupDropdownProps {
  gridPaneBacking?: IGridPanelBacking;
  editingRecord: IDataTableRecord;
  editingField: IDataTableFieldStruct;
  searchText: string;
  onItemSelected?: (optionId: string | undefined) => void;
}

@inject("gridPaneBacking")
@observer
export class LookupDropdown extends React.Component<ILookupDropdownProps> {
  @observable private hoveredRowIndex: number | undefined = undefined;

  @action.bound
  private handleCellMouseEnter(rowIndex: number, columnIndex: number) {
    this.hoveredRowIndex = rowIndex;
  }

  @action.bound
  private handleCellMouseLeave(rowIndex: number, columnIndex: number) {
    this.hoveredRowIndex = undefined;
  }

  @action.bound
  private handleCellClick(event: any, rowIndex: number, columnIndex: number) {
    event.stopPropagation();
    const option = this.getOptionByIndex(rowIndex);
    const optionId = this.getOptionKey(option);
    this.props.onItemSelected && this.props.onItemSelected(optionId);
  }

  @computed
  get columns() {
    return [
      this.props.editingField.lookupIdentifier,
      ...this.props.editingField.dropdownColumns.map(dc => dc.id)
    ];
  }

  public cellRenderer = ({
    columnIndex,
    isScrolling,
    isVisible,
    key,
    parent,
    rowIndex,
    style
  }: GridCellProps) => {
    if (rowIndex === 0) {
      return (
        <div className="lookup-dropdown-header-cell" style={style} key={key}>
          {this.columns[columnIndex]}
        </div>
      );
    } else {
      console.log(rowIndex - 1, columnIndex, this.options[rowIndex - 1][columnIndex])
      return (
        <Observer>
          {() => (
            <div
              className={
                "lookup-dropdown-body-cell" +
                ((rowIndex - 1) % 2 === 0 ? " evenrow" : " oddrow") +
                (computed(() => rowIndex - 1 === this.hoveredRowIndex).get()
                  ? " hover"
                  : "")
              }
              style={style}
              key={key}
              onMouseEnter={() =>
                this.handleCellMouseEnter(rowIndex - 1, columnIndex)
              }
              onMouseLeave={() =>
                this.handleCellMouseLeave(rowIndex - 1, columnIndex)
              }
              onClick={event => {
                this.handleCellClick(event, rowIndex - 1, columnIndex);
              }}
            >
              <Highlighter
                searchWords={[this.props.searchText]}
                textToHighlight={`${this.options[rowIndex - 1][columnIndex]}`}
              />
            </div>
          )}
        </Observer>
      );
    }
  };

  @observable public options: any = [];

  public componentDidMount() {
    this.loadData();
  }

  public componentDidUpdate(prevProps: ILookupDropdownProps) {
    if (prevProps.searchText !== this.props.searchText) {
      this.loadData();
    }
  }

  public async loadData() {
    const { dataStructureEntityId, dataLoader } = this.props.gridPaneBacking!;
    const options = await dataLoader.loadLokupOptions(
      dataStructureEntityId,
      this.props.editingField!.id,
      this.props.editingRecord!.id,
      this.props.editingField!.lookupId,
      this.props.searchText,
      1000,
      1,
      this.columns
    );
    this.options = options.data;
  }

  private getOptionByIndex(rowIndex: number) {
    return this.options[rowIndex];
  }

  private getOptionKey(option: any[]): string {
    return option[0] as string;
  }

  public render() {
    return (
      <AutoSizer>
        {({ width, height }) => (
          <Observer>
            {() => (
              <MultiGrid
                fixedColumnCount={0}
                fixedRowCount={1}
                cellRenderer={this.cellRenderer}
                width={width}
                height={height}
                rowHeight={20}
                columnWidth={100}
                rowCount={this.options.length + 1}
                columnCount={this.columns.length}
                hideTopRightGridScrollbar={true}
              />
            )}
          </Observer>
        )}
      </AutoSizer>
    );
  }
}

@inject("gridPaneBacking")
@observer
export class ComboGridEditor extends React.Component<{
  value: ICellValue | undefined;
  editingRecordId: IRecordId;
  editingFieldId: IFieldId;
  editingRecord: IDataTableRecord;
  editingField: IDataTableFieldStruct;
  cursorPosition: { top: number; left: number; height: number; width: number };
  gridPaneBacking?: IGridPanelBacking;
  onKeyDown?: (event: any) => void;
  onDataCommit?: (
    dirtyValue: ICellValue,
    editingRecordId: IRecordId,
    editingFieldId: IFieldId
  ) => void;
}> {
  private hDropdownRepositionInterval: any;

  public componentDidMount() {
    this.hDropdownRepositionInterval = setInterval(() => {
      this.refMeasure.current && (this.refMeasure.current as any).measure();
    }, 1000);
    runInAction(() => {
      this.dirtyValue = (this.props.value !== undefined
        ? this.props.value
        : "") as string;
      this.elmInput!.focus();
      setTimeout(() => {
        this.elmInput && this.elmInput.select();
      }, 10);
    });
  }

  private elmInput: HTMLInputElement | null;
  @observable
  private dirtyValue: string = "";
  @observable
  private dirtyEditedValue: string = "";
  @observable
  private searchText: string = "";
  @observable
  private isDirty: boolean = false;
  @observable
  private isDirtyEdited: boolean = false;
  @observable
  private isDroppedDown = false;
  private editingCanceled: boolean = false;

  @action.bound
  private refInput(elm: HTMLInputElement) {
    this.elmInput = elm;
  }

  @action.bound
  private handleEditChange(event: any) {
    this.dirtyEditedValue = event.target.value;
    this.isDirtyEdited = true;
    this.isDroppedDown = true;
    this.setSearchText(this.dirtyEditedValue);
  }

  private setSearchText = _.throttle(this.setSearchTextImm, 1000);

  @action.bound
  private setSearchTextImm(searchText: string) {
    this.searchText = searchText;
  }

  public componentWillUnmount() {
    if (this.isDirty && !this.editingCanceled) {
      console.log("Commit data:", this.dirtyValue);
      this.props.onDataCommit &&
        this.props.onDataCommit(
          this.dirtyValue,
          this.props.editingRecordId,
          this.props.editingFieldId
        );
    }
    this.hDropdownRepositionInterval &&
      clearInterval(this.hDropdownRepositionInterval);
  }

  @action.bound
  private handleKeyDown(event: any) {
    if (event.key === "Escape") {
      this.editingCanceled = true;
    }
    this.props.onKeyDown && this.props.onKeyDown(event);
  }

  @action.bound
  private handleItemSelected(optionId: string | undefined) {
    this.dirtyValue = optionId || "";
    this.isDirty = true;
    this.isDirtyEdited = false;
    this.isDroppedDown = false;
  }

  @computed
  public get lookedUpValue() {
    if (!this.dirtyValue) {
      return "";
    }
    return this.props
      .gridPaneBacking!.dataTableSelectors.lookupResolverProvider.get(
        this.props.editingField.lookupId
      )
      .getLookedUpValue(this.dirtyValue);
  }

  @computed
  private get textFieldValue() {
    if (this.isDirtyEdited) {
      return this.dirtyEditedValue;
    } else {
      return this.lookedUpValue;
    }
  }

  @action.bound
  private handleEditFieldClick(event: any) {
    this.isDroppedDown = true;
  }

  public refMeasure = React.createRef<Measure>();

  public render() {
    return (
      <Measure ref={this.refMeasure} bounds={true} offset={true}>
        {({ measureRef, contentRect }) => (
          <Observer>
            {() => (
              <div
                ref={measureRef}
                style={{
                  width: "100%",
                  height: "100%",
                  border: "none",
                  padding: "0px 0px 0px 0px",
                  margin: 0,
                  position: "relative",
                  zIndex: 1500
                }}
              >
                <input
                  onKeyDown={this.handleKeyDown}
                  ref={this.refInput}
                  style={{
                    width: "100%",
                    height: "100%",
                    border: "none",
                    padding: "0px 0px 0px 15px",
                    margin: 0
                  }}
                  value={this.textFieldValue}
                  onChange={this.handleEditChange}
                  onClick={this.handleEditFieldClick}
                />
                <div
                  style={{
                    position: "absolute",
                    right: 3,
                    top: 0,
                    height: "100%",
                    display: "flex",
                    alignItems: "center"
                  }}
                >
                  <i className="fa fa-caret-down" />
                </div>
                {this.isDroppedDown && (
                  <div
                    style={{
                      position: "fixed",
                      width: 300,
                      height: 400,
                      top:
                        contentRect.bounds && contentRect.offset
                          ? contentRect.bounds.top + contentRect.offset.height
                          : 0,
                      left: contentRect.bounds ? contentRect.bounds.left : 0,
                      zIndex: 1000,
                      backgroundColor: "white",
                      border: "1px solid #aaaaaa",
                      borderRadius: 3
                    }}
                  >
                    <LookupDropdown
                      {...this.props}
                      searchText={this.searchText}
                      onItemSelected={this.handleItemSelected}
                    />
                  </div>
                )}
              </div>
            )}
          </Observer>
        )}
      </Measure>
    );
  }
}
