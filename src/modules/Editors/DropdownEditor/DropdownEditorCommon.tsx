export type CancellablePromise<T> = Promise<T> & { cancel(): void };

export const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const EagerlyLoadedGrid = "EagerlyLoadedGrid";
export const LazilyLoadedTree = "LazilyLoadedTree";
export const EagerlyLoadedTree = "EagerlyLoadedTree";
export const LazilyLoadedGrid = "LazilyLoadedGrid";
