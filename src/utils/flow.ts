import {observable} from "mobx";

export const busyDelayMillis = 500;

// Delays setting "isWorking" property by "busyDelayMillis" after "inFlow" counter is incremented from 0 to 1
export class FlowBusyMonitor {
  @observable _inFlow = 0;
  @observable isWorking = false;
  private flowLeft = false;

  set inFlow(value: number) {
    if (this._inFlow === 0 && value === 1) {
      this.flowLeft = false;
      setTimeout(() => {
        if (!this.flowLeft) {
          this.isWorking = true
        }
      }, busyDelayMillis);
    }
    else if (this._inFlow === 1 && value === 0) {
      this.isWorking = false;
      this.flowLeft = true;
    }
    this._inFlow = value;
  }

  get inFlow() {
    return this._inFlow;
  }
}