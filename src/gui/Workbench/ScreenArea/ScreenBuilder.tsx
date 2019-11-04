import React from "react";
import { observer, Provider } from "mobx-react";
import { IOpenedScreen } from "../../../model/entities/types/IOpenedScreen";
import { FormScreenBuilder } from "./FormScreenBuilder";
import { FormScreen } from "./FormScreen";
import { IAction } from "../../../model/entities/types/IAction";
import { FormScreenLoading } from "./FormScreenLoading";

@observer
export class ScreenBuilder extends React.Component<{
  openedScreen: IOpenedScreen;
}> {
  render() {
    const { openedScreen } = this.props;
    const { content } = openedScreen;
    if (!content.isLoading) {
      return (
        <Provider formScreen={content}>
          <FormScreen
            isLoading={false}
            isVisible={openedScreen.isActive}
            isFullScreen={false}
            title={
              !content.isLoading
                ? openedScreen.content.formScreen!.title
                : openedScreen.title
            }
          >
            {!content.isLoading && (
              <FormScreenBuilder
                xmlWindowObject={content.formScreen!.screenUI}
              />
            )}
          </FormScreen>
        </Provider>
      );
    } else {
      return <FormScreenLoading />;
    }
  }
}

export class DialogActionButton extends React.Component<{ action: IAction }> {
  render() {
    return <button>{this.props.action.caption}</button>;
  }
}

@observer
export class DialogScreenBuilder extends React.Component<{
  openedScreen: IOpenedScreen;
}> {
  render() {
    const { openedScreen } = this.props;
    const { content } = openedScreen;
    if (!content.isLoading) {
      return (
        <Provider formScreen={content}>
          {!content.isLoading && (
            <>
              <FormScreenBuilder
                xmlWindowObject={content.formScreen!.screenUI}
              />
            </>
          )}
        </Provider>
      );
    } else {
      return null;
    }
  }
}
