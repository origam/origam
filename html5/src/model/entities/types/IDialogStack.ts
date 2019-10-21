import { IDialogDimensions } from "gui/Components/Dialog/types";

export interface IDialogStack {
  stackedDialogs: Array<{
    key: string;
    component: React.ReactElement;
  }>;
  pushDialog(key: string, component: React.ReactNode, dialogDimensions? : IDialogDimensions): void;
  closeDialog(key: string): void;
  parent?: any;
}
