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

import S from "gui/Components/Dialogs/ColumnsDialog.module.scss";
import React from "react";
import { CloseButton, ModalWindow } from "../Dialog/Dialog";
import { AutoSizer, MultiGrid } from "react-virtualized";
import { bind } from "bind-decorator";
import { action, observable } from "mobx";
import { observer, Observer } from "mobx-react";
import { AggregationType, tryParseAggregationType, } from "model/entities/types/AggregationType";
import { T } from "utils/translation";
import { rowHeight } from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";
import { GroupingUnit, groupingUnitToLabel } from "model/entities/types/GroupingUnit";
import { IColumnConfiguration, ITableConfiguration } from "model/entities/TablePanelView/types/IConfigurationManager";
import { IColumnOptions } from "model/entities/TablePanelView/ColumnConfigurationDialog";
import { IOption, SimpleDropdown } from "@origam/components";
import { compareStrings } from "utils/string";

@observer
export class ColumnsDialog extends React.Component<{
  columnOptions: Map<string, IColumnOptions>;
  configuration: ITableConfiguration;
  onOkClick?: (configuration: ITableConfiguration) => void;
  onSaveAsClick: (event: any, configuration: ITableConfiguration) => void;
  onCancelClick?: (event: any) => void;
  onCloseClick?: (event: any) => void;
}> {
  constructor(props: any) {
    super(props);
    this.configuration = this.props.configuration;
    this.sortedColumnConfigs = [...this.configuration.columnConfigurations].sort(
      (a, b) => {
        const optionA = this.props.columnOptions.get(a.propertyId)!;
        const optionB = this.props.columnOptions.get(b.propertyId)!;
        return compareStrings(optionA.name, optionB.name)
      }
    );
  }

  configuration: ITableConfiguration;
  sortedColumnConfigs: IColumnConfiguration[];

  @observable columnWidths = [70, 220, 110, 150, 90];

  refGrid = React.createRef<MultiGrid>();

  @action.bound setVisible(rowIndex: number, state: boolean) {
    this.sortedColumnConfigs[rowIndex].isVisible = state;
  }

  @action.bound setGrouping(rowIndex: number, state: boolean, entity: string) {
    if (entity === "Date") {
      if (state) {
        this.setTimeGroupingUnit(rowIndex, GroupingUnit.Day);
      } else {
        this.setTimeGroupingUnit(rowIndex, undefined);
      }
    }

    const columnConfCopy = [...this.sortedColumnConfigs];
    columnConfCopy.sort((a, b) => b.groupingIndex - a.groupingIndex);
    if (this.sortedColumnConfigs[rowIndex].groupingIndex === 0) {
      this.sortedColumnConfigs[rowIndex].groupingIndex =
        columnConfCopy[0].groupingIndex + 1;
    } else {
      this.sortedColumnConfigs[rowIndex].groupingIndex = 0;
      let groupingIndex = 1;
      columnConfCopy.reverse();
      for (let columnConfItem of columnConfCopy) {
        if (columnConfItem.groupingIndex > 0) {
          columnConfItem.groupingIndex = groupingIndex++;
        }
      }
    }
  }

  @action.bound setTimeGroupingUnit(rowIndex: number, groupingUnit: GroupingUnit | undefined) {
    this.sortedColumnConfigs[rowIndex].timeGroupingUnit = groupingUnit;
  }

  @action.bound setAggregation(rowIndex: number, selectedAggregation: any) {
    this.sortedColumnConfigs[rowIndex].aggregationType = tryParseAggregationType(
      selectedAggregation
    );
  }

  @action.bound handleFixedColumnsCountChange(event: any) {
    this.configuration.fixedColumnCount = parseInt(event.target.value, 10);
  }

  render() {
    return (
      <ModalWindow
        title={T("Columns", "column_config_title")}
        titleButtons={<CloseButton onClick={this.props.onCloseClick}/>}
        buttonsCenter={
          <>
            <button
              id={"columnConfigOk"}
              tabIndex={0}
              onClick={(event: any) =>
                this.props.onOkClick && this.props.onOkClick(this.configuration)
              }
            >
              {T("OK", "button_ok")}
            </button>
            <button onClick={(event) => this.props.onSaveAsClick(event, this.configuration)}>
              {T("Save As...", "column_config_save_as")}
            </button>
            <button tabIndex={0} onClick={this.props.onCancelClick}>
              {T("Cancel", "button_cancel")}
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
                    columnCount={5}
                    rowCount={1 + this.sortedColumnConfigs.length}
                    columnWidth={({ index }: { index: number }) => {
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
          {T("Locked columns count", "column_config_locked_columns_count")}
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
    const {
      isVisible,
      propertyId,
      aggregationType,
      groupingIndex,
      timeGroupingUnit,
    } = this.sortedColumnConfigs[rowIndex];

    const { name, entity, canGroup, canAggregate, modelInstanceId } = this.props.columnOptions.get(propertyId)!;

    const aggregationOptions =  [
      new AggregationOption("", undefined),
      new AggregationOption(T("SUM", "aggregation_sum"), AggregationType.SUM),
      new AggregationOption(T("AVG", "aggregation_avg"), AggregationType.AVG),
      new AggregationOption(T("MIN", "aggregation_min"), AggregationType.MIN),
      new AggregationOption(T("MAX", "aggregation_max"), AggregationType.MAX),
    ];
    const selectedAggregationOption = aggregationOptions.find(option => option.value === aggregationType)!;

    const timeunitOptions =  [
      new TimeUnitOption(groupingUnitToLabel(GroupingUnit.Year), GroupingUnit.Year),
      new TimeUnitOption(groupingUnitToLabel(GroupingUnit.Month), GroupingUnit.Month),
      new TimeUnitOption(groupingUnitToLabel(GroupingUnit.Day), GroupingUnit.Day),
      new TimeUnitOption(groupingUnitToLabel(GroupingUnit.Hour), GroupingUnit.Hour),
      new TimeUnitOption(groupingUnitToLabel(GroupingUnit.Minute), GroupingUnit.Minute),
    ];
    const selectedTimeUnitOption = timeunitOptions.find(option => option.value === timeGroupingUnit)!;

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
          <label id={"group_index_" + modelInstanceId} className={S.checkBox}>
            <input
              id={"group_by_" + modelInstanceId}
              type="checkbox"
              key={`${rowIndex}@${columnIndex}`}
              checked={groupingIndex > 0}
              onChange={(event: any) => this.setGrouping(rowIndex, event.target.checked, entity)}
              disabled={!canGroup}
            />
            <div>
              {groupingIndex > 0 ? groupingIndex : ""}
            </div>
          </label>
        );
      case 3:
        if (groupingIndex > 0 && entity === "Date") {
          return (
              <SimpleDropdown
                width={"72.5px"}
                options={timeunitOptions}
                selectedOption={selectedTimeUnitOption}
                onOptionClick={option =>  this.setTimeGroupingUnit(rowIndex, option.value)}
              />
          );
        } else {
          return "";
        }
      case 4:
        if (
          (entity === "Currency" ||
            entity === "Integer" ||
            entity === "Float" ||
            entity === "Long") &&
          canAggregate
        ) {
          return (
            <SimpleDropdown
              width={"72.5px"}
              options={aggregationOptions}
              selectedOption={selectedAggregationOption}
              onOptionClick={option => this.setAggregation(rowIndex, option.value)}
            />
          );
        } else {
          return "";
        }
      default:
        return "";
    }
  }

  getCellClass(columnIndex: number){
    let cellClass = S.columnTableCell
    if(columnIndex === 0 || columnIndex === 2){
      cellClass += " " + S.checkBoxCell
    }
    return cellClass;
  }

  @bind renderCell(args: {
    columnIndex: number;
    rowIndex: number;
    style: any;
    key: any;
  }): React.ReactNode {
    const Obsv = Observer as any;
    if (args.rowIndex > 0) {
      const rowClassName = args.rowIndex % 2 === 0 ? "even" : "odd";
      return (
        <Obsv style={args.style} key={args.key}>
          {() => (
            <div style={args.style} className={this.getCellClass(args.columnIndex) + " " + rowClassName}>
              {this.getCell(args.rowIndex - 1, args.columnIndex)}
            </div>
          )}
        </Obsv>
      );
    } else {
      return (
        <Obsv style={args.style} key={args.key}>
          {() => (
            <TableHeader
              columnIndex={args.columnIndex}
              style={args.style}
              columnWidth={this.columnWidths[args.columnIndex]}
            />
          )}
        </Obsv>
      );
    }
  }
}

@observer
export class TableHeader extends React.Component<{
  columnIndex: number;
  columnWidth: number;
  style: any;
}> {
  getHeader(columnIndex: number) {
    switch (columnIndex) {
      case 0:
        return T("Visible", "column_config_visible");
      case 1:
        return T("Name", "column_config_name");
      case 2:
        return T("GroupBy", "column_config_group_by");
      case 3:
        return T("Grouping Unit", "column_config_time_grouping_unit");
      case 4:
        return T("Aggregation", "column_config_aggregation");
      default:
        return "?";
    }
  }

  render() {
    return (
      <div style={this.props.style} className={S.columnTableCell + " header"}>
        {this.getHeader(this.props.columnIndex)}
        {this.props.columnIndex !== 4 && <div className={S.columnWidthHandle}/>}
      </div>
    );
  }
}

class AggregationOption implements IOption<AggregationType | undefined>{
  constructor (
   public label: string,
   public value: AggregationType | undefined,
  ){
  }
}

class TimeUnitOption implements IOption<GroupingUnit>{
  constructor (
   public label: string,
   public value: GroupingUnit,
  ){
  }
}

