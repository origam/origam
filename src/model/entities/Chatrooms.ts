import { action, computed, observable } from "mobx";
import { onRefreshChatrooms } from "model/actions/Chatrooms/onRefreshChatrooms";
import { getApi } from "model/selectors/getApi";

export class Chatrooms {
  *getChatroomsList() {
    const api = getApi(this);
    const chatrooms = yield api.getChatroomList();
    this.items = chatrooms;
  }

  @observable items: any[] = [];
  @computed get totalItemCount() {
    return this.items.map((item) => item.unreadMessageCount).reduce((a, b) => a + b, 0);
  }

  get sortedItems() {
    return this.items;
  }

  hRefreshTimer: any;
  refreshInterval = 0;

  get isTimerRunning() {
    return !!this.hRefreshTimer;
  }

  *startTimer() {
    if (this.refreshInterval === 0) {
      return;
    }
    if (this.hRefreshTimer) {
      yield* this.stopTimer();
    }
    onRefreshChatrooms(this)();
    this.hRefreshTimer = setInterval(() => {
      onRefreshChatrooms(this)();
    }, this.refreshInterval);
  }

  *stopTimer() {
    clearInterval(this.hRefreshTimer);
    this.hRefreshTimer = undefined;
  }

  *setRefreshInterval(ms: number) {
    const willRestart = this.isTimerRunning;
    if (willRestart) {
      yield* this.stopTimer();
    }
    this.refreshInterval = ms;
    if (willRestart) {
      yield* this.startTimer();
    }
  }

  parent?: any;
}
