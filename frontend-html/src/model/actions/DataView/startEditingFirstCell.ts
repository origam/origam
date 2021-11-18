import { getDataView } from "model/selectors/DataView/getDataView";
import { getProperties } from "model/selectors/DataView/getProperties";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";

export function startEditingFirstCell(ctx: any) {
  return function* startEditingFirstCell() {
    const orderingPropertyId = getDataView(ctx)?.orderProperty?.id;
    const firstProperty = getTableViewProperties(ctx) 
          .filter(prop =>  prop.id !== "Id" && prop.id !== orderingPropertyId)?.[0];
    getTablePanelView(ctx).selectedColumnId = firstProperty?.id;
    if(getTablePanelView(ctx).selectedColumnId){
      getTablePanelView(ctx).scrollToCurrentRow();
      setTimeout(()=> getTablePanelView(ctx).setEditing(true));
    }
  };
}