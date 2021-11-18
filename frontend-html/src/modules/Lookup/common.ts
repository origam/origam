export class PubSub<T> {
  idGen = 1;
  handlers = new Map<number, (args: T) => void>();

  subscribe(fn: (args: T) => void) {
    const myId = this.idGen++;
    this.handlers.set(myId, fn);
    return () => this.handlers.delete(myId);
  }

  trigger(args: T) {
    for (let h of this.handlers.values()) h(args);
  }
}

export const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export type InterfaceOf<C> = {
  [K in keyof C]: C[K]
}