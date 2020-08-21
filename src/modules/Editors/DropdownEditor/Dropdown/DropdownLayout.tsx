import Measure, { BoundingRect, ContentRect } from "react-measure";
import React, { useRef, useState, useEffect } from "react";
import ReactDOM from "react-dom";
import {
  CtxDropdownRefBody,
  CtxDropdownRefCtrl,
  CtxDropdownBodyRect,
  CtxDropdownCtrlRect,
} from "./DropdownCommon";

export function DropdownLayout(props: {
  isDropped: boolean;
  renderCtrl: () => React.ReactNode;
  renderDropdown: () => React.ReactNode;
}) {
  function handleCtrlBlockResize(contentRect: ContentRect) {
    const { top, left, width, height, bottom, right } = contentRect.bounds!;
    setRectCtrl({
      top,
      left,
      width,
      height,
      bottom,
      right,
    });
  }

  function handleBodyBlockResize(contentRect: ContentRect) {
    const { top, left, width, height, bottom, right } = contentRect.bounds!;
    setRectBody({
      top,
      left,
      width,
      height,
      bottom,
      right,
    });
  }

  const refMeasureCtrl = useRef<Measure>(null);
  const refMeasureBody = useRef<Measure>(null);

  const [rectCtrl, setRectCtrl] = useState<BoundingRect>({
    top: 0,
    left: 0,
    width: 0,
    height: 0,
    bottom: 0,
    right: 0,
  });

  const [rectBody, setRectBody] = useState<BoundingRect>({
    top: 0,
    left: 0,
    width: 0,
    height: 0,
    bottom: 0,
    right: 0,
  });

  const reMeasure = () => {
    if (refMeasureCtrl.current){
      (refMeasureCtrl.current as any).measure();
    }
    if (refMeasureBody.current){
      (refMeasureBody.current as any).measure();
    }
  };

  useEffect(() => {
    const handleScroll = (event: any) => {
      reMeasure();
    };
    const handleMouse = (event: any) => {
      reMeasure();
    };
    window.addEventListener("scroll", handleScroll, true);
    window.addEventListener("mousedown", handleMouse, true);
    window.addEventListener("mouseup", handleMouse, true);
    return () => {
      window.removeEventListener("scroll", handleScroll, true);
      window.removeEventListener("mousedown", handleMouse, true);
      window.removeEventListener("mouseup", handleMouse, true);
    };
  }, []);

  return (
    <>
      <CtxDropdownBodyRect.Provider value={rectBody}>
        <CtxDropdownCtrlRect.Provider value={rectCtrl}>
          <Measure ref={refMeasureCtrl} bounds={true} onResize={handleCtrlBlockResize}>
            {({ measureRef }) => (
              <CtxDropdownRefCtrl.Provider value={measureRef}>
                {props.renderCtrl()}
              </CtxDropdownRefCtrl.Provider>
            )}
          </Measure>
          {props.isDropped &&
            ReactDOM.createPortal(
              <Measure ref={refMeasureBody} bounds={true} onResize={handleBodyBlockResize}>
                {({ measureRef }) => (
                  <CtxDropdownRefBody.Provider value={measureRef}>
                    {props.renderDropdown()}
                  </CtxDropdownRefBody.Provider>
                )}
              </Measure>,
              document.getElementById("dropdown-portal")!
            )}
        </CtxDropdownCtrlRect.Provider>
      </CtxDropdownBodyRect.Provider>
    </>
  );
}
