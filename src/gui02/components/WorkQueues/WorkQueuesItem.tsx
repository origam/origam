import React from "react";
import S from "./WorkQueuesItem.module.scss";
import cx from "classnames";

export const WorkQueuesItem: React.FC<{
  isActive?: boolean;
  isHidden?: boolean;
  isEmphasized?: boolean;
  level?: number;
  icon?: React.ReactNode;
  label?: React.ReactNode;
  onClick?(event: any): void;
}> = props => (
  <a
    className={cx(
      S.root,
      {
        isActive: props.isActive
      },
      { isHidden: props.isHidden },
      { isEmphasized: props.isEmphasized }
    )}
    style={{ paddingLeft: `${(props.level || 1) * 1.6667}em` }}
    onClick={props.onClick}
  >
    <div className={S.icon}>{props.icon}</div>
    <div className={S.label}>{props.label}</div>
  </a>
);
