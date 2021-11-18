import { IDialogDimensions } from "gui/Components/Dialog/types";
import { IDialogInfo } from "./IDialogInfo";

export interface IDialogStack {
  stackedDialogs: Array<IDialogInfo>;
  pushDialog(
    key: string,
    component: React.ReactElement,
    dialogDimensions?: IDialogDimensions,
    closeOnClickOutside?: boolean
  ): () => void;
  closeDialog(key: string): void;
  isOpen(key: string): boolean;
  isAnyDialogShown: boolean;
  parent?: any;
}
