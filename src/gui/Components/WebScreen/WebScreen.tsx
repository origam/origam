import React, {useCallback, useEffect, useState} from "react";
import S from "gui/Components/WebScreen/WebScreen.module.scss";
import cx from "classnames";

export const WebScreen: React.FC<{
  url: string;
  isLoading?: boolean;
  refIFrame?: any;
  onLoad?: any;
  onLoadStart?: any;
}> = props => {
  const [mouseDowned, setMouseDowned] = useState(false);

  useEffect(() => {
    const handleMouseDown = () => {
      setMouseDowned(true);
      window.addEventListener("mouseup", handleMouseup);
    };
    const handleMouseup = () => {
      setMouseDowned(false);
      window.removeEventListener("mouseup", handleMouseup);
    };

    window.addEventListener("mousedown", handleMouseDown);
    return () => window.removeEventListener("mousedown", handleMouseDown);
  }, []);

  const refIFrame = useCallback((elm: any) => {
    props.refIFrame && props.refIFrame(elm);
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

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
        ref={refIFrame}
        className={S.webContent}
        src={props.url}
      />
      <div className={cx(S.transparentOverlay, { isVisible: mouseDowned })} />
    </div>
  );
};
