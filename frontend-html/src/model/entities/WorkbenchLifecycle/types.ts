import {onChoiceTaken, onInitPortalDone, onInitUIDone, onOpenScreen, onOpenScreenWithSelection} from "./constants";

export interface IOnOpenScreen {
  type: typeof onOpenScreen;
  menuItemId: string;
  menuItemLabel: string;
  menuItemType: string;
  forceOpenNew: boolean;
}

export interface IOnOpenScreenWithSelection {
  type: typeof onOpenScreenWithSelection;
  menuItemId: string;
  menuItemLabel: string;
  menuItemType: string;
  forceOpenNew: boolean;
}

export type IEvent =
  | {
      type: typeof onInitPortalDone;
    }
  | {
      type: typeof onInitUIDone;
    }
  | {
      type: typeof onChoiceTaken;
    }
  | IOnOpenScreen
  | IOnOpenScreenWithSelection;
