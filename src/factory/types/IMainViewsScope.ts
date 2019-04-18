import {
  IMainViews,
  IAActivateOrOpenView,
  IAOpenView,
  IACloseView,
  IAActivateView,
  IAOnHandleClick,
  IAOnCloseClick,
  IMainViewFactory
} from "../../Screens/types";

export interface IMainViewsScope {
  mainViews: IMainViews;
  mainViewFactory: IMainViewFactory
  aActivateOrOpenView: IAActivateOrOpenView;
  aOpenView: IAOpenView;
  aCloseView: IACloseView;
  aActivateView: IAActivateView;
  aOnHandleClick: IAOnHandleClick;
  aOnCloseClick: IAOnCloseClick;
}
