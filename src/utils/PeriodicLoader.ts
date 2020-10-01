export class PeriodicLoader {
  private timeoutHandle: any;

  constructor(private loadFunction: () => Generator) {}

  private sleep(milliseconds: number) {
    return new Promise((resolve) => {
      this.timeoutHandle = setTimeout(resolve, milliseconds);
    });
  }

  *start(refreshIntervalMs: number) {
    if (refreshIntervalMs === 0) {
      return;
    }
    if (this.timeoutHandle) {
      yield* this.stop();
    }
    while (true) {
      const timeBefore = new Date();
      yield* this.loadFunction();
      const timeAfter = new Date();

      const loadTimeMs = timeAfter.valueOf() - timeBefore.valueOf();
      const msToWait = refreshIntervalMs - loadTimeMs;
      yield this.sleep(msToWait);
    }
  }

  *stop() {
    clearInterval(this.timeoutHandle);
    this.timeoutHandle = undefined;
  }
}