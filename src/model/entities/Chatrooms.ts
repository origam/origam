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

import { computed, observable } from "mobx";
import { onRefreshChatrooms } from "model/actions/Chatrooms/onRefreshChatrooms";
import { getApi } from "model/selectors/getApi";
import { PeriodicLoader } from "utils/PeriodicLoader";

export class Chatrooms {
  *getChatroomsList(): any {
    const api = getApi(this);
    const chatrooms = yield api.getChatroomList();
    this.items = chatrooms;
  }

  loader = new PeriodicLoader(onRefreshChatrooms(this), () => getApi(this).onApiResponse);

  @observable items: any[] = [];
  @computed get totalItemCount() {
    return this.items.map((item) => item.unreadMessageCount).reduce((a, b) => a + b, 0);
  }

  get sortedItems() {
    return this.items;
  }

  *startTimer(refreshIntervalMs: number) {
    if (localStorage.getItem("debugNoPolling_chatrooms")) return;
    if (localStorage.getItem("debugPollingMs_chatrooms")) {
      refreshIntervalMs = parseInt(localStorage.getItem("debugPollingMs_chatrooms") || "30000");
    }
    yield* this.loader.start(refreshIntervalMs);
  }

  parent?: any;
}
