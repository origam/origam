import * as React from "react";
import axios from "axios";
import * as xmlJs from "xml-js";
import { observable, action, autorun, computed } from "mobx";
import { observer, inject, Provider } from "mobx-react";
import { MainMenu } from "./MainMenu";
import { IMainViews } from "src/Application/types";
import { IMainMenu } from "./types";



export const Menu = (props: any) => (
  <div className="oui-main-menu">
    <div className={"main-menu-item section"}>{props.children}</div>
  </div>
);

@inject("mainMenuEngine")
@observer
export class Submenu extends React.Component<any> {
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

export const MenuItemWorkflow = (props: any) => <CndMenuItem {...props} />;

export const MenuItemForm = (props: any) => <CndMenuItem {...props} />;




interface IMainMenuProps {
  mainMenu?: IMainMenu;
}

@inject("mainMenu")
@observer
export class MainMenuComponent extends React.Component<IMainMenuProps> {
  constructor(props: IMainMenuProps) {
    super(props);
  }

  public render() {
    const { reactMenu } = this.props.mainMenu!;
    return reactMenu || null;
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
  mainMenu?: IMainMenu;
  mainViews?: IMainViews;
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

@inject("mainViews")
@observer
export class CndMenuItem extends React.Component<ICndMenuItemProps> {
  @action.bound
  public handleClick(event: any) {
    this.props.mainViews!.handleMenuFormItemClick(
      event,
      this.props.id,
      this.props.label
    );
  }

  @computed
  public get isActive() {
    return (
      this.props.mainViews! &&
      this.props.mainViews!.activeView &&
      this.props.id === this.props.mainViews!.activeView!.id
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

@inject("mainMenu")
@observer
export class CndMenuFolderItem extends React.Component<ICndMenuItemProps> {
  public render() {
    return (
      <MenuItem
        {...this.props}
        status={
          this.props.mainMenu!.isSectionExpanded(this.props.id)
            ? IMenuItemStatus.OPENED
            : IMenuItemStatus.CLOSED
        }
        isActive={false}
        onClick={(event: any) =>
          this.props.mainMenu!.handleSectionClick(event, this.props.id)
        }
      />
    );
  }
}
