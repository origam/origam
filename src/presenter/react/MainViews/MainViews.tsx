import React from "react";
import style from "./MainViews.module.css";

import {
  IMainViews as IMainViewsModel,
  IAOnHandleClick,
  IAOnCloseClick,
  IScreenType,
  IMainView
} from "../../../Screens/types";
import { observer } from "mobx-react";
import { action } from "mobx";
import { FormScreen } from "../screenUI/FormScreen/FormScreen";
import {
  IFormScreen,
  isFormScreen
} from "../../../Screens/FormScreen/types";


@observer
class MainViewHandle extends React.Component<{
  menuItemId: string;
  order: number;
  label: string;
  mainViews: IMainViewsModel;
  onClick?: (event: any, menuItemId: string, order: number) => void;
  onCloseClick?: (event: any, menuItemId: string, order: number) => void;
}> {
  getOrderMark() {
    return this.props.order > 0 ? `[${this.props.order}]` : "";
  }

  @action.bound handleClick(event: any) {
    this.props.onClick &&
      this.props.onClick(event, this.props.menuItemId, this.props.order);
  }

  @action.bound handleCloseClick(event: any) {
    event.stopPropagation();
    this.props.onCloseClick &&
      this.props.onCloseClick(event, this.props.menuItemId, this.props.order);
  }

  render() {
    return (
      <div
        className={
          style.TabHandle +
          (this.props.menuItemId === this.props.mainViews.activeViewId &&
          this.props.order === this.props.mainViews.activeViewOrder
            ? ` ${style.active}`
            : "")
        }
        onClick={this.handleClick}
      >
        {this.props.label}
        {this.getOrderMark()}
        <button
          className={style.TabHandleCloseBtn}
          onClick={this.handleCloseClick}
        >
          <i className="fas fa-times" />
        </button>
      </div>
    );
  }
}

@observer
export class MainViews extends React.Component<{
  mainViews: IMainViewsModel;
  aOnHandleClick: IAOnHandleClick;
  aOnCloseClick: IAOnCloseClick;
}> {
  getScreen(ov: IMainView) {
    console.log(ov)
    if (isFormScreen(ov)) {
      return (
        <FormScreen key={`${ov.menuItemId}@${ov.order}`} formScreen={ov} />
      );
    }
    return "Unknown screen type.";
  }

  render() {
    return (
      <div className={style.Root}>
        <div className={style.TabHandles}>
          {this.props.mainViews.openedViews.map(ov => (
            <MainViewHandle
              key={`${ov.menuItemId}@${ov.order}`}
              label={ov.label}
              menuItemId={ov.menuItemId}
              order={ov.order}
              mainViews={this.props.mainViews}
              onClick={this.props.aOnHandleClick.do}
              onCloseClick={this.props.aOnCloseClick.do}
            />
          ))}
        </div>
        <div className={style.Screen}>
          {this.props.mainViews.openedViews.map(ov => this.getScreen(ov))}
        </div>
      </div>
    );
  }
}
