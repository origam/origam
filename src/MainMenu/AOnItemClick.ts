import { action } from "mobx";
import { ICommandType } from "./MainMenu";
import { ML } from "../utils/types";
import {
  IAOpenView,
  IAActivateOrOpenView
} from "../Screens/types";
import { unpack } from "../utils/objects";
import { IAOnItemClick } from "./types";

export class AOnItemClick implements IAOnItemClick{
  constructor(
    public P: {
      aOpenView: ML<IAOpenView>;
      aActivateOrOpenView: ML<IAActivateOrOpenView>;
    }
  ) {}

  @action.bound
  do(event: any, itemId: string, itemType: ICommandType, menuItemLabel: string) {
    if (event.ctrlKey) {
      this.aOpenView.do(itemId, itemType, menuItemLabel);
    } else {
      this.aActivateOrOpenView.do(itemId, itemType, menuItemLabel);
    }
    console.log("Clicked", itemId, itemType);
  }

  get aOpenView() {
    return unpack(this.P.aOpenView);
  }

  get aActivateOrOpenView() {
    return unpack(this.P.aActivateOrOpenView);
  }
}
