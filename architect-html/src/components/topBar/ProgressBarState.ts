import { observable } from "mobx";

export class ProgressBarState {
  @observable private accessor _isWorking = false;
  private timeout;
  get isWorking(): boolean {
    return this._isWorking;
  }

  set isWorking(isWorkingNow: boolean) {
    if(this.timeout) {
      clearTimeout(this.timeout);
    }

    this.timeout = setTimeout(() => {
      this._isWorking = isWorkingNow;
    }, !this._isWorking && isWorkingNow ? 100 : 0);
  }
}