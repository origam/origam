import { observer, Provider } from "mobx-react";
import React from "react";
import { IAction } from "../../../model/entities/types/IAction";
import { IOpenedScreen } from "../../../model/entities/types/IOpenedScreen";
import { FormScreenBuilder } from "./FormScreenBuilder";



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
