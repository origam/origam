import {flow} from "mobx";
import {getDataViewPropertyById} from "model/selectors/DataView/getDataViewPropertyById";
import {runGeneratorInFlowWithHandler} from "utils/runInFlowWithHandler";

export function onColumnWidthChanged(ctx: any, id: string, width: number) {
  runGeneratorInFlowWithHandler({
    ctx: ctx,
    generator: function*(){
      const prop = getDataViewPropertyById(ctx, id);
      if(prop) {
        prop.setColumnWidth(width);
      }
    }()})
}