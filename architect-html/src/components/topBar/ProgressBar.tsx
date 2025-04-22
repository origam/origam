import S from "src/components/topBar/ProgressBar.module.scss";
import React, { useContext } from "react";
import { RootStoreContext } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import { createPortal } from "react-dom";

export const ProgressBar: React.FC = observer(() => {
  const progressBarState = useContext(RootStoreContext).progressBarState;

  return (
    <>
      {(progressBarState.isWorking || window.localStorage.getItem("debugKeepProgressIndicatorsOn")) && (
        <div className={S.progressIndicator}>
          <div className={S.indefinite}/>
        </div>
      )}
      <Overlay/>
    </>
  );
});

export const Overlay: React.FC = observer(() => {
  const progressBarState = useContext(RootStoreContext).progressBarState;

  return createPortal(
    <>{progressBarState.isWorking && <div className={S.overlay}/>}</>,
    document.getElementById("modal-window-portal")!
  );
});
