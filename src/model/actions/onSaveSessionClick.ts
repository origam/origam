import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
// TODO: Move to ui actions
export function onSaveSessionClick(ctx: any) {
  return function onSaveSessionClick() {
    getFormScreenLifecycle(ctx).onSaveSession();
  };
}
