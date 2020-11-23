import { IGrouper } from "./types/IGrouper";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { IProperty } from "./types/IProperty";


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

