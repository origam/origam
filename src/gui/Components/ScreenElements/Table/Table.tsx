import { action, computed, observable, comparer, autorun, runInAction } from "mobx";
import { observer, Observer, MobXProviderContext } from "mobx-react";
import * as React from "react";
import ReactDOM from "react-dom";
import Measure, { BoundingRect } from "react-measure";
import { Canvas } from "./Canvas";
import { HeaderRow } from "./HeaderRow";
import { PositionedField } from "./PositionedField";
import Scrollee from "./Scrollee";
import Scroller from "./Scroller";
import S from "./Table.module.css";
import { ITableProps, IGridDimensions } from "./types";
import { CtxPanelVisibility } from "gui02/contexts/GUIContexts";
import { IClickSubsItem } from "./TableRendering/types";
import { renderTable } from "./TableRendering/renderTable";
import { handleTableClick } from "./TableRendering/onClick";
import { getProperties } from "model/selectors/DataView/getProperties";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getIsSelectionCheckboxesShown } from "model/selectors/DataView/getIsSelectionCheckboxesShown";
import { IProperty } from "model/entities/types/IProperty";
import { flattenToTableRows } from "./TableRendering/tableRows";
import { isDataRow } from "./TableRendering/rowCells/dataRowCells";
import { isGroupRow } from "./TableRendering/rowCells/groupRowCells";

function createTableRenderer(ctx: any, gridDimensions: IGridDimensions) {
  /*const rootGroupsObs = observable([
    new GroupItem([], [], "Column 1", "Value 1"),
    new GroupItem(
      [
        new GroupItem(
          [],
          [
            [1, 2, 3, "A", "B", "C"],
            [4, 5, 6, "D", "E", "F"],
            [7, 8, 9, "G", "H", "I"],
            [10, 11, 12, "J", "K", "L"],
          ],
          "Column 2",
          "Value 1"
        ),
        new GroupItem([], [], "Column 2", "Value 2"),
        new GroupItem([], [], "Column 2", "Value 3"),
        new GroupItem(
          [],
          [
            [1, 2, 3, "A", "B", "C"],
            [4, 5, 6, "D", "E", "F"],
            [7, 8, 9, "G", "H", "I"],
            [10, 11, 12, "J", "K", "L"],
          ],
          "Column 2",
          "Value 4"
        ),
        new GroupItem([], [], "Column 2", "Value 5"),
      ],
      [],
      "Column 1",
      "Value 2"
    ),
    new GroupItem([], [], "Column 1", "Value 3"),
    new GroupItem([], [], "Column 1", "Value 4"),
    new GroupItem([], [], "Column 1", "Value 5"),
  ]);*/
  const groupedColumnIds = computed(() => getGroupingConfiguration(ctx).orderedGroupingColumnIds)

  const tableRowsCom = computed(() =>
    groupedColumnIds.get().length === 0 
      ? getDataTable(ctx).rows
      : flattenToTableRows(getDataTable(ctx).groups));

  // const tableColumnIds = observable<string>(["m", "a", "l", "k", "c", "b"]);
  const rowHeight = 20;
  const properties = observable<IProperty>(getProperties(ctx));
  const propertyById = computed(
    () => new Map(properties.map((property) => [property.id, property]))
  );

  const tableColumnIds = computed(
    () => getTableViewProperties(ctx).map(prop => prop.id)
  )

  const scrollTopObs = observable.box<number>(0);
  const scrollLeftObs = observable.box<number>(0);
  const viewportWidthObs = observable.box<number>(500);
  const viewportHeightObs = observable.box<number>(300);

  const fixedColumnCountObs = observable.box<number>(0);

  const clickSubscriptions: IClickSubsItem[] = [];

  const isCheckBoxedTable = getIsSelectionCheckboxesShown(ctx);

  function drawTable(ctx2d: CanvasRenderingContext2D) {
    renderTable(
      ctx,
      ctx2d,
      tableRowsCom.get(),
      groupedColumnIds.get(),
      tableColumnIds.get(),
      propertyById.get(),
      scrollLeftObs.get(),
      scrollTopObs.get(),
      viewportWidthObs.get(),
      viewportHeightObs.get(),
      isCheckBoxedTable,
      gridDimensions.gridLeadCellsDimensionsCom,
      gridDimensions.columnWidths,
      fixedColumnCountObs.get(),
      clickSubscriptions,
      rowHeight
    );
  }

  function setScroll(scrollLeft: number, scrollTop: number) {
    runInAction(() => {
      scrollTopObs.set(scrollTop);
      scrollLeftObs.set(scrollLeft);
    });
  }

  function handleClick(event: any) {
    const domRect = event.target.getBoundingClientRect();
    console.log(domRect);
    handleTableClick(
      event,
      event.clientX - domRect.x,
      event.clientY - domRect.y,
      scrollLeftObs.get(),
      scrollTopObs.get(),
      clickSubscriptions
    );
  }

  function setViewportSize(width: number, height: number) {
    runInAction(() => {
      viewportWidthObs.set(width);
      viewportHeightObs.set(height);
    });
  }

  return { drawTable, setScroll, handleClick, setViewportSize };
}

export const Table: React.FC<
  ITableProps & {
    refTable(elm: RawTable | null): void;
  }
> = (props) => {
  const ctxPanelVisibility = React.useContext(CtxPanelVisibility);
  return <RawTable {...props} isVisible={ctxPanelVisibility.isVisible} ref={props.refTable} />;
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
  @computed({ equals: comparer.structural }) get contentBounds() {
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
            this.tableRenderer.drawTable(this.ctxCanvas);
          }
        },
        {
          scheduler(fn) {
            requestAnimationFrame(fn);
          },
        }
      )
    );
    this.props.listenForScrollToCell &&
      this.disposers.push(
        this.props.listenForScrollToCell((rowIdx, colIdx) => {
          this.scrollToCellShortest(rowIdx, colIdx);
        })
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

  @action.bound scrollToCellShortest(rowIdx: number, columnIdx: number) {
    // TODO: Refactor to take real scrollbar sizes
    const freeColIndex = columnIdx + this.fixedColumnCount
    const { gridDimensions } = this.props;
    const SCROLLBAR_SIZE = 20;
    if (this.elmScroller) {
      const top = gridDimensions.getRowTop(rowIdx);
      const bottom = gridDimensions.getRowBottom(rowIdx);
      const left = gridDimensions.getColumnLeft(freeColIndex);
      const right = gridDimensions.getColumnRight(freeColIndex);

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
        this.elmScroller.scrollTo({ scrollTop: top });
      }
      if (bottom - this.elmScroller.scrollTop > this.contentBounds.height - SCROLLBAR_SIZE) {
        this.elmScroller.scrollTo({
          scrollTop: bottom - this.contentBounds.height + SCROLLBAR_SIZE,
        });
      }
    }
  }

  get fixedColumnCount() {
    return this.fixedHeaderContainers.length
  }

  get hasFixedColumns() {
    return this.fixedHeaderContainers.length !== 0;
  }

  @computed get fixedColumnsWidth() {
    return this.fixedHeaderContainers
      .map(container => container.width)
      .reduce((x,y) => x+y, 0)
  }

  @computed get freeHeaderContainers(){
    return this.props.headerContainers
      .filter(container => !container.isFixed)
  }

  @computed get freeHeaders(){
    return this.freeHeaderContainers
      .map(container => container.header)
  }

  @computed get fixedHeaderContainers(){
    return this.props.headerContainers
      .filter(container => container.isFixed)
  }

  @computed get fixedHeaders(){
    return this.fixedHeaderContainers
      .map(container => container.header)
  }

  @action.bound handleScrollerClick(event: any) {
    /*if (event.clientX > this.fixedColumnsWidth) {
      this.elmCanvasMoving &&
        this.elmCanvasMoving.triggerCellClick(
          event,
          event.clientX -
            this.contentBounds.left +
            this.props.scrollState.scrollLeft -
            this.fixedColumnsWidth,
          event.clientY - this.contentBounds.top + this.props.scrollState.scrollTop
        );
    } else {
      this.elmCanvasFixed &&
        this.elmCanvasFixed.triggerCellClick(
          event,
          event.clientX - this.contentBounds.left,
          event.clientY - this.contentBounds.top + this.props.scrollState.scrollTop
        );
    }*/
    this.tableRenderer.handleClick(event);
  }

  @action.bound handleResize(contentRect: { bounds: BoundingRect }) {
    this.contentBounds = contentRect.bounds;
    this.tableRenderer.setViewportSize(this.contentBounds.width, this.contentBounds.height);
  }

  @action.bound handleScroll(event: any, scrollLeft: number, scrollTop: number) {
    this.props.scrollState.setScrollOffset(event, scrollTop, scrollLeft);
    this.tableRenderer.setScroll(scrollLeft, scrollTop);
  }

  render() {
    return (
      <div className={S.table}>
        {this.props.isLoading && (
          <div className={S.loadingOverlay}>
            <div className={S.loadingIcon}>
              <i className="far fa-clock fa-7x blink" />
            </div>
          </div>
        )}
        <Measure ref={this.refMeasure} bounds={true} onResize={this.handleResize}>
          {({ measureRef, contentRect }) => (
            <Observer>
              {() => (
                <>
                  {this.props.headerContainers &&
                    (contentRect.bounds!.width ? (
                      <div className={S.headers}>
                        {this.hasFixedColumns ? (
                          <Scrollee
                            scrollOffsetSource={this.props.scrollState}
                            fixedHoriz={true}
                            fixedVert={true}
                            width={this.fixedColumnsWidth}
                          >
                            <HeaderRow
                              headerElements={this.fixedHeaders}
                              zIndex={100}
                            />
                          </Scrollee>
                        ) : (null)}
                        <Scrollee
                          scrollOffsetSource={this.props.scrollState}
                          fixedVert={true}
                          width={contentRect.bounds!.width - 10 - this.fixedColumnsWidth}
                        >
                          <HeaderRow
                            headerElements={this.freeHeaders}
                            zIndex={0}
                          />
                        </Scrollee>
                      </div>
                    ) : null)}
                  <div ref={measureRef} className={S.cellAreaContainer}>
                    {contentRect.bounds!.height && this.props.renderCell ? (
                      <>
                        <div className={S.canvasRow}>
                          <Canvas
                            refCanvasElement={this.refCanvasElement}
                            width={contentRect.bounds!.width - 20}
                            height={contentRect.bounds!.height - 20}
                          />
                          {/* {this.hasFixedColumns && (
                            <>
                              <Canvas
                                ref={this.refCanvasFixed}
                                columnStartIndex={0}
                                leftOffset={0}
                                isHorizontalScroll={false}
                                width={this.fixedColumnsWidth}
                                contentWidth={this.fixedColumnsWidth}
                                height={contentRect.bounds!.height - 20}
                                contentHeight={
                                  this.props.gridDimensions.contentHeight
                                }
                                scrollOffsetSource={this.props.scrollState}
                                gridDimensions={this.props.gridDimensions}
                                renderCell={this.props.renderCell}
                                onNoCellClick={this.props.onNoCellClick}
                              />
                              <Canvas
                                ref={this.refCanvasMoving}
                                columnStartIndex={this.fixedColumnCount}
                                leftOffset={-this.fixedColumnsWidth}
                                isHorizontalScroll={true}
                                width={
                                  contentRect.bounds!.width -
                                  20 -
                                  this.fixedColumnsWidth
                                }
                                contentWidth={
                                  this.props.gridDimensions.contentWidth
                                }
                                contentHeight={
                                  this.props.gridDimensions.contentHeight
                                }
                                height={contentRect.bounds!.height - 20}
                                scrollOffsetSource={this.props.scrollState}
                                gridDimensions={this.props.gridDimensions}
                                renderCell={this.props.renderCell}
                                onNoCellClick={this.props.onNoCellClick}
                              />
                            </>
                          )}
                          {!this.hasFixedColumns && (
                            <Canvas
                              ref={this.refCanvasMoving}
                              columnStartIndex={0}
                              leftOffset={0}
                              isHorizontalScroll={true}
                              width={contentRect.bounds!.width - 20}
                              contentWidth={
                                this.props.gridDimensions.contentWidth
                              }
                              contentHeight={
                                this.props.gridDimensions.contentHeight
                              }
                              height={contentRect.bounds!.height - 20}
                              scrollOffsetSource={this.props.scrollState}
                              gridDimensions={this.props.gridDimensions}
                              renderCell={this.props.renderCell}
                              onVisibleDataChanged={(
                                fvci,
                                lvci,
                                fvri,
                                lvri
                              ) => {
                                // console.log("VDC", fvci, lvci, fvri, lvri);
                              }}
                              onBeforeRender={undefined}
                              onAfterRender={undefined}
                              onNoCellClick={this.props.onNoCellClick}
                            />
                            )}*/}
                        </div>
                        {this.props.isEditorMounted &&
                          this.props.editingRowIndex !== undefined &&
                          this.props.editingColumnIndex !== undefined && (
                            <PositionedField
                              fixedColumnsCount={this.fixedColumnCount}
                              rowIndex={this.props.editingRowIndex}
                              columnIndex={this.props.editingColumnIndex}
                              scrollOffsetSource={this.props.scrollState}
                              gridDimensions={this.props.gridDimensions}
                              worldBounds={contentRect.bounds!}
                            >
                              {this.props.renderEditor && this.props.renderEditor()}
                            </PositionedField>
                          )}
                        <Scroller
                          ref={this.refScroller}
                          width={contentRect.bounds!.width}
                          height={contentRect.bounds!.height}
                          isVisible={true}
                          scrollingDisabled={this.props.isEditorMounted}
                          contentWidth={this.props.gridDimensions.contentWidth}
                          contentHeight={this.props.gridDimensions.contentHeight}
                          onScroll={this.handleScroll}
                          onClick={this.handleScrollerClick}
                          onKeyDown={this.props.onKeyDown}
                        />
                      </>
                    ) : null}
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
