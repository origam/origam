import { flow } from "mobx";
import qs from "querystring";
import { handleError } from "../handleError";
import { executeWeblink } from "./executeWeblink";

export function onRootElementClick(ctx: any) {
  return flow(function* onRootElementClick(event: any): Generator {
    try {
      const { target } = event;
      const { nodeName } = target;

      if (`${nodeName}`.toLowerCase() === "a") {
        const href = target.getAttribute("href");
        if (href && `${href}`.startsWith("web+origam-link://")) {
          const actionUrl = href.replace("web+origam-link://", "");
          event.preventDefault();
          const urlParts = actionUrl.split("?");
          const urlPath = urlParts[0];
          const urlQuery = qs.parse(urlParts[1] || "");
          yield* executeWeblink(ctx)(urlPath, urlQuery);
        }
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
