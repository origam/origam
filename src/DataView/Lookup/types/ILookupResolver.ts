export interface ILookupResolver {
  lookupId: string;
  isLoading(key: string): boolean;
  isError(key: string): boolean;
  getValue(key: string): any;
  start(): void;
  stop(): void;
}
