import { FormScreenBuilder } from "gui/Workbench/ScreenArea/FormScreenBuilder";
import { observer, Provider } from "mobx-react";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import React from "react";
import { Screen } from "../components/Screen/Screen";
import { ScreenContainer } from "gui02/components/Screen/ScreenContainer";
import { CtxPanelVisibility } from "gui02/contexts/GUIContexts";

@observer
export class CScreen extends React.Component<{
  openedScreen: IOpenedScreen;
}> {
  render() {
    const { openedScreen } = this.props;
    const formScreen = openedScreen.content;
    return (
      <Provider formScreen={formScreen}>
        <>
          {!formScreen.isLoading && (
            <Screen isHidden={!openedScreen.isActive}>
              <CtxPanelVisibility.Provider
                value={{ isVisible: openedScreen.isActive }}
              >
                <FormScreenBuilder
                  xmlWindowObject={formScreen.formScreen!.screenUI}
                />
              </CtxPanelVisibility.Provider>
            </Screen>
          )}
        </>
      </Provider>
    );
  }
}
