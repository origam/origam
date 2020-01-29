import React from "react";
import S from "./WebScreen.module.scss";

export const WebScreen: React.FC<{url: string}> = props => (
  <iframe className={S.root} src={props.url} />
);