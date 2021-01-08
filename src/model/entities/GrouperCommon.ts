import { IGrouper } from "./types/IGrouper";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { IProperty } from "./types/IProperty";
import { ICellOffset } from "gui/Components/ScreenElements/Table/TableRendering/types";


export function getAllLoadedValuesOfProp(property: IProperty, grouper: IGrouper): Set<any> {
  const dataTable = getDataTable(grouper);
  return new Set(
    grouper.allGroups
      .filter(group => group.isExpanded)
      .flatMap(group => group.childRows)
      .map((row) => dataTable.getCellValue(row, property))
      .filter((row) => row)
  );
}


export function getRowById(grouper: IGrouper, id: string): any[] | undefined {
  return grouper.allGroups
      .map(group => group.getRowById(id))
      .find(row => row)
}

export function getRowIndex(grouper: IGrouper, rowId: string): number | undefined {
  return grouper.allGroups
  .map(group => group.getRowIndex(rowId))
  .find(index => index !== -1);
}

export function getMaxRowCountSeen(grouper: IGrouper, rowId: string): number {
  return grouper.allGroups
  .find(group => group.getRowById(rowId))?.childRows?.length ?? 0;
}

export function getRowCount(grouper: IGrouper, rowId: string){
  return grouper.allGroups
    .find(group => group.getRowById(rowId))
    ?.rowCount
}

export function getCellOffset(grouper: IGrouper, rowId: string): ICellOffset {
  const containingGroup =  grouper.allGroups
  .filter(group => group.getRowById(rowId) && group.isExpanded)
  .sort((g1, g2) => g2.level - g1.level)[0]
  
  let rowOffset = 0;
  for (const group of grouper.allGroups) {
    rowOffset++;
    if(group === containingGroup){
      return {
        row: rowOffset,
        column: group.level + 1}
    }
    if(group.isExpanded && !group.childGroups.some(child => child.isExpanded)){
      rowOffset += group.childRows.length;
    }
  }
  return {
    row: 0,
    column:0
  }
}