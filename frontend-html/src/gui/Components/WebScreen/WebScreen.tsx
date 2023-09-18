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

import React, { useCallback, useEffect, useState } from "react";
import S from "gui/Components/WebScreen/WebScreen.module.scss";
import cx from "classnames";

export const WebScreen: React.FC<{
  source: string;
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
          <div className={S.indefinite}/>
        </div>
      )}
      <iframe
        onLoad={props.onLoad}
        onLoadStart={props.onLoadStart}
        ref={refIFrame}
        className={S.webContent}
        src={props.source}
      />
      <div className={cx(S.transparentOverlay, {isVisible: mouseDowned})}/>
    </div>
  );
};
