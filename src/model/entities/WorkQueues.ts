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

import { IWorkQueues } from "./types/IWorkQueues";
import { getApi } from "model/selectors/getApi";
import { onRefreshWorkQueues } from "model/actions-ui/WorkQueues/onRefreshWorkQueues";
import { computed, observable } from "mobx";
import { PeriodicLoader } from "utils/PeriodicLoader";

export class WorkQueues implements IWorkQueues {
  $type_IWorkQueues: 1 = 1;

  *getWorkQueueList() {
    const api = getApi(this);
    const workQueues = yield api.getWorkQueueList();
    this.items = workQueues;
  }

  @observable items: any[] = [];
  @computed get totalItemCount() {
    return this.items.map((item) => item.countTotal).reduce((a, b) => a + b, 0);
  }

  loader = new PeriodicLoader(onRefreshWorkQueues(this), () => getApi(this).onApiResponse);

  *startTimer(refreshIntervalMs: number) {
    if (localStorage.getItem("debugNoPolling")) return;
    yield* this.loader.start(refreshIntervalMs);
  }

  *stopTimer() {
    yield* this.loader.stop();
  }

  hRefreshTimer: any;
  refreshInterval = 0;

  get isTimerRunning() {
    return !!this.hRefreshTimer;
  }

  parent?: any;
}
