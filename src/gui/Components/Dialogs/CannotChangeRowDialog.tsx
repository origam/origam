import React from "react";
import { ModalWindowOverlay, ModalWindow } from "../Dialog/Dialog";
import { observer } from "mobx-react";

@observer
export class CannotChangeRowDialog extends React.Component<{}> {
  render() {
    return (
      <ModalWindow
        title="Cannot change row"
        buttonsCenter={
          <>
            <button>OK</button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        Selected row cannot be changed now, because....
      </ModalWindow>
    );
  }
}
