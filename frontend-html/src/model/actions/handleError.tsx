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

import selectors from "model/selectors-tree";
import { stopWorkQueues } from "./WorkQueues/stopWorkQueues";
import { performLogout } from "./User/performLogout";
import { T } from "utils/translation";
import { flow } from "mobx";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";
import { getOpenedScreens } from "model/selectors/getOpenedScreens";

const HANDLED = Symbol("_$ErrorHandled");

export function handleError(ctx: any) {
  return function*handleError(error: any) {
    const openedScreen = getOpenedScreen(ctx);
    if(openedScreen){
      const openedScreens = getOpenedScreens(openedScreen);
      if (!openedScreens.items.includes(openedScreen)) {
        // error on an already closed screen
        return;
      }
    }
    if (error.response && error.response.status === 474) {
      // 747 ~ ServerObjectDisposed happens when the user closes a form before all pending requests have
      // finished (RowStates for example)
      return;
    }
    if (error.code === "ERR_NETWORK" && error.name === "AxiosError"){
      yield*selectors.error.getDialogController(ctx).pushError(
        T(
          "Network Unavailable",
          "network_unavailable"
        )
      );
      return;
    }
    if (error.response &&
        error.response.status === 404 &&
        error.response.data.message.includes("row not found")) {
      yield*selectors.error.getDialogController(ctx).pushError(
        T(
          `The row you requested was not found on the server. Please refresh the data.`,
          "row_not_found"
        )
      );
      return;
    }
    if (error.response && error.response.status === 401) {
      yield*stopWorkQueues(ctx)();
      selectors.error.getDialogController(ctx).dismissErrors();
      yield*selectors.error.getDialogController(ctx).pushError(
        T(
          `Your request is no longer authorized, which means that 
          you were either logged out or your session expired. Please log in again.`,
          "request_no_longer_authorized"
        )
      );
      yield*performLogout(ctx)();
      return;
    }
    if (error[HANDLED]) {
      yield error[HANDLED];
      return;
    }
    const promise = flow(() => selectors.error.getDialogController(ctx).pushError(error))()
    error[HANDLED] = promise;
    yield promise;

  };
}
