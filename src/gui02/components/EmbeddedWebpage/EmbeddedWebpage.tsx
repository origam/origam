import React, { useState, useEffect } from "react";
import S from "./EmbeddedWebpage.module.scss";
import cx from "classnames";

function getRootStyle(props: { height?: number }) {
  if (props.height !== undefined) {
    return {
      flexGrow: 0,
      height: props.height
    };
  } else {
    return {
      flexGrow: 1,
      flexShrink: 0
      // width: "100%",
      // height: "100%"
    };
  }
}

function effMouseEventBlocking(setBlocked: (state: boolean) => void) {
  const handleMouseDown = (event: any) => {
    setBlocked(true);
    window.addEventListener("mouseup", handleMouseUp);
  };
  const handleMouseUp = (event: any) => {
    setBlocked(false);
    window.removeEventListener("mouseup", handleMouseUp);
  };
  window.addEventListener("mousedown", handleMouseDown);
}

export const EmbeddedWebpage: React.FC<{
  id: string;
  height?: number;
}> = props => {
  const [isBlocked, setBlocked] = useState(false);
  useEffect(() => effMouseEventBlocking(setBlocked), []);
  return (
    <div className={S.root} style={getRootStyle(props)}>
      <iframe className={S.iframe} src="http://origam.com" />
      <div className={cx(S.blockerOverlay, { isDisplayed: isBlocked })} />
    </div>
  );
};
