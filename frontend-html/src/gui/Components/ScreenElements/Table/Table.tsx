/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { CtxPanelVisibility } from "gui/contexts/GUIContexts";
import { action, autorun, comparer, computed, observable, runInAction } from "mobx";
import { MobXProviderContext, Observer, observer } from "mobx-react";
import { ITablePanelView } from "model/entities/TablePanelView/types/ITablePanelView";
import { IProperty } from "model/entities/types/IProperty";
import { getIsSelectionCheckboxesShown } from "model/selectors/DataView/getIsSelectionCheckboxesShown";
import { getProperties } from "model/selectors/DataView/getProperties";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";
import * as React from "react";
import ReactDOM from "react-dom";
import Measure, { BoundingRect } from "react-measure";
import { Canvas } from "gui/Components/ScreenElements/Table/Canvas";
import { HeaderRow } from "gui/Components/ScreenElements/Table/HeaderRow";
import { PositionedField } from "gui/Components/ScreenElements/Table/PositionedField";
import Scrollee from "gui/Components/ScreenElements/Table/Scrollee";
import Scroller from "gui/Components/ScreenElements/Table/Scroller";
import S from "gui/Components/ScreenElements/Table/Table.module.scss";
import { getTooltip, handleTableClick, handleTableMouseMove } from "gui/Components/ScreenElements/Table/TableRendering/onClick";
import { renderTable } from "gui/Components/ScreenElements/Table/TableRendering/renderTable";
import { IClickSubsItem, IMouseOverSubsItem, ITableRow, ITooltipData } from "gui/Components/ScreenElements/Table/TableRendering/types";
import { IGridDimensions, ITableProps } from "gui/Components/ScreenElements/Table/types";
import { DragDropContext, Droppable } from "react-beautiful-dnd";
import { onColumnOrderChangeFinished } from "model/actions-ui/DataView/TableView/onColumnOrderChangeFinished";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IFocusable } from "model/entities/FormFocusManager";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getSessionId } from "model/selectors/getSessionId";
import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import cx from 'classnames';
import { getGridFocusManager } from "model/entities/GridFocusManager";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";

function createTableRenderer(ctx: any, gridDimensions: IGridDimensions) {
  const groupedColumnSettings = computed(
    () => getGroupingConfiguration(ctx).orderedGroupingColumnSettings
  );
  const properties = observable<IProperty>(getProperties(ctx));
  const propertyById = computed(
    () => new Map(properties.map((property) => [property.id, property]))
  );

  const tableColumnIds = computed(() => getTableViewProperties(ctx).map((prop) => prop.id));

  const scrollTopObs = observable.box<number>(0);
  const scrollLeftObs = observable.box<number>(0);
  const viewportWidthObs = observable.box<number>(500);
  const viewportHeightObs = observable.box<number>(300);

  const clickSubscriptions: IClickSubsItem[] = [];
  const mouseOverSubscriptions: IMouseOverSubsItem[] = [];
  const mouseMoveSubscriptions: IClickSubsItem[] = [];

  const isCheckBoxedTable = getIsSelectionCheckboxesShown(ctx);

  function drawTable(
    ctx2d: CanvasRenderingContext2D,
    fixedColumnCount: number,
    tableRows: ITableRow[]
  ) {
    renderTable(
      ctx,
      ctx2d,
      tableRows,
      groupedColumnSettings.get().map((settings) => settings.columnId),
      tableColumnIds.get(),
      propertyById.get(),
      scrollLeftObs.get(),
      scrollTopObs.get(),
      viewportWidthObs.get(),
      viewportHeightObs.get(),
      isCheckBoxedTable,
      gridDimensions.displayedColumnDimensionsCom,
      gridDimensions.columnWidths,
      fixedColumnCount,
      clickSubscriptions,
      mouseMoveSubscriptions,
      mouseOverSubscriptions,
      gridDimensions.rowHeight
    );
  }

  function setScroll(scrollLeft: number, scrollTop: number) {
    runInAction(() => {
      scrollTopObs.set(scrollTop);
      scrollLeftObs.set(scrollLeft);
    });
  }

  async function handleClick(event: any) {
    const domRect = event.target.getBoundingClientRect();
    const handlingResult = await handleTableClick(
      event,
      event.clientX - domRect.x,
      event.clientY - domRect.y,
      scrollLeftObs.get(),
      scrollTopObs.get(),
      clickSubscriptions
    );
    return handlingResult;
  }

  function handleMouseMove(event: any) {
    const domRect = event.target.getBoundingClientRect();
    const handlingResult = handleTableMouseMove(
      event,
      event.clientX - domRect.x,
      event.clientY - domRect.y,
      scrollLeftObs.get(),
      scrollTopObs.get(),
      mouseMoveSubscriptions
    );
    if (!handlingResult.handled) {
      getTablePanelView(ctx).onMouseMoveOutsideCells();
    }
    return handlingResult;
  }

  function setViewportSize(width: number, height: number) {
    runInAction(() => {
      viewportWidthObs.set(width);
      viewportHeightObs.set(height);
    });
  }

  function getTooltipContent(event: any, boundingRectangle: DOMRect) {
    return getTooltip(
      event.clientX - boundingRectangle.x + scrollLeftObs.get(),
      event.clientY - boundingRectangle.y + scrollTopObs.get(),
      mouseOverSubscriptions
    );
  }

  return {drawTable, setScroll, handleClick, handleMouseMove, setViewportSize, getTooltipContent: getTooltipContent};
}

export const Table: React.FC<ITableProps & {
  refTable(elm: RawTable | null): void;
}> = (props) => {
  const ctxPanelVisibility = React.useContext(CtxPanelVisibility);
  return <RawTable {...props} isVisible={ctxPanelVisibility.isVisible} ref={props.refTable}/>;
};

@observer
export class RawTable extends React.Component<ITableProps & { isVisible: boolean }> {
  static contextType = MobXProviderContext;

  tableRenderer = createTableRenderer(this.context.tablePanelView, this.props.gridDimensions);

  @observable _contentBounds: BoundingRect = {
    top: 0,
    left: 0,
    bottom: 0,
    right: 0,
    width: 0,
    height: 0,
  };
  @computed({equals: comparer.structural}) get contentBounds() {
    return this._contentBounds;
  }

  set contentBounds(value: BoundingRect) {
    this._contentBounds = value;
  }

  @observable.ref elmCanvasFixed: Canvas | null = null;
  @observable.ref elmCanvasMoving: Canvas | null = null;

  @observable.ref elmCanvasElement: HTMLCanvasElement | null = null;
  @observable.ref ctxCanvas: CanvasRenderingContext2D | null = null;

  @observable.ref elmScroller: Scroller | null = null;

  elmMeasure: Measure | null = null;

  @action.bound handleWindowClick(event: any) {
    const domNode = ReactDOM.findDOMNode(this.elmScroller);
    if (domNode && !domNode.contains(event.target)) {
      this.props.onOutsideTableClick && this.props.onOutsideTableClick(event);
    }
  }

  @action.bound refCanvasFixed(elm: Canvas | null) {
    this.elmCanvasFixed = elm;
  }

  @action.bound refCanvasMoving(elm: Canvas | null) {
    this.elmCanvasMoving = elm;
    this.props.refCanvasMovingComponent && this.props.refCanvasMovingComponent(elm);
  }

  @action.bound refCanvasElement(elm: HTMLCanvasElement | null) {
    this.elmCanvasElement = elm;
    if (elm) {
      this.ctxCanvas = elm.getContext("2d");
    } else {
      this.ctxCanvas = null;
    }
  }

  @action.bound refMeasure(elm: Measure | null) {
    this.elmMeasure = elm;
  }

  @action.bound refScroller(elm: Scroller | null) {
    this.elmScroller = elm;
    if (elm) {
      this.props.scrollState.scrollToFunction = elm.scrollTo;
      window.addEventListener("click", this.handleWindowClick);
    } else {
      window.removeEventListener("click", this.handleWindowClick);
    }
  }

  disposers: Array<() => void> = [];

  componentDidMount() {
    this.disposers.push(
      autorun(
        () => {
          if (this.ctxCanvas) {
            this.tableRenderer.drawTable(
              this.ctxCanvas,
              this.props.fixedColumnCount,
              this.props.tableRows
            );
          }
        },
        {
          scheduler(fn) {
            requestAnimationFrame(fn);
          },
        }
      )
    );
  }

  componentDidUpdate(prevProps: ITableProps & { isVisible: boolean }) {
    if (this.props.isVisible !== prevProps.isVisible) {
      this.remeasureCellArea();
    }
  }

  componentWillUnmount() {
    this.disposers.forEach((d) => d());
  }

  @action.bound remeasureCellArea() {
    if (this.elmMeasure && (this.elmMeasure as any).measure) {
      (this.elmMeasure as any).measure();
    }
  }

  @action.bound focusTable() {
    this.elmScroller && this.elmScroller.focus();
  }

  @action.bound scrollToCellShortest(rowIdx: number, dataColumnIndex: number) {
    // TODO: Refactor to take real scrollbar sizes
    //const freeColIndex = columnIdx + this.fixedColumnCount;
    const {gridDimensions} = this.props;
    const SCROLLBAR_SIZE = 24;
    const ROW_HEIGHT = 25;
    if (this.elmScroller) {
      const top = gridDimensions.getRowTop(rowIdx);
      const bottom = gridDimensions.getRowBottom(rowIdx);
      const left = gridDimensions.getColumnLeft(dataColumnIndex);
      const right = gridDimensions.getColumnRight(dataColumnIndex);

      if (left - this.elmScroller.scrollLeft < this.fixedColumnsWidth) {
        this.elmScroller.scrollTo({
          scrollLeft: left - this.fixedColumnsWidth,
        });
      }
      if (right - this.elmScroller.scrollLeft > this.contentBounds.width - SCROLLBAR_SIZE) {
        this.elmScroller.scrollTo({
          scrollLeft: right - this.contentBounds.width + SCROLLBAR_SIZE,
        });
      }
      if (top - this.elmScroller.scrollTop < 0) {
        this.elmScroller.scrollTo({scrollTop: top});
      }
      if (
        bottom - this.elmScroller.scrollTop > this.contentBounds.height - 
        SCROLLBAR_SIZE
      ) {
        this.elmScroller.scrollTo({
          scrollTop: bottom - this.contentBounds.height + SCROLLBAR_SIZE + ROW_HEIGHT ,
        });
      }
    }
  }

  get fixedColumnCount() {
    return this.fixedHeaderContainers.length;
  }

  get hasFixedColumns() {
    return this.fixedHeaderContainers.length !== 0;
  }

  @computed get fixedColumnsWidth() {
    return this.fixedHeaderContainers
      .map((container) => container.width)
      .reduce((x, y) => x + y, 0);
  }

  @computed get freeHeaderContainers() {
    return this.props.headerContainers.filter((container) => !container.isFixed);
  }

  @computed get freeHeaders() {
    return this.freeHeaderContainers.map((container) => container.header);
  }

  @computed get fixedHeaderContainers() {
    return this.props.headerContainers.filter((container) => container.isFixed);
  }

  @computed get fixedHeaders() {
    return this.fixedHeaderContainers.map((container) => container.header);
  }

  get tablePanelView() {
    return this.context.tablePanelView as ITablePanelView;
  }

  @observable
  tooltipData: ITooltipData | undefined = undefined;

  @action.bound onMouseOver(event: any, boundingRectangle: DOMRect) {
    this.tooltipData = undefined;
    setTimeout(() => {
      runInAction(() => {
        this.mouseInTooltipEnabledArea = true;
      });
    });
  }

  @action.bound
  handleScrollerMouseMove(event: any) {
    this.tableRenderer.handleMouseMove(event);
  }

  @observable
  mouseInTooltipEnabledArea = true;

  @action.bound onMouseLeaveTooltipEnabledArea(event: any) {
    this.tablePanelView.currentTooltipText = undefined;
    this.mouseInTooltipEnabledArea = false;
  }

  @action.bound handleScrollerClick(event: any) {
    const self = this;
    runGeneratorInFlowWithHandler({
      ctx: this.context.tablePanelView,
      generator: function* (){
        const handled = yield self.tableRenderer.handleClick(event);
        if (!self.tablePanelView.isEditing || !handled) {
          self.focusTable();
          let dataView = getDataView(self.context.tablePanelView);
          if (getFormScreenLifecycle(dataView).focusedDataViewId === dataView.id && dataView.selectedRowId) {
            yield*getRecordInfo(dataView).onSelectedRowMaybeChanged(
              getMenuItemId(dataView),
              getDataStructureEntityId(dataView),
              dataView.selectedRowId,
              getSessionId(dataView)
            );
          }
        }
        if (!handled) {
          self.props.onOutsideTableClick?.(event);
        }
      }()
    });
  }

  @action.bound handleResize(contentRect: { bounds: BoundingRect }) {
    this.contentBounds = contentRect.bounds;
    this.props.onContentBoundsChanged(contentRect.bounds);
    this.tableRenderer.setViewportSize(this.contentBounds.width, this.contentBounds.height);
  }

  @action.bound handleScroll(event: any, scrollLeft: number, scrollTop: number) {
    this.context.tablePanelView.handleTableScroll(event, scrollTop, scrollLeft);
    this.props.scrollState.setScrollOffset(event, scrollTop, scrollLeft);
    this.tableRenderer.setScroll(scrollLeft, scrollTop);
  }

  onColumnDragEnd(result: any) {
    if (!result.destination) {
      return;
    }
    let destinationHeaderIndex = Math.floor(result.destination.index); // separators must also have indices (1.5, 2.5, 3.5...)
    onColumnOrderChangeFinished(
      this.context.tablePanelView,
      this.context.tablePanelView.tableProperties[result.source.index].id,
      this.context.tablePanelView.tableProperties[destinationHeaderIndex].id
    );
  }

  onFocus(event: any){
    if(event.target){
      let dataView = getDataView(this.context.tablePanelView);
      dataView.gridFocusManager.setLastFocusedFilter(event.target! as IFocusable);
    }
  }

  isCursorIconPointer(): boolean {
    return !!this.tablePanelView.property?.isLink
    && this.tablePanelView.ctrlPressed
    && this.tablePanelView.currentTooltipText !== undefined;
  }

  canFocus(){
    const formScreen = getFormScreen(this.tablePanelView);
    const childBindings = formScreen.rootDataViews[0].childBindings;
    const noEditorInChildViewsOpen = childBindings.every(x => x.childDataView.gridFocusManager.canFocusTable)
    return getGridFocusManager(this.context.tablePanelView).canFocusTable && noEditorInChildViewsOpen;
  }

  render() {
    const editorCellRectangle =
      this.props.editingRowIndex !== undefined && this.props.editingColumnIndex !== undefined
        ? this.tablePanelView.getCellRectangle(
          this.props.editingRowIndex,
          this.props.editingColumnIndex
        )
        : undefined;

    return (
      <div className={`${S.table} tableContainer`}>
        {this.props.isLoading && (
          <div className={S.loadingOverlay}>
            <div className={S.loadingIcon}>
              <i className="far fa-clock fa-7x blink"/>
            </div>
          </div>
        )}
        <Measure ref={this.refMeasure} bounds={true} onResize={this.handleResize}>
          {({measureRef, contentRect}) => (
            <Observer>
              {() => (
                <>
                  {this.props.headerContainers &&
                  (contentRect.bounds!.width ? (
                    <div
                      onFocus={(event) => this.onFocus(event)}
                      className={S.headers}>
                      {this.hasFixedColumns ? (
                        <Scrollee
                          scrollOffsetSource={this.props.scrollState}
                          fixedHoriz={true}
                          fixedVert={true}
                          width={this.fixedColumnsWidth}
                          zIndex={101}
                        >
                          <HeaderRow headerElements={this.fixedHeaders} zIndex={100}/>
                        </Scrollee>
                      ) : null}
                      <Scrollee
                        scrollOffsetSource={this.props.scrollState}
                        fixedVert={true}
                        zIndex={100}
                        width={contentRect.bounds!.width - 10 - this.fixedColumnsWidth}
                        controlScrollStateByFocus={true}
                        controlScrollStateSelector=".tableContainer"
                        controlScrollStatePadding={{ left: 40, right: 40 }}
                      >
                        <DragDropContext onDragEnd={(result) => this.onColumnDragEnd(result)}>
                          <Droppable droppableId="headers" direction="horizontal" >
                            {(provided) => (
                              // width:"10000%" - when the table was horizontally scrolled to the right the drag
                              // and drop did not work on the headers that were outside of the originally visible area.
                              // This div has to be at least as wide as ALL its children, not just the visible ones.
                              <div style={{width:"10000%"}}  {...provided.droppableProps} ref={provided.innerRef}>
                                <HeaderRow headerElements={[...this.freeHeaders, <div key={"placeholder"}>{provided.placeholder}</div> as any]}/>
                              </div>
                            )}
                          </Droppable>
                        </DragDropContext>
                      </Scrollee>
                    </div>
                  ) : null)}

                  <div
                    ref={measureRef}
                    className={cx("cellAreaContainer", S.cellAreaContainer, (this.isCursorIconPointer()) ? ["isLink", S.isLink] : "")}
                    title={this.tablePanelView.currentTooltipText}
                  >
                    <>
                      {contentRect.bounds!.height ? (
                        <div className={S.canvasRow}>
                          <Canvas
                            refCanvasElement={this.refCanvasElement}
                            width={contentRect.bounds!.width}
                            height={contentRect.bounds!.height}
                          />
                        </div>
                      ) : null}
                      {this.props.isEditorMounted && editorCellRectangle && (
                        <PositionedField
                          fixedColumnsCount={this.fixedColumnCount}
                          rowIndex={this.props.editingRowIndex!}
                          columnIndex={this.props.editingColumnIndex!}
                          scrollOffsetSource={this.props.scrollState}
                          worldBounds={contentRect.bounds!}
                          cellRectangle={editorCellRectangle!}
                          onMouseEnter={(event) => this.onMouseLeaveTooltipEnabledArea(event)}
                        >
                          {this.props.renderEditor && this.props.renderEditor()}
                        </PositionedField>
                      )}
                      <Scroller
                        ref={this.refScroller}
                        width={contentRect.bounds!.width}
                        height={contentRect.bounds!.height}
                        isVisible={true}
                        scrollingDisabled={false /*this.props.isEditorMounted*/}
                        contentWidth={this.props.gridDimensions.contentWidth + 20}
                        // +30px to make the last row visible on some dirty browsers
                        contentHeight={this.props.gridDimensions.contentHeight + 30}
                        onScroll={this.handleScroll}
                        onClick={this.handleScrollerClick}
                        onMouseMove={this.handleScrollerMouseMove}
                        onMouseOver={this.onMouseOver}
                        onMouseLeave={(event) => this.onMouseLeaveTooltipEnabledArea(event)}
                        onKeyDown={this.props.onKeyDown}
                        onFocus={this.props.onFocus}
                        canFocus={()=>this.canFocus()}
                      />
                    </>
                  </div>
                </>
              )}
            </Observer>
          )}
        </Measure>
      </div>
    );
  }
}
