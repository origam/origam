export type CancellablePromise<T> = Promise<T> & { cancel(): void };
