import React from "react";
import { observer, Provider } from "mobx-react";
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
    console.log(content);
    switch (content.$type) {
      case CFormScreen:
        return !content.isLoading ? (
          <Provider formScreen={content}>
            <FormScreen
              isLoading={false}
              isVisible={openedScreen.isActive}
              isFullScreen={false}
              title={openedScreen.title}
              isSessioned={content.isSessioned}
            >
              {!content.isLoading && (
                <FormScreenBuilder xmlWindowObject={content.screenUI} />
              )}
            </FormScreen>
          </Provider>
        ) : null;
      default:
        return null;
    }
  }
}
