import { IToolbarButtonState, IViewTypeBtn } from "../IScreenPresenter";


export interface IToolbar {
  isLoading: boolean;
  isError: boolean;
  label: string;
  isFiltered: boolean;
  btnMoveUp: IToolbarButtonState;
  btnMoveDown: IToolbarButtonState;
  btnAdd: IToolbarButtonState;
  btnDelete: IToolbarButtonState;
  btnCopy: IToolbarButtonState;
  btnFirst: IToolbarButtonState;
  btnPrev: IToolbarButtonState;
  btnNext: IToolbarButtonState;
  btnLast: IToolbarButtonState;
  recordNo: string;
  recordTotal: string;
  btnsViews: IViewTypeBtn[];
  btnFilter: IToolbarButtonState;
  btnFilterDropdown: IToolbarButtonState;
  btnSettings: IToolbarButtonState;
}