import React from "react";
import { observer, Provider } from "mobx-react";
import { IOpenedScreen } from "../../../model/types/IOpenedScreen";
import { FormScreenBuilder } from "./FormScreenBuilder";
import { FormScreen } from "./FormScreen";
import { isILoadedFormScreen } from "../../../model/types/IFormScreen";

@observer
export class ScreenBuilder extends React.Component<{
  openedScreen: IOpenedScreen;
}> {
  render() {
    const { openedScreen } = this.props;
    const { content } = openedScreen;
    console.log(content);
    if (isILoadedFormScreen(content)) {
      return (
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
      );
    } else {
      return null;
    }
  }
}
