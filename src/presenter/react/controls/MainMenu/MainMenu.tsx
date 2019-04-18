import React from "react";
import { observer } from "mobx-react";
import {
  Command,
  Submenu,
  IMenuItemIcon
} from "../../../../MainMenu/MainMenu";
import { observable, action } from "mobx";

enum IMenuItemStatus {
  Opened,
  Closed,
  None
}

@observer
export class MenuItem extends React.Component<{
  icon: IMenuItemIcon;
  label: string;
  status: IMenuItemStatus;
  active: boolean;
  isHidden: boolean;
  onClick?: (event: any) => void;
}> {
  render() {
    const { icon, label, status, active, onClick, isHidden } = this.props;
    return (
      <div
        className={
          "main-menu-item item" +
          (active ? " active" : "") +
          (isHidden ? " hidden" : "")
        }
        onClick={onClick}
      >
        {icon === IMenuItemIcon.Folder && <i className="fa fa-folder icon" />}
        {icon === IMenuItemIcon.Form && <i className="fas fa-file-alt icon" />}
        {icon === IMenuItemIcon.Workflow && <i className="fa fa-magic icon" />}
        {icon === IMenuItemIcon.Parameter && <i className="fas fa-asterisk icon" />}
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

function getItems(item: Submenu | Command) {
  switch (item.type) {
    case "Submenu":
      return <MainMenuSection key={item.id} item={item} />;
    case "Command":
      return (
        <MenuItem
          key={item.id}
          icon={item.icon}
          label={item.label}
          status={IMenuItemStatus.None}
          isHidden={false}
          active={false}
          onClick={item.handleOnClick}
        />
      );
  }
}

@observer
export class MainMenuSection extends React.Component<{
  item: Submenu;
}> {
  @observable isOpened = false;

  @action.bound toggleOpened() {
    this.isOpened = !this.isOpened;
  }

  render() {
    const { item } = this.props;
    return (
      <React.Fragment key={item.id}>
        <MenuItem
          icon={item.icon}
          label={item.label}
          status={
            this.isOpened ? IMenuItemStatus.Opened : IMenuItemStatus.Closed
          }
          active={false}
          isHidden={item.isHidden}
          onClick={this.toggleOpened}
        />
        <div
          className={
            "main-menu-item section" +
            (this.isOpened ? "" : " collapsed") +
            (item.isHidden ? " hidden" : "")
          }
        >
          {item.children.map(itemCh => getItems(itemCh))}
        </div>
      </React.Fragment>
    );
  }
}

@observer
export class MainMenu extends React.Component<{
  items: Array<Command | Submenu>;
}> {
  render() {
    const { items } = this.props;
    return (
      <div className="oui-main-menu">
        <div className="main-menu-item section">
          {items.map(item => getItems(item))}
        </div>
      </div>
    );
  }
}
