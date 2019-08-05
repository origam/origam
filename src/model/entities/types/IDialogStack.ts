export interface IDialogStack {
  stackedDialogs: Array<{
    key: string;
    component: React.ReactElement;
  }>;
  pushDialog(key: string, component: React.ReactNode): void;
  closeDialog(key: string): void;
  parent?: any;
}
