import React, { useState, useEffect } from "react";
import S from "./WebScreen.module.scss";
import cx from "classnames";

export const WebScreen: React.FC<{
  url: string;
  isLoading?: boolean;
  refIFrame?: any;
  onLoad?: any;
  onLoadStart?: any;
}> = props => {
  const [mouseDowned, setMouseDowned] = useState(false);
  const handleMousedown = () => {
    setMouseDowned(true);
    window.addEventListener("mouseup", handleMouseup);
  };
  const handleMouseup = () => {
    setMouseDowned(false);
    window.removeEventListener("mouseup", handleMouseup);
  };
  useEffect(() => {
    window.addEventListener("mousedown", handleMousedown);
    return () => window.removeEventListener("mousedown", handleMousedown);
  }, []);

  return (
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
      <div className={cx(S.transparentOverlay, { isVisible: mouseDowned })} />
    </div>
  );
};
