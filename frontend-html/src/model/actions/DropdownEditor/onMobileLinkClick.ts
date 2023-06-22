import { IProperty } from "model/entities/types/IProperty";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";
import { getDataView } from "model/selectors/DataView/getDataView";

export function onMobileLinkClick(property: IProperty | undefined, currentRow: any[] | undefined) {
  if(!currentRow || !property?.isLink){
    return
  }

  const value = getDataView(property).dataTable.getCellValue(currentRow, property);
  if(!value){
    return;
  }

  runGeneratorInFlowWithHandler({
    ctx: property,
    generator: function*() {
      yield*getDataView(property).navigateLookupLink(property, currentRow);
    }()
  });
}