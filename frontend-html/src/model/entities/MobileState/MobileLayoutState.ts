import { T } from "utils/translation";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { getActiveScreen } from "model/selectors/getActiveScreen";

export interface IMobileLayoutState {
  actionDropUpHidden: boolean;
  refreshButtonHidden: boolean;
  saveButtonHidden: boolean;
  showOpenTabCombo: boolean;
  showSearchButton: boolean;
  showHamburgerMenuButton: boolean;
  heading: string;

  showCloseButton(someScreensAreOpen: boolean): boolean;

  hamburgerClick(): IMobileLayoutState;

  close(ctx: any): Promise<IMobileLayoutState>;
}

export class MenuLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpenTabCombo = false;
  showSearchButton = true;
  showHamburgerMenuButton = false;
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
  showHamburgerMenuButton = true;
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
  showHamburgerMenuButton = true;
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

export class ComboEditLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpenTabCombo = false;
  showSearchButton = false;
  showHamburgerMenuButton = false;
  heading = "";

  constructor(public component: React.ReactNode) {
  }

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

export class ScreenLayoutState implements IMobileLayoutState {
  actionDropUpHidden = false;
  refreshButtonHidden = false;
  saveButtonHidden = false;
  showOpenTabCombo = true;
  showSearchButton = true;
  showHamburgerMenuButton = true;
  heading = "";

  showCloseButton(someScreensAreOpen: boolean) {
    return true;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    const activeScreen = getActiveScreen(ctx);
    if (activeScreen) {
      await onScreenTabCloseClick(activeScreen)(null);
      const stillOpenScreens = getOpenedNonDialogScreenItems(ctx);
      if (stillOpenScreens.length === 0) {
        return new MenuLayoutState();
      }
    }
    return this;
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}