import bind from "bind-decorator";
import {action, autorun, computed, observable} from "mobx";
import {inject, observer, Provider} from "mobx-react";
import {onTableKeyDown} from "model/actions-ui/DataView/TableView/onTableKeyDown";
import React from "react";
import {onColumnHeaderClick} from "../../../../model/actions-ui/DataView/TableView/onColumnHeaderClick";
import {ITablePanelView} from "../../../../model/entities/TablePanelView/types/ITablePanelView";
import {IDataView} from "../../../../model/entities/types/IDataView";
import {IProperty} from "../../../../model/entities/types/IProperty";
import {getColumnHeaders} from "../../../../model/selectors/TablePanelView/getColumnHeaders";
import {getIsEditing} from "../../../../model/selectors/TablePanelView/getIsEditing";
import {getSelectedColumnIndex} from "../../../../model/selectors/TablePanelView/getSelectedColumnIndex";
import {getTableViewProperties} from "../../../../model/selectors/TablePanelView/getTableViewProperties";
import {IColumnHeader} from "../../../../model/selectors/TablePanelView/types";
import {ITableColumnsConf} from "../../../Components/Dialogs/ColumnsDialog";
import {FilterSettings} from "../../../Components/ScreenElements/Table/FilterSettings/FilterSettings";
import {Header} from "../../../Components/ScreenElements/Table/Header";
import {SimpleScrollState} from "../../../Components/ScreenElements/Table/SimpleScrollState";
import {RawTable, Table} from "../../../Components/ScreenElements/Table/Table";
import {IGridDimensions} from "../../../Components/ScreenElements/Table/types";
import {CellRenderer} from "./CellRenderer";
import {TableViewEditor} from "./TableViewEditor";
import {getSelectedRowIndex} from "model/selectors/DataView/getSelectedRowIndex";
import {onNoCellClick} from "model/actions-ui/DataView/TableView/onNoCellClick";
import {onOutsideTableClick} from "model/actions-ui/DataView/TableView/onOutsideTableClick";
import {getFixedColumnsCount} from "model/selectors/TablePanelView/getFixedColumnsCount";
import {getIsSelectionCheckboxesShown} from "model/selectors/DataView/getIsSelectionCheckboxesShown";
import {onColumnWidthChanged} from "model/actions-ui/DataView/TableView/onColumnWidthChanged";
import {onColumnWidthChangeFinished} from "model/actions-ui/DataView/TableView/onColumnWidthChangeFinished";
import {onColumnOrderChangeFinished} from "model/actions-ui/DataView/TableView/onColumnOrderChangeFinished";
import {getGroupingConfiguration} from "model/selectors/TablePanelView/getGroupingConfiguration";
import {getLeadingColumnCount} from "model/selectors/TablePanelView/getLeadingColumnCount";
import {getDataTable} from "../../../../model/selectors/DataView/getDataTable";
import {flattenToTableRows} from "../../../Components/ScreenElements/Table/TableRendering/tableRows";
import {getTablePanelView} from "../../../../model/selectors/TablePanelView/getTablePanelView";
import {getFormScreenLifecycle} from "../../../../model/selectors/FormScreen/getFormScreenLifecycle";
import {aggregationToString, IAggregation, parseAggregations} from "model/entities/types/IAggregation";
import {InfiniteScrollLoader, IInfiniteScrollLoader} from "./InfiniteScrollLoader";

@inject(({ dataView }) => {
  return {
    dataView,
    tablePanelView: dataView.tablePanelView,
    onColumnDialogCancel: dataView.tablePanelView.onColumnConfCancel,
    onColumnDialogOk: dataView.tablePanelView.onColumnConfSubmit,

    onTableKeyDown: onTableKeyDown(dataView)
  };
})
@observer
export class TableView extends React.Component<{
  dataView?: IDataView;
  tablePanelView?: ITablePanelView;
  onColumnDialogCancel?: (event: any) => void;
  onColumnDialogOk?: (event: any, configuration: ITableColumnsConf) => void;
  onTableKeyDown?: (event: any) => void;
}> {

  infiniteScrollLoader: IInfiniteScrollLoader;

  constructor(props: any) {
    super(props);
    this.infiniteScrollLoader = new InfiniteScrollLoader({
      dataView: this.props.dataView!,
      gridDimensions: this.gDim,
      scrollState: this.scrollState,
    });
    getFormScreenLifecycle(this.props.dataView).registerDisposer(this.infiniteScrollLoader.start());
  }

  refTableDisposer: any;
  refTable = (elmTable: RawTable | null) => {
    this.elmTable = elmTable;
    if (elmTable) {
      const d1 = this.props.tablePanelView!.subOnFocusTable(
        elmTable.focusTable
      );
      const d2 = this.props.tablePanelView!.subOnScrollToCellShortest(
        elmTable.scrollToCellShortest
      );
      this.refTableDisposer = () => {
        d1();
        d2();
      };
    } else {
      this.refTableDisposer && this.refTableDisposer();
    }
  };

  @computed get tableRows(){
    const groupedColumnIds = getGroupingConfiguration(this.props.dataView).orderedGroupingColumnIds;
    return groupedColumnIds.length === 0
        ? getDataTable(this.props.dataView).rows
        : flattenToTableRows(getDataTable(this.props.dataView).groups);
  }


  elmTable: RawTable | null = null;
  gDim = new GridDimensions({
    getTableViewProperties: () => getTableViewProperties(this.props.dataView),
    getRowCount: () => this.tableRows.length,
    getIsSelectionCheckboxes: () =>
      getIsSelectionCheckboxesShown(this.props.tablePanelView),
    ctx: this.props.dataView
  });

  headerRenderer = new HeaderRenderer({
    gridDimensions: this.gDim,
    tablePanelView: this.props.tablePanelView!,
    getFixedColumnCount: () => getFixedColumnsCount(this.props.tablePanelView),
    getIsSelectionCheckboxes: () =>
      getIsSelectionCheckboxesShown(this.props.tablePanelView),
    dataView: this.props.dataView!,
    getColumnHeaders: () => getColumnHeaders(this.props.dataView),
    getTableViewProperties: () => getTableViewProperties(this.props.dataView),
    onColumnWidthChange: (cid, nw) =>
      onColumnWidthChanged(this.props.tablePanelView)(cid, nw),
    onColumnOrderChange: (id1, id2) =>
      onColumnOrderChangeFinished(this.props.tablePanelView)(id1, id2),
    onColumnOrderAttendantsChange: (
      idSource: string | undefined,
      idTarget: string | undefined
    ) =>
      this.props.tablePanelView!.setColumnOrderChangeAttendants(
        idSource,
        idTarget
      )
  });

  scrollState = new SimpleScrollState(0, 0);
  cellRenderer = new CellRenderer({
    tablePanelView: this.props.tablePanelView!
  });

  render() {
    const self = this;
    const isSelectionCheckboxes = getIsSelectionCheckboxesShown(
      this.props.tablePanelView
    );
    const editingRowIndex = getSelectedRowIndex(this.props.tablePanelView);
    let editingColumnIndex = getSelectedColumnIndex(this.props.tablePanelView);
    if (editingColumnIndex !== undefined && isSelectionCheckboxes) {
      editingColumnIndex++;
    }
   const fixedColumnCount = this.props.tablePanelView ? this.props.tablePanelView.fixedColumnCount || 0 : 0;

    return (
      <Provider tablePanelView={this.props.tablePanelView}>
        <>
          <Table
            tableRows={this.tableRows}
            gridDimensions={self.gDim}
            scrollState={self.scrollState}
            editingRowIndex={editingRowIndex}
            editingColumnIndex={editingColumnIndex}
            isEditorMounted={getIsEditing(this.props.tablePanelView)}
            isLoading={false}
            fixedColumnCount={fixedColumnCount}
            headerContainers = {self.headerRenderer.headerContainers}
            renderCell={self.cellRenderer.renderCell}
            renderEditor={() => (
              <TableViewEditor
                key={`${editingRowIndex}@${editingColumnIndex}`}
              />
            )}
            onNoCellClick={onNoCellClick(this.props.tablePanelView)}
            onOutsideTableClick={onOutsideTableClick(this.props.tablePanelView)}
            onContentBoundsChanged={bounds => this.infiniteScrollLoader.contentBounds=bounds}
            refCanvasMovingComponent={this.props.tablePanelView!.setTableCanvas}
            onKeyDown={this.props.onTableKeyDown}
            refTable={this.refTable}
          />
        </>
      </Provider>
    );
  }
}

interface IGridDimensionsData {
  getTableViewProperties: () => IProperty[];
  getRowCount: () => number;
  getIsSelectionCheckboxes: () => boolean;
  ctx: any;
}

class GridDimensions implements IGridDimensions {
  constructor(data: IGridDimensionsData) {
    Object.assign(this, data);
  }

  @computed get columnWidths(): Map<string, number> {
    return new Map(
      this.tableViewProperties.map(prop => [prop.id, prop.columnWidth])
    );
  }

  getTableViewProperties: () => IProperty[] = null as any;
  getRowCount: () => number = null as any;
  getIsSelectionCheckboxes: () => boolean = null as any;
  ctx: any;

  @computed get isSelectionCheckboxes() {
    return this.getIsSelectionCheckboxes();
  }

  @computed get tableViewPropertiesOriginal() {
    return this.getTableViewProperties();
  }

  @computed get tableViewProperties() {
    return this.tableViewPropertiesOriginal;
  }

  @computed get rowCount() {
    return this.getRowCount();
  }

  @computed get columnCount() {
    return (
      this.tableViewProperties.length
    );
  }

  get contentWidth() {
    return this.getColumnRight(this.columnCount - 1);
  }

  get contentHeight() {
    return this.getRowBottom(this.rowCount - 1);
  }

  getColumnLeft(dataColumnIndex: number): number {
    const displayedColumnIndex = this.dataColumnIndexToDisplayedIndex(dataColumnIndex);
    return this.displayedColumnDimensionsCom[displayedColumnIndex].left;
  }

  getColumnRight(dataColumnIndex: number): number {
    const displayedColumnIndex = this.dataColumnIndexToDisplayedIndex(dataColumnIndex);
    return this.displayedColumnDimensionsCom[displayedColumnIndex].right;
  }

  dataColumnIndexToDisplayedIndex(dataColumnIndex: number){
    return dataColumnIndex + getLeadingColumnCount(this.ctx);
  }

  getRowTop(rowIndex: number): number {
    return rowIndex * this.getRowHeight(rowIndex);
  }

  getRowHeight(rowIndex: number): number {
    return 20;
  }

  getRowBottom(rowIndex: number): number {
    return this.getRowTop(rowIndex) + this.getRowHeight(rowIndex);
  }

  @action.bound setColumnWidth(columnId: string, newWidth: number) {
    this.columnWidths.set(columnId, Math.max(newWidth, 20));
  }

  @computed get displayedColumnDimensionsCom():{left: number; width: number; right: number;}[] {
    const isCheckBoxedTable = getIsSelectionCheckboxesShown(this.ctx);
    const groupedColumnIds = getGroupingConfiguration(this.ctx).orderedGroupingColumnIds;
    const tableColumnIds =  getTableViewProperties(this.ctx).map(prop => prop.id)
    const columnWidths = this.columnWidths;

    const widths = Array.from(
      (function* () {
        if (isCheckBoxedTable) yield 20;
        yield* groupedColumnIds.map((id) => 20);
        yield* tableColumnIds
          .map((id) => columnWidths.get(id))
          .filter((width) => width !== undefined) as number[];
      })()
    );
    let acc = 0;
    return Array.from(
      (function* () {
        for (let w of widths) {
          yield {
            left: acc,
            width: w,
            right: acc + w,
          };
          acc = acc + w;
        }
      })()
    );
  }

}

interface IHeaderRendererData {
  getTableViewProperties: () => IProperty[];

  tablePanelView: ITablePanelView;
  getColumnHeaders: () => IColumnHeader[];
  getIsSelectionCheckboxes: () => boolean;
  onColumnWidthChange: (id: string, newWidth: number) => void;
  onColumnOrderChange: (id: string, targetId: string) => void;
  onColumnOrderAttendantsChange: (
    idSource: string | undefined,
    idTarget: string | undefined
  ) => void;
  dataView: IDataView;
  gridDimensions: IGridDimensions;
  getFixedColumnCount: () => number;
}

class HeaderRenderer implements IHeaderRendererData {
  constructor(data: IHeaderRendererData) {
    Object.assign(this, data);
    const disposer = this.start();
    getFormScreenLifecycle(this.dataView).registerDisposer(disposer);
  }
  gridDimensions: IGridDimensions  = null as any;
  getTableViewProperties: () => IProperty[] = null as any;
  getIsSelectionCheckboxes: () => boolean = null as any;
  dataView: IDataView = null as any;
  getFixedColumnCount = null as any;
  @observable aggregationData: IAggregation[] = []

  @computed get tableViewPropertiesOriginal() {
    return this.getTableViewProperties();
  }

  @computed get tableViewProperties() {
    return this.tableViewPropertiesOriginal;
  }

  getColumnHeaders: () => IColumnHeader[] = null as any;
  onColumnWidthChange: (id: string, newWidth: number) => void = null as any;
  onColumnOrderChange: (id: string, targetId: string) => void = null as any;
  onColumnOrderAttendantsChange: (
    idSource: string | undefined,
    idTarget: string | undefined
  ) => void = null as any;
  tablePanelView: ITablePanelView = null as any;

  columnOrderChangeSourceId: string | undefined;
  columnOrderChangeTargetId: string | undefined;

  @computed get columnHeaders() {
    return this.getColumnHeaders();
  }

  @computed get isSelectionCheckboxes() {
    return this.getIsSelectionCheckboxes();
  }

  @observable isColumnOrderChanging = false;

  @action.bound handleStartColumnOrderChanging(id: string) {
    this.isColumnOrderChanging = true;
    this.columnOrderChangeSourceId = id;
    this.onColumnOrderAttendantsChange(
      this.columnOrderChangeSourceId,
      this.columnOrderChangeTargetId
    );
  }

  @action.bound handleStopColumnOrderChanging(id: string) {
    this.isColumnOrderChanging = false;
    this.columnOrderChangeSourceId = undefined;
    this.columnOrderChangeTargetId = undefined;
    this.onColumnOrderAttendantsChange(
      this.columnOrderChangeSourceId,
      this.columnOrderChangeTargetId
    );
  }

  @action.bound handlePossibleColumnOrderChange(targetId: string | undefined) {
    this.columnOrderChangeTargetId = targetId;
    this.onColumnOrderAttendantsChange(
      this.columnOrderChangeSourceId,
      this.columnOrderChangeTargetId
    );
  }

  @action.bound handleColumnOrderDrop(targetId: string) {
    this.onColumnOrderChange(this.columnOrderChangeSourceId!, targetId);
    this.onColumnOrderAttendantsChange(
      this.columnOrderChangeSourceId,
      this.columnOrderChangeTargetId
    );
  }

  @computed get headerContainers(){
    const columnDimensions = this.gridDimensions.displayedColumnDimensionsCom;
    const leadingColumnCount = getLeadingColumnCount(this.dataView);
    const selectionCheckBoxesShown = getIsSelectionCheckboxesShown(this.dataView);
    const groupingColumnCount = leadingColumnCount - (selectionCheckBoxesShown ? 1 : 0)
    const dataColumnCount = columnDimensions.length - leadingColumnCount;
    const headerContainers = []

    if(selectionCheckBoxesShown){
      headerContainers.push(
        new HeaderContainer({
          header: this.renderDummyHeader(columnDimensions[0].width),
          isFixed: true,
          width: columnDimensions[0].width,
        }));
    }

    let columnsToFix = this.getFixedColumnCount();
    for(let i=0; i < groupingColumnCount; i++){
      headerContainers.push(
        new HeaderContainer({
          header: this.renderDummyHeader(columnDimensions[i].width),
          isFixed: columnsToFix > i,
          width: columnDimensions[i].width,
        }));
    }

    columnsToFix -= groupingColumnCount;
    for(let i=0; i < dataColumnCount; i++){
      const columnWidth = columnDimensions[i + leadingColumnCount].width;
      headerContainers.push(
        new HeaderContainer({
          header:  this.renderDataHeader({
            columnIndex: i ,
            columnWidth: columnWidth}),
          isFixed: columnsToFix > i,
          width: columnWidth,
        }));
    }

    return headerContainers;
  }

  renderDummyHeader(columnWidth: number){
    return <div style={{minWidth: columnWidth+"px"}}></div>;
  }

  @bind
  renderDataHeader(args:{columnIndex: number, columnWidth: number}) {
    const property = this.tableViewProperties[args.columnIndex];
    const header = this.columnHeaders[args.columnIndex];

    return (
      <Provider key={property.id} property={property}>
        <Header
          key={header.id}
          id={header.id}
          width={args.columnWidth}
          label={header.label}
          orderingDirection={header.ordering}
          orderingOrder={header.order}
          onColumnWidthChange={this.onColumnWidthChange}
          onColumnWidthChangeFinished={onColumnWidthChangeFinished(
            this.tablePanelView
          )}
          isColumnOrderChanging={this.isColumnOrderChanging}
          onColumnOrderDrop={this.handleColumnOrderDrop}
          onStartColumnOrderChanging={this.handleStartColumnOrderChanging}
          onStopColumnOrderChanging={this.handleStopColumnOrderChanging}
          onPossibleColumnOrderChange={this.handlePossibleColumnOrderChange}
          onClick={onColumnHeaderClick(this.tablePanelView)}
          additionalHeaderContent={this.makeAdditionalHeaderContent(header.id)}
        />
      </Provider>
    );
  }

  makeAdditionalHeaderContent(columnId: string){
    const filterControlsDisplayed = this.tablePanelView.filterConfiguration.isFilterControlsDisplayed;
    if(!filterControlsDisplayed && this.aggregationData.length === 0){
      return undefined;
    }
    const headerContent: JSX.Element[] =[]
    if(filterControlsDisplayed){
      headerContent.push(<FilterSettings />)
    }
    if(this.aggregationData.length !== 0){
      const aggregation = this.aggregationData.find(agg => agg.columnId === columnId)
      if(aggregation){
        headerContent.push(<div>{aggregationToString(aggregation)}</div>)
      }
    }
    return(() =>
        <>
          {headerContent}
        </>);
  }

  start(){
   return autorun(()=>{
      const aggregations = getTablePanelView(this.dataView).aggregations.aggregationList;
      if(aggregations.length === 0){
        this.aggregationData.length = 0;
        return
      }
      getFormScreenLifecycle(this.dataView)
        .loadAggregations(this.dataView, aggregations)
        .then(data => this.aggregationData = parseAggregations(data) || []);
    });
  }
}

export interface IHeaderContainer {
  header: JSX.Element;
  isFixed: boolean;
  width: number;
}

class HeaderContainer implements IHeaderContainer {
  constructor(data: IHeaderContainer) {
    Object.assign(this, data);
  }
  header: JSX.Element  = null as any;
  isFixed: boolean = null as any;
  width: number = null as any;
}
