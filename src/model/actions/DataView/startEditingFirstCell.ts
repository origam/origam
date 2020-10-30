import { getDataView } from "model/selectors/DataView/getDataView";
import { getProperties } from "model/selectors/DataView/getProperties";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";

export function startEditingFirstCell(ctx: any) {
  return function* stertEditingFirstCell() {
    const orderingPropertyId = getDataView(ctx)?.orderProperty?.id;
    const firstProperty = getProperties(ctx) //getTableViewProperties(ctx) // getTableViewProperties will return visible properties in the order shown in the table. This led to strange editor positioning and was therefore replaced. To be solved later.
          .filter(prop =>  prop.id !== "Id" && prop.id !== orderingPropertyId)?.[0];
    getTablePanelView(ctx).selectedColumnId = firstProperty?.id;
    if(getTablePanelView(ctx).selectedColumnId){
      getTablePanelView(ctx).setEditing(true);
    }
  };
}