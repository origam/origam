import { IToolbarButtonState, IViewTypeBtn } from "../types";
import { IViewType } from "../../../../DataView/types/IViewType";
import { unpack } from "../../../../utils/objects";
import { computed, action } from "mobx";
import { ML } from "../../../../utils/types";
import { IDataTable } from "../../../../DataView/types/IDataTable";
import { IASwitchView } from "../../../../DataView/types/IASwitchView";
import { IAvailViews } from "../../../../DataView/types/IAvailViews";
import { IASelNextRec } from "../../../../DataView/types/IASelNextRec";
import { IASelPrevRec } from "../../../../DataView/types/IASelPrevRec";
import { IDataViewMediator } from "../../../../DataView/types/IDataViewMediator";
import * as DataViewActions from "../../../../DataView/DataViewActions";

export class TableViewToolbar {
  constructor(
    public P: {
      dataTable: ML<IDataTable>;
      aSwitchView: ML<IASwitchView>;
      mediator: ML<IDataViewMediator>;
    }
  ) {}

  isLoading: boolean = false;
  isError: boolean = false;
  label: string = "Form view label";
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
  recordNo: string = "0";
  @computed get recordTotal(): string {
    return "" + this.dataTable.existingRecordCount;
  }

  btnsViews: IViewTypeBtn[] = [
    {
      btn: {
        isActive: false,
        isVisible: true,
        isEnabled: true,
        onClick: () => this.aSwitchView.do(IViewType.Form)
      },
      type: IViewType.Form
    },
    {
      btn: { isActive: true, isVisible: true, isEnabled: true },
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

  get dataTable() {
    return unpack(this.P.dataTable);
  }

  get aSwitchView() {
    return unpack(this.P.aSwitchView);
  }

  get mediator() {
    return unpack(this.P.mediator);
  }
}
