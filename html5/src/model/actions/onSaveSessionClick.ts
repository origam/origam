import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export function onSaveSessionClick(ctx: any) {
  return function onSaveSessionClick() {
    getFormScreenLifecycle(ctx).onSaveSession();
  };
}
