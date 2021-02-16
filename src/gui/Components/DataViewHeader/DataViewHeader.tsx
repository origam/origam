import React from "react";
import S from "gui/Components/DataViewHeader/DataViewHeader.module.scss";
import cx from 'classnames';

export const DataViewHeader: React.FC<{ domRef?: any, isVisible: boolean }> = props => (
  <div className={cx(S.root, {isVisible: props.isVisible})}>
    <div ref={props.domRef} className={S.inner}>
      {props.children}
    </div>
  </div>
);
