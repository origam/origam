import {IWorkQueues} from "./types/IWorkQueues";
import {getApi} from "model/selectors/getApi";
import {onRefreshWorkQueues} from "model/actions-ui/WorkQueues/onRefreshWorkQueues";
import {computed, observable} from "mobx";
import {PeriodicLoader} from "utils/PeriodicLoader";

export class WorkQueues implements IWorkQueues {
  $type_IWorkQueues: 1 = 1;

  *getWorkQueueList() {
    const api = getApi(this);
    const workQueues = yield api.getWorkQueueList();
    this.items = workQueues;
  }

  @observable items: any[] = [];
  @computed get totalItemCount() {
    return this.items.map(item => item.countTotal).reduce((a, b) => a + b, 0);
  }

  loader = new PeriodicLoader(onRefreshWorkQueues(this));


  *startTimer(refreshIntervalMs: number) {
    if(localStorage.getItem('debugNoPolling')) return
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
