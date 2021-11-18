export interface IErrorDialogController {
  errorMessages: any[];
  pushError(error: any): Generator;
  dismissError(id: number): void;
  dismissErrors(): void;

  parent?: any;
}
