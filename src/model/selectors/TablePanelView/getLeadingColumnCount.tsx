import { getGroupingConfiguration } from "./getGroupingConfiguration";
import { getIsSelectionCheckboxesShown } from "../DataView/getIsSelectionCheckboxesShown";

export function getLeadingColumnCount(ctx: any){
  const isCheckBoxedTable = getIsSelectionCheckboxesShown(ctx);
  const groupedColumnIds = getGroupingConfiguration(ctx).orderedGroupingColumnIds;
  return groupedColumnIds.length + (isCheckBoxedTable ? 1 : 0);
}