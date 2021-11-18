import { IDialogDimensions } from "gui/Components/Dialog/types";

export interface IDialogInfo {
  key: string;
  component: React.ReactElement;
  dimensions?: IDialogDimensions;
  closeOnClickOutside?: boolean;
}
