


interface IActiveState {
  isActive: boolean;
}

export interface ITabPanelProps {
  activeState?: IActiveState;
  id: string;
  activePanelId: string;
}

export interface ITabsProps {
  Id: string;
}