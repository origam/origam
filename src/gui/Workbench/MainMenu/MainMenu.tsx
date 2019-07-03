import S from "./MainMenu.module.css";
import React from "react";
import { observer } from "mobx-react";
import { observable, action } from "mobx";

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
  @observable isOpened = true;

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

export class MainMenu extends React.Component {
  render() {
    return (
      <div className={S.mainMenu}>
        <div className={S.mainMenuSectionItem}>
          <MainMenuSection label="Folder 1" isHidden={false} level={0}>
            <MainMenuItem
              level={1}
              icon={IMenuItemIcon.Form}
              label="Form 1"
              active={false}
              isHidden={false}
              status={IMenuItemStatus.None}
            />
            <MainMenuItem
              level={1}
              icon={IMenuItemIcon.Form}
              label="Form 1"
              active={true}
              isHidden={false}
              status={IMenuItemStatus.None}
            />
            <MainMenuItem
              level={1}
              icon={IMenuItemIcon.Workflow}
              label="Form 1"
              active={false}
              isHidden={false}
              status={IMenuItemStatus.None}
            />
            <MainMenuItem
              level={1}
              icon={IMenuItemIcon.Form}
              label="Form 1"
              active={false}
              isHidden={false}
              status={IMenuItemStatus.None}
            />
          </MainMenuSection>
        </div>
      </div>
    );
  }
}
