/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { flow } from "mobx";
import { handleError } from "../handleError";
import { executeWeblink } from "./executeWeblink";

export function onRootElementClick(ctx: any) {
  return flow(function*onRootElementClick(event: any): Generator {
    try {
      const {target} = event;
      const {nodeName} = target;

      if (`${nodeName}`.toLowerCase() === "a") {
        const href = target.getAttribute("href");
        if (href && `${href}`.startsWith("web+origam-link://")) {
          const actionUrl = href.replace("web+origam-link://", "");
          event.preventDefault();
          const urlParts = actionUrl.split("?");
          const urlPath = urlParts[0];
          const urlParams = new URLSearchParams(urlParts[1] || "");
          const urlQuery: {[key: string]: any} = {};
          for (let key of urlParams.keys()){
            urlQuery[key] = urlParams.get(key);
          }
          yield*executeWeblink(ctx)(urlPath, urlQuery);
        }
      }
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  });
}
