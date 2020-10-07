import { flow } from "mobx";

export class PeriodicLoader {
  private timeoutHandle: any;

  constructor(private loadFunction: () => Generator) {}

  private sleep(milliseconds: number) {
    return new Promise((resolve) => {
      this.timeoutHandle = setTimeout(resolve, milliseconds);
    });
  }

  *start(refreshIntervalMs: number) {
    const self = this;
    flow(function* (){
      if (refreshIntervalMs <= 0) {
        return;
      }
      if (self.timeoutHandle) {
        yield* self.stop();
      }
      while (true) {
        const timeBefore = new Date();
        yield* self.loadFunction();
        const timeAfter = new Date();

        const loadTimeMs = timeAfter.valueOf() - timeBefore.valueOf();
        const msToWait = refreshIntervalMs - loadTimeMs;
        yield self.sleep(msToWait);
      }
    })();
  }

  *stop() {
    clearInterval(this.timeoutHandle);
    this.timeoutHandle = undefined;
  }
}