/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { FormScreenBuilder } from "gui/Workbench/ScreenArea/FormScreenBuilder/FormScreenBuilder";
import { observer, Provider } from "mobx-react";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import React, { useEffect, useMemo, useRef, useState } from "react";
import { Screen } from "gui/Components/Screen/Screen";
import { CtxPanelVisibility } from "gui/contexts/GUIContexts";
import { WebScreen } from "gui/Components/WebScreen/WebScreen";
import { IWebScreen } from "model/entities/types/IWebScreen";
import { getIsTopmostNonDialogScreen } from "model/selectors/getIsTopmostNonDialogScreen";
import { ErrorBoundaryEncapsulated } from "gui/Components/Utilities/ErrorBoundary";
import { IFormScreenEnvelope } from "model/entities/types/IFormScreen";
import { onIFrameClick } from "model/actions/WebScreen/onIFrameClick";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";

const WebScreenComposite: React.FC<{ openedScreen: IOpenedScreen }> = observer((props) => {
  const {openedScreen} = props;
  const [isLoading, setLoading] = useState(false);
  const refIFrame = useRef<any>(null);

  const setTabTitleFromIFrame = useMemo(
    () => () => {
      const frameWindow = refIFrame.current;
      const contentDocument = frameWindow?.contentDocument;
      if (contentDocument?.title) {
        ((openedScreen as unknown) as IWebScreen).setTitle(contentDocument.title);
      }
    },
    [openedScreen]
  );

  useEffect(() => {
    if (openedScreen.screenUrl) {
      setLoading(true);
    }
  }, []); // eslint-disable-line react-hooks/exhaustive-deps
  useEffect(() => {
    const handle = setInterval(() => {
      setTabTitleFromIFrame();
    }, 10000);
    return () => clearTimeout(handle);
  }, [setTabTitleFromIFrame]);

  useEffect(()=> {
    const frameWindow = refIFrame.current as HTMLIFrameElement;
    const contentDocument = frameWindow?.contentDocument;
    const headNode = contentDocument?.querySelector("head");

    if (contentDocument && headNode) {
      const mo = new MutationObserver(() => {
        setTabTitleFromIFrame();
      });
      mo.observe(headNode, {
        subtree: true,
        characterData: true,
        childList: true,
      });
      return () => mo.disconnect();
    }
  });

  const initIFrame = useMemo(
    () => () => {
      const frameWindow = refIFrame.current;
      const contentDocument = frameWindow?.contentDocument;

      if (contentDocument) {
        contentDocument.closeOrigamTab = (() => {
          onScreenTabCloseClick(openedScreen)(undefined);
        });
        contentDocument.addEventListener(
          "click",
          (event: any) => {
            onIFrameClick(openedScreen)(event);
          },
          true
        );
      }
    },
    [openedScreen]
  );
  return (
    <Screen isHidden={!getIsTopmostNonDialogScreen(openedScreen)}>
      <WebScreen
        source={openedScreen.screenUrl || ""}
        isLoading={isLoading}
        onLoad={(event: any) => {
          event.persist();
          setTabTitleFromIFrame();
          initIFrame();
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
    const {openedScreen} = this.props;
    if (openedScreen.screenUrl) {
      return (
        <ErrorBoundaryEncapsulated ctx={openedScreen}>
          <WebScreenComposite openedScreen={openedScreen}/>
        </ErrorBoundaryEncapsulated>
      );
    }
    if (!openedScreen.content) return null;
    const formScreen = openedScreen.content;
    return !formScreen.isLoading ? (
      <Provider key={formScreen.formScreen!.screenUI.$iid} formScreen={formScreen}>
        <ErrorBoundaryEncapsulated ctx={openedScreen}>
          <CScreenInner openedScreen={openedScreen} formScreen={formScreen}/>
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
    const {openedScreen, formScreen} = this.props;
    return (
      <Screen isHidden={!getIsTopmostNonDialogScreen(openedScreen)}>
        <CtxPanelVisibility.Provider
          value={{isVisible: getIsTopmostNonDialogScreen(openedScreen)}}
        >
          <FormScreenBuilder
            title={formScreen.formScreen!.title}
            xmlWindowObject={formScreen.formScreen!.screenUI}
          />
        </CtxPanelVisibility.Provider>
      </Screen>
    );
  }
}
