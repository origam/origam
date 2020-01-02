import React, { useContext } from "react";
import S from "./TabbedViewPanel.module.scss";
import cx from "classnames";
import { CtxPanelVisibility } from "gui02/contexts/GUIContexts";

export const TabbedViewPanel: React.FC<{ isActive: boolean }> = props => {
  const ctxPanelVisibility = useContext(CtxPanelVisibility);
  return (
    <CtxPanelVisibility.Provider
      value={{ isVisible: props.isActive && ctxPanelVisibility.isVisible }}
    >
      <div className={cx(S.root, { isActive: props.isActive })}>
        {props.children}
      </div>
    </CtxPanelVisibility.Provider>
  );
};
