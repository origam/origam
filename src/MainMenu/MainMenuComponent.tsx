import * as React from "react";
import axios from "axios";
import * as xmlJs from "xml-js";
import { observable, action, autorun } from "mobx";
import { observer, inject, Provider } from "mobx-react";

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
    <CndMenuSection id={"0"} isExpanded={true}>
      {props.children}
    </CndMenuSection>
  </div>
);

const Submenu = (props: any) => (
  <>
    <CndMenuFolderItem id={props.id} label={props.label} icon={props.icon} />
    <CndMenuSection id={props.id}>{props.children}</CndMenuSection>
  </>
);

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

class MainMenuEngine {
  @observable public expandedSections = new Map();

  @action.bound
  public expandSection(id: string) {
    this.expandedSections.set(id, true);
  }

  @action.bound
  public collapseSection(id: string) {
    this.expandedSections.delete(id);
  }

  public isSectionExpanded(id: string): boolean {
    return this.expandedSections.has(id);
  }

  @action.bound
  public handleSectionClick(event: any, id: string) {
    if (this.isSectionExpanded(id)) {
      this.collapseSection(id);
    } else {
      this.expandSection(id);
    }
    console.log(Array.from(this.expandedSections));
  }
}

interface IMainMenuProps {
  children?: React.ReactNode;
}

@observer
export class MainMenu extends React.Component {
  constructor(props: IMainMenuProps) {
    super(props);
    this.mainMenuEngine = new MainMenuEngine();
  }

  @observable.ref public reactMenu: React.ReactNode = null;

  public async componentDidMount() {
    const xml = (await axios.get("/menu01.xml")).data;
    const xmlObj = xmlJs.xml2js(xml, { compact: false });
    const reactMenu = interpretMenu(xmlObj);
    this.reactMenu = reactMenu;
  }

  public mainMenuEngine: MainMenuEngine;

  public render() {
    return (
      <Provider mainMenuEngine={this.mainMenuEngine}>
        {this.reactMenu || <div />}
        {/*<div className="oui-main-menu">
          <CndMenuSection id="0">
            <CndMenuFolderItem id="0" label="Folder 1" icon="menu_folder.png" />
            <CndMenuSection id="1">
              <CndMenuFolderItem
                id="1"
                label="Folder 11"
                icon="menu_folder.png"
              />
              <CndMenuSection id="2">
                <CndMenuItem id="7" label="Form 111" icon="menu_form.png" />
                <CndMenuItem
                  id="8"
                  label="Workflow 112"
                  icon="menu_workflow.png"
                />
              </CndMenuSection>
            </CndMenuSection>
          </CndMenuSection>
          <CndMenuSection id="3">
            <CndMenuFolderItem id="3" label="Folder 2" icon="menu_folder.png" />
          </CndMenuSection>
          <CndMenuSection id="4">
            <CndMenuFolderItem id="4" label="Folder 3" icon="menu_folder.png" />
            <CndMenuSection id="11">
              <CndMenuFolderItem
                id="11"
                label="Folder 2"
                icon="menu_folder.png"
              />
            </CndMenuSection>
          </CndMenuSection>
      </div>*/}
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

@observer
export class CndMenuItem extends React.Component<ICndMenuItemProps> {
  public render() {
    return (
      <MenuItem
        {...this.props}
        status={IMenuItemStatus.NONE}
        isActive={false}
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

interface ICndMenuSectionProps {
  children?: React.ReactNode;
  id: string;
  mainMenuEngine?: MainMenuEngine;
  isExpanded?: boolean;
}

interface IMenuSectionProps extends ICndMenuSectionProps {
  isExpanded?: boolean;
}

const MenuSection = ({ isExpanded, children }: IMenuSectionProps) => {
  return (
    <div
      className={"main-menu-item section" + (!isExpanded ? " collapsed" : "")}
    >
      {children}
    </div>
  );
};

@inject("mainMenuEngine")
@observer
export class CndMenuSection extends React.Component<ICndMenuSectionProps> {
  public render() {
    const mainMenuEngine = this.props.mainMenuEngine!;
    const isExpanded = mainMenuEngine.isSectionExpanded(this.props.id);
    return (
      <MenuSection
        {...this.props}
        isExpanded={this.props.isExpanded || isExpanded}
      />
    );
  }
}
