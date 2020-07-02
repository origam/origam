import {IWorkQueues} from "./types/IWorkQueues";
import {getApi} from "model/selectors/getApi";
import {onRefreshWorkQueues} from "model/actions-ui/WorkQueues/onRefreshWorkQueues";
import {computed, observable} from "mobx";

export class WorkQueues implements IWorkQueues {
  $type_IWorkQueues: 1 = 1;

  *getWorkQueueList() {
    const api = getApi(this);
    const workQueues = yield api.getWorkQueueList();
    // console.log(workQueues);
    this.items = workQueues;
  }

  @observable items: any[] = [];
  @computed get totalItemCount() {
    return this.items.map(item => item.countTotal).reduce((a, b) => a + b, 0);
  }

  hRefreshTimer: any;
  refreshInterval = 0;

  get isTimerRunning() {
    return !!this.hRefreshTimer;
  }

  *startTimer() {
    if (this.refreshInterval === 0) {
      throw new Error("Work queues refresh interval was not set.");
    }
    if (this.hRefreshTimer) {
      yield* this.stopTimer();
    }
    onRefreshWorkQueues(this)();
    this.hRefreshTimer = setInterval(
      onRefreshWorkQueues(this),
      this.refreshInterval
    );
    
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
