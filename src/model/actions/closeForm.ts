import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";
import {getOpenedScreen} from "model/selectors/getOpenedScreen";
import {IRefreshOnReturnType} from "../entities/WorkbenchLifecycle/WorkbenchLifecycle";
import {getApi} from "../selectors/getApi";

export function closeForm(ctx: any) {
  return function* closeForm(): Generator {
    const lifecycle = getWorkbenchLifecycle(ctx);
    const openedScreen = getOpenedScreen(ctx);

    const refreshOnReturnType = openedScreen.content.refreshOnReturnType;
    switch(refreshOnReturnType){
      case IRefreshOnReturnType.ReloadActualRecord:
        break;
      case IRefreshOnReturnType.RefreshCompleteForm:
        break;
      case IRefreshOnReturnType.MergeModalDialogChanges:
        const api = getApi(ctx);
        const data = yield api.pendingChanges({ sessionFormIdentifier: openedScreen.parent.activeItem.content!.formScreen!.sessionId});
        // const data = yield api.pendingChanges({ sessionFormIdentifier: openedScreen.content!.formScreen!.sessionId});
        break;
    }
    yield* lifecycle.closeForm(openedScreen);

  };
}
