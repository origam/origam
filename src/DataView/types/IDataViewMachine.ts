export interface IDataViewMachine {
  start(): void;
  stop(): void;
  loadFresh(): void;
}