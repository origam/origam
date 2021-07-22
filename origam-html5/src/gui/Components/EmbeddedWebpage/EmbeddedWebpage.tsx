/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import React, {useEffect, useState} from "react";
import S from "gui/Components/EmbeddedWebpage/EmbeddedWebpage.module.scss";
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
