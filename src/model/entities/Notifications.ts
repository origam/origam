import { PeriodicLoader } from "utils/PeriodicLoader";
import { observable } from "mobx";
import { getApi } from "model/selectors/getApi";
import { getNotificationBoxContent } from "model/actions/Notifications/GetNotificationBoxContent";

export class Notifications {
  @observable
  notificationBox: any;

  *getNotificationBoxContent() {
    this.notificationBox = yield getApi(this).getNotificationBoxContent();
  }

  loader = new PeriodicLoader(getNotificationBoxContent(this));

  *startTimer(refreshIntervalMs: number) {
    if(localStorage.getItem('debugNoPolling')) return
    yield* this.loader.start(refreshIntervalMs);
  }

  parent?: any;
}

