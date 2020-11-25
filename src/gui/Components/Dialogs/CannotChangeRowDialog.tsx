import React from "react";
import { CloseButton, ModalWindow } from "../Dialog/Dialog";
import { observer } from "mobx-react";
import { getDialogStack } from "../../../model/selectors/DialogStack/getDialogStack";

@observer
export class CannotChangeRowDialog extends React.Component<{
  onCloseClick?: (event: any) => void;
  onOkClick?: (event: any) => void;
}> {
  render() {
    return (
      <ModalWindow
        title="Cannot change row"
        titleButtons={<CloseButton onClick={this.props.onCloseClick} />}
        buttonsCenter={
          <>
            <button tabIndex={0} autoFocus={true} onClick={this.props.onOkClick}>
              OK
            </button>
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

let nextId = 0;

export function cannotChangeRowDialog(ctx: any) {
  const key = `CannotChangeRowDialog@${nextId}`;
  return new Promise((resolve) => {
    getDialogStack(ctx).pushDialog(
      key,
      <CannotChangeRowDialog
        onOkClick={() => {
          getDialogStack(ctx).closeDialog(key);
          resolve();
        }}
        onCloseClick={() => {
          getDialogStack(ctx).closeDialog(key);
          resolve();
        }}
      />
    );
  });
}
