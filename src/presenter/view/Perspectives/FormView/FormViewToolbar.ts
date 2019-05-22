import { IToolbarButtonState, IViewTypeBtn } from "../types";
import { IViewType } from "../../../../DataView/types/IViewType";
import { ML } from "../../../../utils/types";
import { IASwitchView } from "../../../../DataView/types/IASwitchView";
import { unpack } from "../../../../utils/objects";
import * as DataViewActions from "../../../../DataView/DataViewActions";
import { action } from "mobx";

import { IDataViewMediator02 } from "../../../../DataView/DataViewMediator02";

export class FormViewToolbar {
  constructor(
    public P: {
      aSwitchView: ML<IASwitchView>;
      mediator: ML<IDataViewMediator02>;
      label: string;
      isLoading: () => boolean;
    }
  ) {}


  isError: boolean = false;
  get label(): string {
    return this.P.label;
  }
  get isLoading(): boolean {
    return this.P.isLoading();
  }
  isFiltered: boolean = false;
  btnMoveUp: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };
  btnMoveDown: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };
  btnAdd: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };
  btnDelete: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };
  btnCopy: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };
  btnFirst: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };
  btnPrev: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true,
    onClick: action(() => {
      this.mediator.dispatch(DataViewActions.selectPrevRow());
    })
  };
  btnNext: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true,
    onClick: action(() => {
      this.mediator.dispatch(DataViewActions.selectNextRow());
    })
  };
  btnLast: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };
  recordNo: string = "99";
  recordTotal: string = "117";
  btnsViews: IViewTypeBtn[] = [
    {
      btn: { isActive: true, isVisible: true, isEnabled: true },
      type: IViewType.Form
    },
    {
      btn: {
        isActive: false,
        isVisible: true,
        isEnabled: true,
        onClick: () => this.aSwitchView.do(IViewType.Table)
      },
      type: IViewType.Table
    }
  ];
  btnFilter: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };
  btnFilterDropdown: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };
  btnSettings: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };

  get aSwitchView() {
    return unpack(this.P.aSwitchView);
  }

  get mediator() {
    return unpack(this.P.mediator);
  }
}
