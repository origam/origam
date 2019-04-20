export interface ILookupResolver {
  isLoading(key: string): boolean;
  isError(key: string): boolean;
  getValue(key: string): any;
  start(): void;
  stop(): void;
}
