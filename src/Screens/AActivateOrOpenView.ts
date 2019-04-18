import { ML } from "../utils/types";
import {
  IAActivateView,
  IAOpenView,
  IMainViews,
  IAActivateOrOpenView
} from "./types";
import { unpack } from "../utils/objects";
import { ICommandType } from "../MainMenu/MainMenu";

export class AActivateOrOpenView implements IAActivateOrOpenView {
  constructor(
    public P: {
      aActivateView: ML<IAActivateView>;
      aOpenView: ML<IAOpenView>;
      mainViews: ML<IMainViews>;
    }
  ) {}

  do(id: string, itemType: ICommandType, menuItemLabel: string) {
    const alreadyOpened = this.mainViews.findFirstById(id);
    if (alreadyOpened) {
      this.aActivateView.do(alreadyOpened.menuItemId, alreadyOpened.order);
    } else {
      this.aOpenView.do(id, itemType, menuItemLabel);
    }
  }

  get mainViews() {
    return unpack(this.P.mainViews);
  }

  get aActivateView() {
    return unpack(this.P.aActivateView);
  }

  get aOpenView() {
    return unpack(this.P.aOpenView);
  }
}
