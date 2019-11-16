import React from "react";
import S from "./WorkbenchPage.module.scss";
import SSplitter from "styles/CustomSplitter.module.scss";
import { Splitter } from "../Splitter/Splitter";

export const WorkbenchPage: React.FC<{
  sidebar: React.ReactNode;
  mainbar: React.ReactNode;
}> = props => (
  <div className={S.root}>
    <Splitter
      type="isHoriz"
      STYLE={SSplitter}
      panels={[
        ["sidebar", 1, props.sidebar],
        ["mainbar", 5, props.mainbar]
      ]}
    />
  </div>
);
