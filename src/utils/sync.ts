/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/


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

export function sleep(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}
