import React, { RefObject } from "react";
import S from "gui/Components/WorkQueues/WorkQueuesItem.module.scss";
import cx from "classnames";
import { MobXProviderContext } from "mobx-react";
import { getMainMenuState } from "model/selectors/MainMenu/getMainMenuState";

export class WorkQueuesItem extends React.Component<{
  isActiveScreen?: boolean;
  isOpenedScreen?: boolean;
  isHidden?: boolean;
  isEmphasized?: boolean;
  level?: number;
  icon?: React.ReactNode;
  label?: React.ReactNode;
  tooltip?: string;
  id?: string;
  onClick?(event: any): void;
}> {
  static contextType = MobXProviderContext;
  itemRef: RefObject<HTMLAnchorElement> = React.createRef();

  componentDidMount() {
    if (this.props.id) {
      this.mainMenuState.setReference(this.props.id, this.itemRef);
    }
  }

  get mainMenuState() {
    return getMainMenuState(this.context.application);
  }

  render() {
    return (
      <a
        ref={this.itemRef}
        className={cx(
          S.root,
          {
            isActiveScreen: this.props.isActiveScreen,
            isOpenedScreen: this.props.isOpenedScreen,
          },
          { isHidden: this.props.isHidden },
          { isEmphasized: this.props.isEmphasized }
        )}
        style={{ paddingLeft: `${(this.props.level || 1) * 1.6667}em` }}
        onClick={this.props.onClick}
        title={this.props.tooltip}
      >
        <div className={S.icon}>{this.props.icon}</div>
        <div className={S.label}>{this.props.label}</div>
      </a>
    );
  }
}
