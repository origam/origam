import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";
import {getOpenedScreen} from "model/selectors/getOpenedScreen";
import {IRefreshOnReturnType} from "../entities/WorkbenchLifecycle/WorkbenchLifecycle";
import {getApi} from "../selectors/getApi";
import {ICRUDResult, processCRUDResult} from "./DataLoading/processCRUDResult";
import {getDontRequestData} from "model/selectors/getDontRequestData";
import {getDataViewLifecycle} from "model/selectors/DataView/getDataViewLifecycle";
import {getFormScreen} from "model/selectors/FormScreen/getFormScreen";
import {onRefreshSessionClick} from "model/actions-ui/ScreenToolbar/onRefreshSessionClick";

export function closeForm(ctx: any) {
  return function* closeForm(): Generator {
    const lifecycle = getWorkbenchLifecycle(ctx);
    const openedScreen = getOpenedScreen(ctx);

    yield* lifecycle.closeForm(openedScreen);
    if(openedScreen.content?.refreshOnReturnType){
      const refreshOnReturnType = openedScreen.content.refreshOnReturnType;
      const parentScreen = getOpenedScreen(openedScreen.parentContext);
      const parentFormScreen = getFormScreen(openedScreen.parentContext);
      switch(refreshOnReturnType){
        case IRefreshOnReturnType.ReloadActualRecord:
          if (getDontRequestData(ctx)) {
            break;
          }
          for (let dataView of parentFormScreen.dataViews) {
            const dataViewLifecycle = getDataViewLifecycle(dataView);
            yield dataViewLifecycle.runRecordChangedReaction();
          }
          break;
        case IRefreshOnReturnType.RefreshCompleteForm:
          onRefreshSessionClick(parentFormScreen)
          break;
        case IRefreshOnReturnType.MergeModalDialogChanges:
          const api = getApi(ctx);
          const parentScreenSessionId = parentScreen!.content!.formScreen!.sessionId;
          const changes = (yield api.pendingChanges({ sessionFormIdentifier: parentScreenSessionId})) as ICRUDResult[];
          for (let change of changes) {
            yield* processCRUDResult(openedScreen.parentContext, change);
          }
          const dataViews = parentScreen?.content?.formScreen?.dataViews ?? [];
          for (const dataView of dataViews) {
            dataView.dataTable.unlockAddedRowPosition();
            dataView.dataTable.updateSortAndFilter();
          }
          
          break;
      }
    }
  };
}
