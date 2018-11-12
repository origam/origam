import * as React from "react";
import axios from "axios";
import * as xmlJs from "xml-js";
import { observable, action, autorun, computed } from "mobx";
import { observer, inject, Provider } from "mobx-react";
import { MainViewEngine } from "src/MainTabs/MainViewEngine";
import { MainMenuEngine } from "./MainMenuEngine";

function processNode(node: any, context: any) {
  switch (node.name) {
    case "Menu": {
      const newNode = {
        type: "Menu",
        props: {
          label: node.attributes.label,
          icon: node.attributes.icon
        },
        children: []
      };
      context.uiNode.children.push(newNode);
      for (const element of node.elements) {
        processNode(element, { ...context, uiNode: newNode });
      }
      return;
    }
    case "Command": {
      switch (node.attributes.type) {
        case "FormReferenceMenuItem": {
          const newNode = {
            type: "MenuItemForm",
            props: {
              id: node.attributes.id,
              label: node.attributes.label,
              icon: node.attributes.icon
            },
            children: []
          };
          context.uiNode.children.push(newNode);
          return;
        }
        case "WorkflowReferenceMenuItem": {
          const newNode = {
            type: "MenuItemWorkflow",
            props: {
              id: node.attributes.id,
              label: node.attributes.label,
              icon: node.attributes.icon
            },
            children: []
          };
          context.uiNode.children.push(newNode);
          return;
        }
      }
      return;
    }
    case "Submenu": {
      const newNode = {
        type: "Submenu",
        props: {
          id: node.attributes.id,
          label: node.attributes.label,
          icon: node.attributes.icon
        },
        children: []
      };
      context.uiNode.children.push(newNode);
      for (const element of node.elements) {
        processNode(element, { ...context, uiNode: newNode });
      }
      return;
    }
  }
  console.log(node);
  throw new Error("Unknown menu structure.");
}

function buildMenu(menuDef: any) {
  const context = {
    uiNode: {
      children: []
    }
  };
  processNode(menuDef.elements[0], context);
  return context.uiNode.children[0];
}

const Menu = (props: any) => (
  <div className="oui-main-menu">
    <div className={"main-menu-item section"}>{props.children}</div>
  </div>
);

@inject("mainMenuEngine")
@observer
class Submenu extends React.Component<any> {
  public render() {
    const isExpanded = this.props.mainMenuEngine!.isSectionExpanded(
      this.props.id
    );
    return (
      <>
        <CndMenuFolderItem {...this.props as any} />
        <div
          className={
            "main-menu-item section" + (!isExpanded ? " collapsed" : "")
          }
        >
          {this.props.children}
        </div>
      </>
    );
  }
}

const MenuItemWorkflow = (props: any) => <CndMenuItem {...props} />;

const MenuItemForm = (props: any) => <CndMenuItem {...props} />;

const REACT_TYPES = {
  Menu,
  Submenu,
  MenuItemWorkflow,
  MenuItemForm
};

function buildReactTree(node: any) {
  return React.createElement(
    REACT_TYPES[node.type],
    node.props,
    ...node.children.map((child: any) => buildReactTree(child))
  );
}

export function interpretMenu(xmlObj: any) {
  const menu = buildMenu(xmlObj);
  const reactMenu = buildReactTree(menu);
  return reactMenu;
}

interface IMainMenuProps {
  children?: React.ReactNode;
  mainViewEngine: MainViewEngine;
}

@observer
export class MainMenu extends React.Component<IMainMenuProps> {
  constructor(props: IMainMenuProps) {
    super(props);
    this.mainMenuEngine = new MainMenuEngine();
  }

  public mainMenuEngine: MainMenuEngine;

  public render() {
    const { reactMenu } = this.props.mainViewEngine;
    return (
      <Provider
        mainMenuEngine={this.mainMenuEngine}
        mainViewEngine={this.props.mainViewEngine}
      >
        {reactMenu || <></>}
      </Provider>
    );
  }
}

export enum IMenuItemStatus {
  NONE,
  OPENED,
  CLOSED
}

export interface ICndMenuItemProps {
  label: string;
  icon: string;
  id: string;
  mainMenuEngine?: MainMenuEngine;
  mainViewEngine?: MainViewEngine;
}

export interface IMenuItemProps extends ICndMenuItemProps {
  status: IMenuItemStatus;
  isActive?: boolean;
  children?: React.ReactNode;
  onClick?: (event: any) => void;
}

export const MenuItem = ({
  label,
  icon,
  status,
  isActive,
  onClick
}: IMenuItemProps) => {
  const iconNode = {
    "menu_form.png": <i className="fa fa-file-text" />,
    "menu_folder.png": <i className="fa fa-folder" />,
    "menu_workflow.png": <i className="fa fa-magic" />
  }[icon];
  const statusNode = {
    [IMenuItemStatus.NONE]: null,
    [IMenuItemStatus.OPENED]: (
      <div className="status">
        <i className="fa fa-caret-down" />
      </div>
    ),
    [IMenuItemStatus.CLOSED]: (
      <div className="status">
        <i className="fa fa-caret-right" />
      </div>
    )
  }[status];
  return (
    <div
      className={"main-menu-item item" + (isActive ? " active" : "")}
      onClick={onClick}
    >
      {iconNode}
      {label}
      {statusNode}
    </div>
  );
};

@inject("mainViewEngine")
@observer
export class CndMenuItem extends React.Component<ICndMenuItemProps> {
  @action.bound
  public handleClick(event: any) {
    console.log("click");
    this.props.mainViewEngine!.handleMenuFormItemClick(event, this.props.id, this.props.label);
  }

  @computed
  public get isActive() {
    return (
      this.props.mainViewEngine &&
      this.props.mainViewEngine.activeView &&
      this.props.id === this.props.mainViewEngine.activeView.id
    );
  }

  public render() {
    return (
      <MenuItem
        {...this.props}
        status={IMenuItemStatus.NONE}
        isActive={this.isActive}
        onClick={this.handleClick}
      />
    );
  }
}

@inject("mainMenuEngine")
@observer
export class CndMenuFolderItem extends React.Component<ICndMenuItemProps> {
  public render() {
    return (
      <MenuItem
        {...this.props}
        status={
          this.props.mainMenuEngine!.isSectionExpanded(this.props.id)
            ? IMenuItemStatus.OPENED
            : IMenuItemStatus.CLOSED
        }
        isActive={false}
        onClick={(event: any) =>
          this.props.mainMenuEngine!.handleSectionClick(event, this.props.id)
        }
      />
    );
  }
}
