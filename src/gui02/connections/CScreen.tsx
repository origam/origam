import { FormScreenBuilder } from "gui/Workbench/ScreenArea/FormScreenBuilder";
import { observer, Provider } from "mobx-react";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import React, { useEffect, useMemo, useRef, useState } from "react";
import { Screen } from "../components/Screen/Screen";
import { CtxPanelVisibility } from "gui02/contexts/GUIContexts";
import { WebScreen } from "gui02/components/WebScreen/WebScreen";
import { IWebScreen } from "model/entities/types/IWebScreen";
import { getIsTopmostNonDialogScreen } from "model/selectors/getIsTopmostNonDialogScreen";
import { ErrorBoundary, ErrorBoundaryEncapsulated } from "gui02/components/Utilities/ErrorBoundary";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { IFormScreenEnvelope } from "model/entities/types/IFormScreen";

const WebScreenComposite: React.FC<{ openedScreen: IOpenedScreen }> = observer((props) => {
  const { openedScreen } = props;
  const [isLoading, setLoading] = useState(false);
  const refIFrame = useRef<any>(null);
  useEffect(() => {
    if (openedScreen.screenUrl) {
      setLoading(true);
    }
  }, []);
  useEffect(() => {
    const handle = setInterval(() => {
      setTabTitleFromIFrame();
    }, 10000);
    return () => clearTimeout(handle);
  }, []);

  useEffect(() => {
    if (refIFrame.current?.contentDocument) {
      console.log("Mutation observer will observe.");
      const mo = new MutationObserver(() => {
        console.log("Detected tree change");
        if (refIFrame.current?.contentDocument?.title) {
          console.log("XXXXX", refIFrame.current.contentDocument.title);
        }
        setTabTitleFromIFrame();
      });
      mo.observe(refIFrame.current?.contentDocument?.querySelector("head")!, {
        subtree: true,
        characterData: true,
        childList: true,
      });
      return () => mo.disconnect();
    }
  });
  const setTabTitleFromIFrame = useMemo(
    () => () => {
      if (refIFrame.current?.contentDocument?.title) {
        ((openedScreen as unknown) as IWebScreen).setTitle(refIFrame.current.contentDocument.title);
      }
    },
    []
  );
  return (
    <Screen isHidden={!getIsTopmostNonDialogScreen(openedScreen)}>
      <WebScreen
        url={openedScreen.screenUrl || ""}
        isLoading={isLoading}
        onLoad={(event: any) => {
          event.persist();
          setTabTitleFromIFrame();
          setLoading(false);
        }}
        refIFrame={(elm: any) => {
          refIFrame.current = elm;
          ((openedScreen as unknown) as IWebScreen).setReloader(
            elm
              ? {
                  reload: () => {
                    setLoading(true);
                    elm.contentWindow.location.reload();
                  },
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
    return !formScreen.isLoading ? (
      <Provider key={formScreen.formScreen!.screenUI.$iid} formScreen={formScreen}>
        <ErrorBoundaryEncapsulated ctx={openedScreen}>
          <CScreenInner openedScreen={openedScreen} formScreen={formScreen} />
        </ErrorBoundaryEncapsulated>
      </Provider>
    ) : null;
  }
}

@observer
class CScreenInner extends React.Component<{
  openedScreen: IOpenedScreen;
  formScreen: IFormScreenEnvelope;
}> {
  render() {
    const { openedScreen, formScreen } = this.props;
    return (
      <Screen isHidden={!getIsTopmostNonDialogScreen(openedScreen)}>
        <CtxPanelVisibility.Provider
          value={{ isVisible: getIsTopmostNonDialogScreen(openedScreen) }}
        >
          <FormScreenBuilder xmlWindowObject={formScreen.formScreen!.screenUI} />
        </CtxPanelVisibility.Provider>
      </Screen>
    );
  }
}
