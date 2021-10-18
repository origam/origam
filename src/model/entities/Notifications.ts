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

import { PeriodicLoader } from "utils/PeriodicLoader";
import { observable } from "mobx";
import { getApi } from "model/selectors/getApi";
import { getNotificationBoxContent } from "model/actions/Notifications/GetNotificationBoxContent";

export class Notifications {
  @observable
  notificationBox: any;

  *getNotificationBoxContent(): any {
    this.notificationBox = yield getApi(this).getNotificationBoxContent();
  }

  loader = new PeriodicLoader(getNotificationBoxContent(this), () => getApi(this).onApiResponse);

  *startTimer(refreshIntervalMs: number) {
    if (localStorage.getItem("debugNoPolling_notificationBox")) return;
    yield*this.loader.start(refreshIntervalMs);
  }

  parent?: any;
}
