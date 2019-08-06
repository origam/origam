import S from "./MainMenu.module.css";
import React from "react";
import { observer, inject } from "mobx-react";
import { observable, action } from "mobx";

export enum IMenuItemIcon {
  Form = "menu_form.png",
  Folder = "menu_folder.png",
  Workflow = "menu_workflow.png",
  Parameter = "menu_parameter.png"
}



export enum IMenuItemStatus {
  Opened,
  Closed,
  None
}

@observer
export class MainMenuItem extends React.Component<{
  icon: IMenuItemIcon;
  label: string;
  status: IMenuItemStatus;
  active: boolean;
  level: number;
  isHidden: boolean;
  onClick?: (event: any) => void;
}> {
  render() {
    const {
      icon,
      label,
      status,
      active,
      onClick,
      isHidden,
      level
    } = this.props;
    return (
      <div
        className={
          S.mainMenuItem +
          ` level${level}` +
          (active ? " active" : "") +
          (isHidden ? " hidden" : "")
        }
        onClick={onClick}
      >
        {icon === IMenuItemIcon.Folder && <i className="fa fa-folder icon" />}
        {icon === IMenuItemIcon.Form && <i className="fas fa-file-alt icon" />}
        {icon === IMenuItemIcon.Workflow && <i className="fa fa-magic icon" />}
        {icon === IMenuItemIcon.Parameter && (
          <i className="fas fa-asterisk icon" />
        )}
        {label}
        {status !== IMenuItemStatus.None && (
          <div className="status">
            {status === IMenuItemStatus.Closed && (
              <i className="fa fa-caret-right icon" />
            )}
            {status === IMenuItemStatus.Opened && (
              <i className="fa fa-caret-down icon" />
            )}
          </div>
        )}
      </div>
    );
  }
}

@observer
export class MainMenuSection extends React.Component<{
  label: string;
  level: number;
  isHidden: boolean;
}> {
  @observable isOpened = false;

  @action.bound toggleOpened() {
    this.isOpened = !this.isOpened;
  }

  render() {
    return (
      <React.Fragment key={undefined}>
        <MainMenuItem
          level={this.props.level}
          icon={IMenuItemIcon.Folder}
          label={this.props.label}
          status={
            this.isOpened ? IMenuItemStatus.Opened : IMenuItemStatus.Closed
          }
          active={false}
          isHidden={this.props.isHidden}
          onClick={this.toggleOpened}
        />
        <div
          className={
            S.mainMenuSection +
            (this.isOpened ? "" : " collapsed") +
            (this.props.isHidden ? " hidden" : "")
          }
        >
          {this.props.children}
        </div>
      </React.Fragment>
    );
  }
}

export class MainMenuRecursiveItem extends React.Component<{
  node: any;
  level: number;
  onItemClick?: (event: any, item: any) => void;
}> {
  render() {
    switch (this.props.node.name) {
      case "Submenu":
        return (
          <MainMenuSection
            label={this.props.node.attributes.label}
            isHidden={this.props.node.attributes.isHidden === "true"}
            level={this.props.level}
          >
            {this.props.node.elements.map((child: any, idx: number) => (
              <MainMenuRecursiveItem
                key={idx}
                node={child}
                level={this.props.level + 1}
                onItemClick={this.props.onItemClick}
              />
            ))}
          </MainMenuSection>
        );
      case "Command":
        return (
          <MainMenuItem
            level={this.props.level}
            icon={this.props.node.attributes.icon}
            label={this.props.node.attributes.label}
            active={false}
            isHidden={this.props.node.attributes.isHidden === "true"}
            status={IMenuItemStatus.None}
            onClick={(event: any) =>
              this.props.onItemClick &&
              this.props.onItemClick(event, this.props.node)
            }
          />
        );
      default:
        return null;
    }
  }
}

@observer
export class MainMenu extends React.Component<{
  menuUI: any;
  onItemClick?: (event: any, item: any) => void;
}> {
  render() {
    return (
      <div className={S.mainMenu}>
        <div className={S.mainMenuSectionItem}>
          {this.props.menuUI.elements.map((item: any, idx: number) => (
            <MainMenuRecursiveItem
              key={idx}
              node={item}
              level={0}
              onItemClick={this.props.onItemClick}
            />
          ))}
        </div>
      </div>
    );
  }
}


