import { IGrouper } from "./types/IGrouper";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { IProperty } from "./types/IProperty";
import { ICellOffset, IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";


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
    .filter(group => group.childGroups.length === 0)
    .map(group => group.getRowIndex(rowId))
    .find(index => index !== -1);
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

export function getPreviousRowId(grouper: IGrouper,rowId: string): string {
  const group = grouper.allGroups
    .find(group => group.getRowById(rowId))!;
  const indexInGroup = group.getRowIndex(rowId);
  if(indexInGroup !== undefined && indexInGroup !== 0){
    const previousRow =  group.childRows[indexInGroup - 1]
    return getDataTable(grouper).getRowId(previousRow);
  }else{
    const previousGroup = getPreviousNonEmptyGroup(grouper, group);
    if(previousGroup === undefined){
      return rowId;
    }
    const previousRow = previousGroup.childRows[previousGroup.childRows.length - 1];
    return getDataTable(grouper).getRowId(previousRow);
  }
}

export function getNextRowId(grouper: IGrouper,rowId: string): string {
  const group = grouper.allGroups
    .find(group => group.getRowById(rowId))!;
  const indexInGroup = group.getRowIndex(rowId);
  if(indexInGroup !== undefined && indexInGroup !== (group.rowCount - 1)){
    const nextRow =  group.childRows[indexInGroup + 1]
    return getDataTable(grouper).getRowId(nextRow);
  }else{
    const nextGroup = getNextNonEmptyGroup(grouper, group);
    if(nextGroup === undefined){
      return rowId;
    }
    const nextRow = nextGroup.childRows[0];
    return getDataTable(grouper).getRowId(nextRow);
  }
}

function getPreviousNonEmptyGroup(grouper: IGrouper, currentGroup: IGroupTreeNode){
  const sameLevelGroups = grouper.allGroups
    .filter(group => currentGroup.level === group.level && group.rowCount > 0 && group.isExpanded)
  const currentGroupIndex = sameLevelGroups.indexOf(currentGroup);
  return currentGroupIndex === 0
    ? undefined
    : sameLevelGroups[currentGroupIndex - 1];
}

function getNextNonEmptyGroup(grouper: IGrouper, currentGroup: IGroupTreeNode){
  const sameLevelGroups = grouper.allGroups
    .filter(group => currentGroup.level === group.level && group.rowCount > 0 && group.isExpanded)
  const currentGroupIndex = sameLevelGroups.indexOf(currentGroup);
  return currentGroupIndex === sameLevelGroups.length - 1
    ? undefined
    : sameLevelGroups[currentGroupIndex + 1];
}
