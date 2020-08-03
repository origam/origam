import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";
import {getOpenedScreen} from "model/selectors/getOpenedScreen";
import {IRefreshOnReturnType} from "../entities/WorkbenchLifecycle/WorkbenchLifecycle";
import {getApi} from "../selectors/getApi";
import {ICRUDResult, processCRUDResult} from "./DataLoading/processCRUDResult";

export function closeForm(ctx: any) {
  return function* closeForm(): Generator {
    const lifecycle = getWorkbenchLifecycle(ctx);
    const openedScreen = getOpenedScreen(ctx);

    yield* lifecycle.closeForm(openedScreen);
    if(openedScreen.content){
      const refreshOnReturnType = openedScreen.content.refreshOnReturnType;
      switch(refreshOnReturnType){
        case IRefreshOnReturnType.ReloadActualRecord:
          break;
        case IRefreshOnReturnType.RefreshCompleteForm:
          break;
        case IRefreshOnReturnType.MergeModalDialogChanges:
          const api = getApi(ctx);
          const parentScreen = getOpenedScreen(openedScreen.parentContext);
          const parentScreenSessionId = parentScreen!.content!.formScreen!.sessionId;
          const changes = (yield api.pendingChanges({ sessionFormIdentifier: parentScreenSessionId})) as ICRUDResult[];
          for (let change of changes) {
            yield* processCRUDResult(openedScreen.parentContext, change);
          }
          break;
      }
    }
  };
}
