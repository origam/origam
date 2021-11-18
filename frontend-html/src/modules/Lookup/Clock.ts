import _ from "lodash";
import { TypeSymbol } from "dic/Container";

export class Clock {
  debounce(fn: () => void, ms: number) {
    return _.debounce(fn, ms);
  }

  getTimeMs() {
    return new Date().getTime();
  }

  setInterval(fn: () => void, ms:number) {
    const handle = setInterval(fn, ms);
    return () => clearInterval(handle);
  }
}
export const IClock = TypeSymbol<Clock>("IClock");
