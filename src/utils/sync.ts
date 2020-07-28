
interface ITriggerablePromise<T> extends Promise<T> {
  resolve(arg?: T): void;
  reject(arg?: any): void;
}

export function TriggerablePromise<T>() {
  let fnResolve;
  let fnReject;
  const promise = new Promise<T>((resolve, reject) => {
    fnResolve = resolve;
    fnReject = reject;
  });
  (promise as any).resolve = fnResolve;
  (promise as any).reject = fnReject;
  return promise as ITriggerablePromise<T>;
}

export class CriticalSection {
  constructor() {}

  tokensIn = 0;

  tokenQueue: ITriggerablePromise<any>[] = [];

  enqueue() {
    const promise = TriggerablePromise();
    this.tokenQueue.push(promise);
    return promise;
  }

  dequeue() {
    const promise = this.tokenQueue.shift();
    promise?.resolve();
  }

  incTokens() {
    this.tokensIn++;
  }

  decTokens() {
    this.tokensIn--;
  }

  enterPromise() {
    this.incTokens();
    if (this.tokensIn > 1) {
      return this.enqueue();
    }
  }

  *enterGenerator() {
    this.incTokens();
    if (this.tokensIn > 1) {
      yield this.enqueue();
    }
  }

  leave() {
    this.decTokens();
    if (this.tokensIn > 0) {
      this.dequeue();
    }
  }

  async runAsync<R>(fn: () => Promise<R>) {
    try {
      await this.enterPromise();
      return fn();
    } finally {
      this.leave();
    }
  }

  *runGenerator<R>(fn: () => Generator<any, R>) {
    try {
      yield* this.enterGenerator();
      return yield* fn();
    } finally {
      this.leave();
    }
  }
}