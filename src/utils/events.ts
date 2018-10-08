export type ISubscriber = (...args: any[]) => void;

export interface IEventSubscriber {
  (fn: ISubscriber): () => {};
  trigger(...args: any[]): void;
}

export function EventObserver(): IEventSubscriber {
  let nextId = 0;
  const listeners = new Map<number, ISubscriber>();
  const subscribe = ((fn: (...args: any[]) => void) => {
    const id = nextId++;
    listeners.set(id, fn);
    return function unsubscribe() {
      listeners.delete(id);
    }
  }) as IEventSubscriber;
  subscribe.trigger = function trigger(...args) {
    for(const listener of listeners.values()) {
      listener(...args);
    }
  }
  return subscribe;
}