import { ML } from "../utils/types";
import { observable, action } from "mobx";
import axios from "axios";
import xmlJs from "xml-js";
import { findStopping, parseBoolean } from "../utils/xml";
import { AOnItemClick } from "./AOnItemClick";
import { IMainMenu, IAOnItemClick } from "./types";
import { findMenu, recursiveBuildMenuItems } from "./factory";
import { unpack } from "../utils/objects";

/* export class Menu {
  constructor(public P: {}) {}
}*/

export enum IMenuItemIcon {
  Form = "menu_form.png",
  Folder = "menu_folder.png",
  Workflow = "menu_workflow.png",
  Parameter = "menu_parameter.png"
}

export enum ICommandType {
  FormRef = "FormReferenceMenuItem",
  FormRefWithSelection = "FormReferenceMenuItem_WithSelection",
  WorkflowRef = "WorkflowReferenceMenuItem"
}

export class Submenu {
  type: "Submenu" = "Submenu";
  constructor(
    public P: {
      id: string;
      icon: IMenuItemIcon;
      label: string;
      isHidden: boolean;
      children: Array<Submenu | Command>;
    }
  ) {}

  @observable isOpened = false;

  get id() {
    return this.P.id;
  }

  get icon() {
    return this.P.icon;
  }

  get label() {
    return this.P.label;
  }

  get isHidden() {
    return this.P.isHidden;
  }

  get children() {
    return this.P.children;
  }
}

export class Command {
  type: "Command" = "Command";
  constructor(
    public P: {
      id: string;
      icon: IMenuItemIcon;
      label: string;
      showInfoPanel: boolean;
      commandType: ICommandType;
      aOnItemClick: ML<IAOnItemClick>;
      // parent: L<Submenu | undefined>;
    }
  ) {}

  @action.bound
  handleOnClick(event: any) {
    unpack(this.P.aOnItemClick).do(
      event,
      this.id,
      this.commandType,
      this.label
    );
  }

  get id() {
    return this.P.id;
  }

  get icon() {
    return this.P.icon;
  }

  get label() {
    return this.P.label;
  }

  get showInfopanel() {
    return this.P.showInfoPanel;
  }

  get commandType() {
    return this.P.commandType;
  }
}

export class MainMenu implements IMainMenu {
  constructor(public P: {}) {}

  @action.bound setItems(items: any) {
    this.items = items;
  }

  @action.bound resetItems() {
    this.items.length = 0;
  }

  @observable items: any = [];
}
