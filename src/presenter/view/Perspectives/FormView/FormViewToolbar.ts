import { IToolbarButtonState, IViewTypeBtn } from "../types";
import { IViewType } from "../../../../DataView/types/IViewType";
import { ML } from "../../../../utils/types";
import { IASwitchView } from "../../../../DataView/types/IASwitchView";
import { unpack } from "../../../../utils/objects";
import * as FormViewActions from "../../../../DataView/FormView/FormViewActions";
import { action, computed } from "mobx";

import { ISelection } from "../../../../DataView/Selection";
import { IDataTable } from "../../../../DataView/types/IDataTable";
import * as DataViewActions from "../../../../DataView/DataViewActions";
import { IFormViewMediator } from "../../../../DataView/FormView/FormViewMediator";

export class FormViewToolbar {
  constructor(
    public P: {
      aSwitchView: ML<IASwitchView>;
      mediator: ML<IFormViewMediator>;
      selection: ISelection;
      dataTable: IDataTable;
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
      this.mediator.dispatch(FormViewActions.selectPrevRow());
    })
  };
  btnNext: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true,
    onClick: action(() => {
      this.mediator.dispatch(FormViewActions.selectNextRow());
    })
  };
  btnLast: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true
  };

  @computed get recordNo(): string {
    return "" + ((this.P.selection.selRowIdx || 0) + 1);
  }

  @computed get recordTotal(): string {
    return "" + this.P.dataTable.existingRecordCount;
  }

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
        onClick: () =>
          this.mediator.dispatch(
            DataViewActions.switchView({ viewType: IViewType.Table })
          )
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
