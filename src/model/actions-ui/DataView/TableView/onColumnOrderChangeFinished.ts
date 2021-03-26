import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import {getConfigurationManager} from "model/selectors/TablePanelView/getConfigurationManager";
import {runGeneratorInFlowWithHandler} from "utils/runInFlowWithHandler";

export function onColumnOrderChangeFinished(ctx: any, id1: string, id2: string) {
  runGeneratorInFlowWithHandler({
    ctx: ctx,
    generator: function* (){
      const tablePanelView = getTablePanelView(ctx);
      tablePanelView.moveColumnBehind(id1, id2);
      yield* getConfigurationManager(ctx).onColumnOrderChnaged();
    }()
  })
}
