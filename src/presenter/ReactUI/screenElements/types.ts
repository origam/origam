import { ITabs, ITabPanel } from "src/presenter/types/IScreenPresenter";


interface IActiveState {
  isActive: boolean;
}

export interface ITabPanelProps {
  activeState?: IActiveState;
  id: string;
  activePanelId: string;
}

export interface ITabsProps {
  id: string;
  controller?: ITabs;
  panels: ITabPanel[];
}