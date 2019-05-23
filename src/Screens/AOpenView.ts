import { IAOpenView, IMainViewFactory, IAActivateView } from "./types";
import { action } from "mobx";
import { ML } from "../utils/types";
import { IMainViews } from "./types";
import { unpack } from "../utils/objects";
import { ICommandType } from "../MainMenu/MainMenu";
import * as ScreensActions from "./ScreensActions";

export class AOpenView implements IAOpenView {
  constructor(
    public P: {
      mainViews: ML<IMainViews>;
      mainViewFactory: ML<IMainViewFactory>;
      aActivateView: ML<IAActivateView>;
    }
  ) {}

  @action.bound
  do(id: string, itemType: ICommandType, menuItemLabel: string): void {
    const alreadyOpened = this.mainViews.findLastById(id);
    let order = 0;
    if (alreadyOpened) {
      order = alreadyOpened.order + 1;
    }
    const newView = this.mainViewFactory.create(
      id,
      order,
      itemType,
      menuItemLabel
    );
    this.mainViews.pushView(newView);
    // newView.open();
    this.aActivateView.do(id, order);
    newView.dispatch(ScreensActions.startScreen());
  }

  get mainViews() {
    return unpack(this.P.mainViews);
  }

  get mainViewFactory() {
    return unpack(this.P.mainViewFactory);
  }

  get aActivateView() {
    return unpack(this.P.aActivateView);
  }
}
