import { T } from "utils/translation";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import React from "react";
import { getMobileState } from "model/selectors/getMobileState";

export interface IMobileLayoutState {
  actionDropUpHidden: boolean;
  refreshButtonHidden: boolean;
  saveButtonHidden: boolean;
  showSearchButton: boolean;
  topLeftComponent: TopLeftComponent;
  showOkButton: boolean;
  showBackButton: boolean;

  getTopComponentState(ctx: any): ITopComponentState;

  showCloseButton(someScreensAreOpen: boolean): boolean;

  hamburgerClick(): IMobileLayoutState;

  close(ctx: any): Promise<IMobileLayoutState>;
}

export interface ITopComponentState {
  heading: string;
  topMiddleComponent: TopCenterComponent
}

export enum TopLeftComponent {
  Menu,
  Close,
  None
}

export enum TopCenterComponent {
  OpenTabCombo,
  MenuEditButton,
  Heading
}

export class MenuLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showSearchButton = true;
  topLeftComponent = TopLeftComponent.Close;
  showOkButton = false;
  showBackButton = false;

  showCloseButton(someScreensAreOpen: boolean) {
    return someScreensAreOpen;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new ScreenLayoutState();
  }

  getTopComponentState(ctx: any): ITopComponentState {
    const mobileState = getMobileState(ctx);
    const activeSection = mobileState.sidebarState.activeSection;
    switch (activeSection) {
      case "Menu":
        return {
          heading: T("Menu", "menu"),
          topMiddleComponent: TopCenterComponent.MenuEditButton
        };
      case "Favorites":
        return {
          heading: "Favorites",
          topMiddleComponent: TopCenterComponent.MenuEditButton
        };
      case "WorkQueues":
        return {
          heading: "WorkQueues",
          topMiddleComponent: TopCenterComponent.Heading
        };
      case "Search":
        return {
          heading: "Search",
          topMiddleComponent: TopCenterComponent.Heading
        };
      case "Chat":
        return {
          heading: "Chat",
          topMiddleComponent: TopCenterComponent.Heading
        };
      default:
        throw Error(activeSection + " not implemented ");
    }
  }
}

export class AboutLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showSearchButton = true;
  topLeftComponent = TopLeftComponent.Menu;
  showOkButton = false;
  showBackButton = false;

  showCloseButton(someScreensAreOpen: boolean) {
    return true;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }

  getTopComponentState(ctx: any): ITopComponentState {
    return {
      heading: T("About", "about_application"),
      topMiddleComponent: TopCenterComponent.Heading
    };
  }
}

export class SearchLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showSearchButton = false;
  topLeftComponent = TopLeftComponent.Menu;
  showOkButton = false;
  showBackButton = false;

  showCloseButton(someScreensAreOpen: boolean) {
    return true;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }

  getTopComponentState(ctx: any): ITopComponentState {
    return {
      heading: T("Search", "mobile_search_title"),
      topMiddleComponent: TopCenterComponent.Heading
    };
  }
}

export class EditLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showSearchButton = false;
  topLeftComponent = TopLeftComponent.None;
  showOkButton = true;
  showBackButton = false;
  private readonly showCloseButton_: boolean;

  constructor(
    public component: React.ReactNode,
    private heading: string,
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

  getTopComponentState(ctx: any): ITopComponentState {
    return {
      heading: this.heading,
      topMiddleComponent: TopCenterComponent.Heading
    };
  }
}

export class ScreenLayoutState implements IMobileLayoutState {
  actionDropUpHidden = false;
  refreshButtonHidden = false;
  saveButtonHidden = false;
  showSearchButton = true;
  topLeftComponent = TopLeftComponent.Menu;
  showOkButton = false;
  showBackButton = true;

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

  getTopComponentState(ctx: any): ITopComponentState {
    return {
      heading: "",
      topMiddleComponent: TopCenterComponent.OpenTabCombo
    };
  }
}