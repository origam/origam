import { IUrlUpenMethod } from "model/entities/types/IUrlOpenMethod";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { getApi } from "model/selectors/getApi";

export function openNewUrl(ctx: any) {
  return function* openNewUrl(
    url: string,
    urlOpenMethod: IUrlUpenMethod,
    title: string
  ) {
    switch (urlOpenMethod) {
      case IUrlUpenMethod.OrigamTab:
        yield* getWorkbenchLifecycle(ctx).openNewUrl(url, title);
        break;
      default:
        // TODO: Transform url to be absolute to urlroot?
        const win = window.open(`${url}`, "_blank");
        win && win.focus();
        break;
    }
  };
}
