import * as React from "react";
import axios from "axios";
import * as xmlJs from "xml-js";
import { observable, action, autorun } from "mobx";
import { observer, inject, Provider } from "mobx-react";

class MainMenuEngine {
  @observable public expandedSections = new Map([["0", true], ["4", true]]);

  constructor() {
    autorun(() => {
      console.log(Array.from(this.expandedSections.entries()));
    });
  }

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
    console.log("click", id);
    if (this.isSectionExpanded(id)) {
      console.log("collapsing");
      this.collapseSection(id);
    } else {
      console.log("expanding");
      this.expandSection(id);
    }
    console.log(Array.from(this.expandedSections));
  }
}

interface IMainMenuProps {
  children?: React.ReactNode;
}

export class MainMenu extends React.Component {
  constructor(props: IMainMenuProps) {
    super(props);
    this.mainMenuEngine = new MainMenuEngine();
  }

  public async componentDidMount() {
    const xml = (await axios.get("/menu01.xml")).data;
    const xmlObj = xmlJs.xml2js(xml, { compact: false });
    console.log(xmlObj);
  }

  public mainMenuEngine: MainMenuEngine;

  public render() {
    return (
      <Provider mainMenuEngine={this.mainMenuEngine}>
        <div className="oui-main-menu">
          <CndMenuSection id="0">
            <CndMenuFolderItem id="0" label="Folder 1" icon="menu_folder.png" />
            <CndMenuSection id="1">
              <CndMenuFolderItem
                id="1"
                label="Folder 11"
                icon="menu_folder.png"
              />
              <CndMenuSection id="2">
                <CndMenuItem
                  id="7"
                  label="Form 111"
                  icon="menu_form.png"
                />
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
        </div>
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
    return <MenuSection {...this.props} isExpanded={isExpanded} />;
  }
}
