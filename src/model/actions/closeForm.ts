import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";
import {getOpenedScreen} from "model/selectors/getOpenedScreen";
import {IRefreshOnReturnType} from "../entities/WorkbenchLifecycle/WorkbenchLifecycle";
import {getApi} from "../selectors/getApi";
import {getFormScreen} from "../selectors/FormScreen/getFormScreen";

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
          const changeList = (yield api.pendingChanges({ sessionFormIdentifier: openedScreen.parentSessionId!})) as any[];
          for (let change of changeList) {
            const dataViews = getFormScreen(ctx).getDataViewsByEntity(change.entity);
            for (let dataView of dataViews) {
              dataView.dataTable.substituteRecord(change.wrappedObject);
            }
          }
          break;
      }
    }
  };
}
