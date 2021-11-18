export type IPSSubscriber<T> = (arg?: T) => void;

export class PubSub<T> {
  newId = 0;
  subscribers = new Map();

  subscribe(subscriber: IPSSubscriber<T>) {
    const myId = this.newId++;
    this.subscribers.set(myId, subscriber);
    return () => {
      this.subscribers.delete(myId);
    };
  }

  trigger(arg?: T) {
    for (let subscriber of this.subscribers.values()) {
      subscriber(arg);
    }
  }
}