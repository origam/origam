import React from "react";
import S from "gui/connections/MobileComponents/BottomToolBar/BottomButton.module.scss";
import { Icon } from "gui/Components/Icon/Icon";
import cx from "classnames";

export const BottomButton: React.FC<{
  iconPath: string;
  caption: string;
  onClick: () => void;
  hidden?: boolean;
  className?: string;
}> = (props) => {
  return (
    <div className={cx(S.root, "bottomBarItem")}>
      <button
        className={S.button}
        onClick={props.onClick}
      >
        <div className={S.captionAndIcon}>
          <div className={S.icon}>
            <Icon
              src={props.iconPath}
              className={props.className + " " + (props.hidden ? S.hidden : "")}
            />
          </div>
          <div className={S.caption}>
            {props.caption}
          </div>
        </div>
      </button>
    </div>
  );
}