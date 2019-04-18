import { IMainViewsScope } from "./types/IMainViewsScope";
import {
  IMainViews,
  IAActivateOrOpenView,
  IAOpenView,
  IACloseView,
  IAActivateView,
  IMainViewFactory,
  IAOnHandleClick,
  IAOnCloseClick
} from "../Screens/types";
import { AActivateOrOpenView } from "../Screens/AActivateOrOpenView";
import { MainViews } from "../Screens/MainViews";
import { AOpenView } from "../Screens/AOpenView";
import { AActivateView } from "../Screens/AActivateView";
import { MainViewFactory } from "../Screens/MainViewFactory";
import { ACloseView } from "../Screens/ACloseView";
import { AOnHandleClick } from "../Screens/AOnHandleClick";
import { AOnCloseClick } from "../Screens/AOnCloseClick";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { IApi } from "../Api/IApi";

export class MainViewsScope implements IMainViewsScope {
  constructor(
    public P: {
      api: ML<IApi>;
    }
  ) {}

  mainViews: IMainViews = new MainViews({});
  mainViewFactory: IMainViewFactory = new MainViewFactory({
    mainViews: () => this.mainViews,
    api: () => this.api
  });

  aActivateOrOpenView: IAActivateOrOpenView = new AActivateOrOpenView({
    aActivateView: () => this.aActivateView,
    aOpenView: () => this.aOpenView,
    mainViews: () => this.mainViews
  });
  aOpenView: IAOpenView = new AOpenView({
    mainViews: () => this.mainViews,
    aActivateView: () => this.aActivateView,
    mainViewFactory: () => this.mainViewFactory
  });
  aCloseView: IACloseView = new ACloseView({
    mainViews: () => this.mainViews,
    aActivateView: () => this.aActivateView
  });
  aActivateView: IAActivateView = new AActivateView({
    mainViews: () => this.mainViews
  });
  aOnHandleClick: IAOnHandleClick = new AOnHandleClick({
    aActivateView: () => this.aActivateView
  });
  aOnCloseClick: IAOnCloseClick = new AOnCloseClick({
    aCloseView: () => this.aCloseView
  });

  get api() {
    return unpack(this.P.api);
  }
}
