import React from "react";
import S from "./WebScreen.module.scss";

export const WebScreen: React.FC<{url: string, refIFrame?: any}> = props => (
  <iframe ref={props.refIFrame} className={S.root} src={props.url} />
);