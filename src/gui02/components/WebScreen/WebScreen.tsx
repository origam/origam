import React from "react";
import S from "./WebScreen.module.scss";

export const WebScreen: React.FC<{
  url: string;
  isLoading?: boolean;
  refIFrame?: any;
  onLoad?: any;
  onLoadStart?: any;
}> = props => (
  <div className={S.root}>
    {props.isLoading && (
      <div className={S.progressIndicator}>
        <div className={S.indefinite} />
      </div>
    )}
    <iframe
      onLoad={props.onLoad}
      onLoadStart={props.onLoadStart}
      ref={props.refIFrame}
      className={S.webContent}
      src={props.url}
    />
  </div>
);
