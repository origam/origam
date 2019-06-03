import { action } from "mobx";
import { IAFocusEditor } from "./types/IAFocusEditor";

export class AFocusEditor implements IAFocusEditor {
  constructor(public P: {}) {}

  @action.bound
  do() {
    for (let l of this.refocusListeners.values()) {
      l();
    }
  }

  refocusListeners = new Map<number, () => void>();
  refocusIdgen = 0;
  @action.bound
  listenForRefocus(cb: () => void): () => void {
    const myId = this.refocusIdgen++;
    this.refocusListeners.set(myId, cb);
    return () => this.refocusListeners.delete(myId);
  }
}
