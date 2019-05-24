import { IToolbarButtonState, IViewTypeBtn } from "../types";
import { IViewType } from "../../../../DataView/types/IViewType";
import { unpack } from "../../../../utils/objects";
import { computed, action } from "mobx";
import { ML } from "../../../../utils/types";
import { IDataTable } from "../../../../DataView/types/IDataTable";
import { IASwitchView } from "../../../../DataView/types/IASwitchView";
import * as DataViewActions from "../../../../DataView/DataViewActions";
import * as TableViewActions from "../../../../DataView/TableView/TableViewActions";
import { IDataViewMediator02 } from "../../../../DataView/DataViewMediator02";
import { ISelection } from "../../../../DataView/Selection";
import { ITableViewMediator } from "../../../../DataView/TableView/TableViewMediator";

export class TableViewToolbar {
  constructor(
    public P: {
      dataTable: ML<IDataTable>;
      selection: ISelection;
      aSwitchView: ML<IASwitchView>;
      mediator: ML<ITableViewMediator>;
      label: string;
      isLoading: () => boolean;
    }
  ) {}

  get isLoading(): boolean {
    return this.P.isLoading();
  }
  isError: boolean = false;
  get label(): string {
    return this.P.label;
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
    isVisible: true,
    onClick: action(() => {
      this.mediator.dispatch(DataViewActions.createRow());
    })
  };
  btnDelete: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true,
    onClick: action(() => {
      this.mediator.dispatch(DataViewActions.deleteSelectedRow());
    })
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
      this.mediator.dispatch(TableViewActions.onPrevRowClick());
    })
  };
  btnNext: IToolbarButtonState = {
    isActive: false,
    isEnabled: true,
    isVisible: true,
    onClick: action(() => {
      this.mediator.dispatch(TableViewActions.onNextRowClick());
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
