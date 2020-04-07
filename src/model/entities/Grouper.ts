import { IRowGroup } from "./types/IRowGroup";
import { getGroupingConfiguration } from "../selectors/TablePanelView/getGroupingConfiguration";
import { IGroupData } from "./types/IGroupData";
export class Grouper {
  group(groupData: any[]): IRowGroup[] {
    const groupingConfiguration = getGroupingConfiguration(this);
    const columnName = groupingConfiguration.orderedGroupingColumnIds[0];
    return groupData
      .map(groupDataItem => {
        return {
          isExpanded: false,
          level: 0,
          groupColumnName: columnName,
          groupValue: groupDataItem["groupCaption"]
            ? groupDataItem["groupCaption"]
            : groupDataItem[columnName],
          rowCount: groupDataItem["groupCount"],
          groupChildren: [],
          rowChildren: []
        };
      });
  }
}
