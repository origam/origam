import { FormScreenBuilder } from "gui/Workbench/ScreenArea/FormScreenBuilder";
import { observer, Provider } from "mobx-react";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import React, { useState, useEffect } from "react";
import { Screen } from "../components/Screen/Screen";
import { ScreenContainer } from "gui02/components/Screen/ScreenContainer";
import { CtxPanelVisibility } from "gui02/contexts/GUIContexts";
import { WebScreen } from "gui02/components/WebScreen/WebScreen";
import { IWebScreen } from "model/entities/types/IWebScreen";

const WebScreenComposite: React.FC<{ openedScreen: IOpenedScreen }> = observer(props => {
  const { openedScreen } = props;
  const [isLoading, setLoading] = useState(false);
  useEffect(() => {
    if (openedScreen.screenUrl) {
      setLoading(true);
    }
  }, []);
  return (
    <Screen isHidden={!openedScreen.isActive}>
      <WebScreen
        url={openedScreen.screenUrl || ""}
        isLoading={isLoading}
        onLoad={() => {
          setLoading(false);
        }}
        refIFrame={(elm: any) => {
          ((openedScreen as unknown) as IWebScreen).setReloader(
            elm
              ? {
                  reload: () => {
                    setLoading(true);
                    elm.contentWindow.location.reload();
                  }
                }
              : null
          );
        }}
      />
    </Screen>
  );
});

@observer
export class CScreen extends React.Component<{
  openedScreen: IOpenedScreen;
}> {
  render() {
    const { openedScreen } = this.props;
    if (openedScreen.screenUrl) {
      return <WebScreenComposite openedScreen={openedScreen} />;
    }
    if (!openedScreen.content) return null;
    const formScreen = openedScreen.content;
    return (
      <Provider formScreen={formScreen}>
        <>
          {!formScreen.isLoading && (
            <Screen isHidden={!openedScreen.isActive}>
              <CtxPanelVisibility.Provider value={{ isVisible: openedScreen.isActive }}>
                <FormScreenBuilder xmlWindowObject={formScreen.formScreen!.screenUI} />
              </CtxPanelVisibility.Provider>
            </Screen>
          )}
        </>
      </Provider>
    );
  }
}
