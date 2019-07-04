import React from "react";
import { observer } from "mobx-react";
import { IOpenedScreen } from "../../../model/types/IOpenedScreen";
import { CFormScreen } from "../../../model/types/IFormScreen";
import { FormScreenBuilder } from "./FormScreenBuilder";
import { FormScreen } from "./FormScreen";

@observer
export class ScreenBuilder extends React.Component<{
  openedScreen: IOpenedScreen;
}> {
  render() {
    const { openedScreen } = this.props;
    const { content } = openedScreen;
    switch (content.$type) {
      case CFormScreen:
        return (
          <FormScreen
            isLoading={false}
            isVisible={openedScreen.isActive}
            isFullScreen={false}
            title={openedScreen.title}
            isSessioned={false}
          >
            {!content.isLoading && (
              <FormScreenBuilder xmlWindowObject={content.screenUI} />
            )}
          </FormScreen>
        );
      default:
        return null;
    }
  }
}
