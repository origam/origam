import { T } from "utils/translation";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import React from "react";

export interface IMobileLayoutState {
  actionDropUpHidden: boolean;
  refreshButtonHidden: boolean;
  saveButtonHidden: boolean;
  showOpenTabCombo: boolean;
  showSearchButton: boolean;
  topLeftComponent: TopLeftComponent;
  heading: string;
  showOkButton: boolean;
  showBackButton: boolean;

  showCloseButton(someScreensAreOpen: boolean): boolean;

  hamburgerClick(): IMobileLayoutState;

  close(ctx: any): Promise<IMobileLayoutState>;
}

export enum TopLeftComponent {
  Menu,
  Close,
  None
}

export class MenuLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpenTabCombo = false;
  showSearchButton = true;
  topLeftComponent = TopLeftComponent.Close;
  showOkButton = false;
  showBackButton = false;
  heading = T("Menu", "menu");

  showCloseButton(someScreensAreOpen: boolean) {
    return someScreensAreOpen;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new ScreenLayoutState();
  }
}

export class AboutLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpenTabCombo = false;
  showSearchButton = true;
  topLeftComponent = TopLeftComponent.Menu;
  showOkButton = false;
  showBackButton = false;
  heading = T("About", "about_application");

  showCloseButton(someScreensAreOpen: boolean) {
    return true;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}

export class SearchLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpenTabCombo = false;
  showSearchButton = false;
  topLeftComponent = TopLeftComponent.Menu;
  showOkButton = false;
  showBackButton = false;
  heading = T("Search", "mobile_search_title");

  showCloseButton(someScreensAreOpen: boolean) {
    return true;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}

export class EditLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpenTabCombo = false;
  showSearchButton = false;
  topLeftComponent = TopLeftComponent.None;
  showOkButton = true;
  showBackButton = false;
  private readonly showCloseButton_: boolean;

  constructor(
    public component: React.ReactNode,
    public heading: string,
    public layoutAfterClose?: IMobileLayoutState,
    showOkButton?: boolean,
    showCloseButton = false)
  {
    this.showOkButton = showOkButton ?? true;
    this.showCloseButton_ = showCloseButton;
  }

  showCloseButton(someScreensAreOpen: boolean) {
    return this.showCloseButton_;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    return this.layoutAfterClose ?? new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}

export class ScreenLayoutState implements IMobileLayoutState {
  actionDropUpHidden = false;
  refreshButtonHidden = false;
  saveButtonHidden = false;
  showOpenTabCombo = true;
  showSearchButton = true;
  topLeftComponent = TopLeftComponent.Menu;
  showOkButton = false;
  showBackButton = true;
  heading = "";

  showCloseButton(someScreensAreOpen: boolean) {
    return true;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    const activeScreen = getActiveScreen(ctx);
    if (activeScreen) {
      const isClosing = await onScreenTabCloseClick(activeScreen)(null);
      const stillOpenScreens = getOpenedNonDialogScreenItems(ctx);
      if (stillOpenScreens.length === 0 && isClosing) {
        return new MenuLayoutState();
      }
    }
    return this;
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}