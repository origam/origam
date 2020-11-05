import S from "./ColumnsDialog.module.css";
import React from "react";
import {CloseButton, ModalWindow} from "../Dialog/Dialog";
import {AutoSizer, MultiGrid} from "react-virtualized";
import {bind} from "bind-decorator";
import {action, observable} from "mobx";
import {observer, Observer} from "mobx-react";
import produce from "immer";
import {Dropdowner} from "../Dropdowner/Dropdowner";
import {DataViewHeaderAction} from "../../../gui02/components/DataViewHeader/DataViewHeaderAction";
import {Dropdown} from "../../../gui02/components/Dropdown/Dropdown";
import {DropdownItem} from "../../../gui02/components/Dropdown/DropdownItem";
import {AggregationType, tryParseAggregationType} from "../../../model/entities/types/AggregationType";
import {T} from "../../../utils/translation";
import {rowHeight} from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";

export interface ITableColumnsConf {
  fixedColumnCount: number;
  columnConf: ITableColumnConf[];
}

export interface ITableColumnConf {
  id: string;
  name: string;
  isVisible: boolean;
  groupingIndex: number;
  aggregationType: AggregationType | undefined;
  entity: string;
  canGroup: boolean;
  canAggregate: boolean;
}

@observer
export class ColumnsDialog extends React.Component<{
  configuration: ITableColumnsConf;
  onOkClick?: (event: any, configuration: ITableColumnsConf) => void;
  onSaveAsClick?: (event: any) => void;
  onCancelClick?: (event: any) => void;
  onCloseClick?: (event: any) => void;
}> {
  constructor(props: any) {
    super(props);
    this.configuration = this.props.configuration;
  }

  @observable.ref configuration: ITableColumnsConf;

  @observable columnWidths = [70, 160, 70, 90];

  refGrid = React.createRef<MultiGrid>();

  @action.bound setVisible(rowIndex: number, state: boolean) {
    this.configuration = produce(this.configuration, (draft) => {
      draft.columnConf[rowIndex].isVisible = state;
    });
  }

  @action.bound setGrouping(rowIndex: number, state: boolean) {
    this.configuration = produce(this.configuration, (draft) => {
      const columnConfCopy = [...draft.columnConf];
      columnConfCopy.sort((a, b) => b.groupingIndex - a.groupingIndex);
      if (draft.columnConf[rowIndex].groupingIndex === 0) {
        draft.columnConf[rowIndex].groupingIndex = columnConfCopy[0].groupingIndex + 1;
      } else {
        draft.columnConf[rowIndex].groupingIndex = 0;
        let groupingIndex = 1;
        columnConfCopy.reverse();
        for (let columnConfItem of columnConfCopy) {
          if (columnConfItem.groupingIndex > 0) {
            columnConfItem.groupingIndex = groupingIndex++;
          }
        }
      }
    });
  }

  @action.bound setAggregation(rowIndex: number, selectedAggregation: any) {
    this.configuration = produce(this.configuration, (draft) => {
      draft.columnConf[rowIndex].aggregationType = tryParseAggregationType(selectedAggregation);
    });
  }

  @action.bound handleFixedColumnsCountChange(event: any) {
    this.configuration = produce(this.configuration, (draft) => {
      draft.fixedColumnCount = parseInt(event.target.value, 10);
    });
    console.log(this.configuration);
  }

  render() {
    return (
      <ModalWindow
        title={T("Columns","column_config_title")}
        titleButtons={<CloseButton onClick={this.props.onCloseClick}/>}
        buttonsCenter={
          <>
            <button
              onClick={(event: any) =>
                this.props.onOkClick && this.props.onOkClick(event, this.configuration)
              }
            >
              {T("OK","button_ok")}
            </button>
            {/*<button onClick={this.props.onSaveAsClick}>*/}
            {/*  {T("Save As...","column_config_save_as")}*/}
            {/*</button>*/}
            <button onClick={this.props.onCancelClick}>
              {T("Cancel","button_cancel")}
            </button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={S.columnTable}>
          <AutoSizer>
            {({width, height}) => (
              <Observer>
                {() => (
                  <MultiGrid
                    ref={this.refGrid}
                    fixedRowCount={1}
                    cellRenderer={this.renderCell}
                    columnCount={4}
                    rowCount={1 + this.configuration.columnConf.length}
                    columnWidth={({index}: { index: number }) => {
                      return this.columnWidths[index];
                    }}
                    rowHeight={rowHeight}
                    width={width}
                    height={height}
                  />
                )}
              </Observer>
            )}
          </AutoSizer>
        </div>
        <div className={S.lockedColumns}>
          {T("Locked columns count","column_config_locked_columns_count")}
          <input
            className={S.lockedColumnsInput}
            type="number"
            min={0}
            value={"" + this.configuration.fixedColumnCount}
            onChange={this.handleFixedColumnsCountChange}
          />
        </div>
      </ModalWindow>
    );
  }

  getCell(rowIndex: number, columnIndex: number) {
    const {isVisible, name, aggregationType, groupingIndex, entity, canGroup, canAggregate} = this.configuration.columnConf[rowIndex];
    switch (columnIndex) {
      case 0:
        return (
          <input
            type="checkbox"
            key={`${rowIndex}@${columnIndex}`}
            onChange={(event: any) => this.setVisible(rowIndex, event.target.checked)}
            checked={isVisible}
          />
        );
      case 1:
        return name;
      case 2:
        return (
          <span>
            <input
              type="checkbox"
              key={`${rowIndex}@${columnIndex}`}
              checked={groupingIndex > 0}
              onClick={(event: any) => this.setGrouping(rowIndex, event.target.checked)}
              disabled={!canGroup}
            />{" "}
            {groupingIndex > 0 ? groupingIndex : ""}
          </span>
        );
      case 3:
        if ((entity === "Currency" || entity === "Integer" || entity === "Float" || entity === "Long") && canAggregate) {
          return (
              <Dropdowner
                trigger={({refTrigger, setDropped}) => (
                  <DataViewHeaderAction
                    refDom={refTrigger}
                    onMouseDown={() => setDropped(true)}
                    isActive={false}
                  >
                    {aggregationType}
                  </DataViewHeaderAction>
                )}
                content={({setDropped}) => (
                  <Dropdown>
                    <DropdownItem
                      onClick={(event: any) => {
                        setDropped(false);
                        this.setAggregation(rowIndex, undefined);
                      }}>
                    </DropdownItem>
                    <DropdownItem
                      onClick={(event: any) => {
                        setDropped(false);
                        this.setAggregation(rowIndex, AggregationType.SUM)
                      }}>
                      {T("SUM","aggregation_sum")}
                    </DropdownItem>
                    <DropdownItem
                      onClick={(event: any) => {
                        setDropped(false);
                        this.setAggregation(rowIndex, AggregationType.AVG)
                      }}>
                      {T("AVG","aggregation_avg")}
                    </DropdownItem>
                    <DropdownItem
                      onClick={(event: any) => {
                        setDropped(false);
                        this.setAggregation(rowIndex, AggregationType.MIN)
                      }}>
                      {T("MIN","aggregation_min")}
                    </DropdownItem>
                    <DropdownItem
                      onClick={(event: any) => {
                        setDropped(false);
                        this.setAggregation(rowIndex, AggregationType.MAX)
                      }}>
                      {T("MAX","aggregation_max")}
                    </DropdownItem>
                  </Dropdown>
                )}
              />
          );
        } else {
          return "";
        }
      default:
        return "";
    }
  }

  @bind renderCell(args: {
    columnIndex: number;
    rowIndex: number;
    style: any;
    key: any;
  }): React.ReactNode {
    if (args.rowIndex > 0) {
      const rowClassName = args.rowIndex % 2 === 0 ? "even" : "odd";
      return (
        <Observer>
          {() => (
            <div style={args.style} className={S.columnTableCell + " " + rowClassName}>
              {this.getCell(args.rowIndex - 1, args.columnIndex)}
            </div>
          )}
        </Observer>
      );
    } else {
      return (
        <Observer>
          {() => (
            <TableHeader
              columnIndex={args.columnIndex}
              style={args.style}
              columnWidth={this.columnWidths[args.columnIndex]}
              onColumnWidthChange={this.handleColumnWidthChange}
            />
          )}
        </Observer>
      );
    }
  }

  @action.bound handleColumnWidthChange(columnIndex: number, newWidth: number) {
    if (newWidth >= 30) {
      this.columnWidths[columnIndex] = newWidth;
      this.refGrid.current!.recomputeGridSize();
    }
  }
}

@observer
export class TableHeader extends React.Component<{
  columnIndex: number;
  columnWidth: number;
  style: any;
  onColumnResizeStart?: (columnIndex: number) => void;
  onColumnWidthChange?: (columnIndex: number, newWidth: number) => void;
  onColumnResizeEnd?: (columnIndex: number) => void;
}> {
  getHeader(columnIndex: number) {
    switch (columnIndex) {
      case 0:
        return T("Visible","column_config_visible");
      case 1:
        return T("Name","column_config_name");
      case 2:
        return T("GroupBy","column_config_group_by");
      case 3:
        return T("Aggregation","column_config_aggregation");
      default:
        return "?";
    }
  }

  width0 = 0;
  mouseX0 = 0;

  @action.bound handleColumnWidthHandleMouseDown(event: any) {
    event.preventDefault();
    this.width0 = this.props.columnWidth;
    this.mouseX0 = event.screenX;
    window.addEventListener("mousemove", this.handleWindowMouseMove);
    window.addEventListener("mouseup", this.handleWindowMouseUp);
  }

  @action.bound handleWindowMouseMove(event: any) {
    const vec = event.screenX - this.mouseX0;
    const newWidth = this.width0 + vec;
    console.log(this.props.columnIndex, newWidth);
    this.props.onColumnWidthChange &&
    this.props.onColumnWidthChange(this.props.columnIndex, newWidth);
  }

  @action.bound handleWindowMouseUp(event: any) {
    window.removeEventListener("mousemove", this.handleWindowMouseMove);
    window.removeEventListener("mouseup", this.handleWindowMouseUp);
  }

  render() {
    return (
      <div style={this.props.style} className={S.columnTableCell + " header"}>
        {this.getHeader(this.props.columnIndex)}
        <div className={S.columnWidthHandle} onMouseDown={this.handleColumnWidthHandleMouseDown}/>
      </div>
    );
  }
}
