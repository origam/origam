export interface ITableViewMachine {
  isActive: boolean;
  start(): void;
  stop(): void;
  send(event: any): void;
}