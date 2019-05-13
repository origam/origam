import { unpack } from "../../utils/objects";
import { ML } from "../../utils/types";
import { ASelCell } from "../ASelCell";
import { ASelNextProp } from "../ASelNextProp";
import { ASelNextRec } from "../ASelNextRec";
import { ASelPrevProp } from "../ASelPrevProp";
import { ASelPrevRec } from "../ASelPrevRec";
import { ASelProp } from "../ASelProp";
import { ASelRec } from "../ASelRec";
import { DataView } from "../DataView";
import { AOnEditorKeyDown } from "../FormView/AOnEditorKeyDown";
import { PropCursor } from "../PropCursor";
import { PropReorder } from "../PropReorder";
import { IViewType } from "../types/IViewType";
import { AActivateView } from "./AActivateView";
import { AOnCellClick } from "./AOnCellClick";
import { AOnGridKeyDown } from "./AOnGridKeyDown";
import { AOnNoCellClick } from "./AOnNoCellClick";
import { AOnOutsideGridClick } from "./AOnOutsideGridClick";
import { AOnScroll } from "./AOnScroll";
import { ITableView } from "./ITableView";
import { IDataViewMediator } from "../types/IDataViewMediator";
import { action } from "mobx";
import { isType } from "ts-action";
import * as DataViewActions from "../DataViewActions";
import * as TableViewActions from "./TableViewActions";
import { ADeactivateView } from "./ADeactivateView";

import { IADeactivateView } from "../types/IADeactivateView";
import { IASelNextProp } from "../types/IASelNextProp";
import { IASelPrevProp } from "../types/IASelPrevProp";
import { IASelProp } from "../types/IASelProp";
import { IASelNextRec } from "../types/IASelNextRec";
import { IASelPrevRec } from "../types/IASelPrevRec";
import { IASelRec } from "../types/IASelRec";
import { IASelCell } from "../types/IASelCell";
import { IPropCursor } from "../types/IPropCursor";
import { IPropReorder } from "../types/IPropReorder";
import { IAActivateView } from "../types/IAActivateView";

export class TableView implements ITableView {
  constructor(
    public P: {
      propIds?: string[];
      dataView: ML<DataView>;
      mediator: IDataViewMediator;
    }
  ) {
    this.mediator = P.mediator;

    /* ACTIONS */
    this.aOnCellClick = new AOnCellClick({ aSelCell: () => this.aSelCell });
    this.aOnNoCellClick = new AOnNoCellClick({
      aFinishEditing: () => this.aFinishEdit
    });
    this.aOnGridKeyDown = new AOnGridKeyDown({});
    this.aOnEditorKeyDown = new AOnEditorKeyDown({}); // TODO
    this.aOnScroll = new AOnScroll({}); // TODO
    this.aOnOutsideGridClick = new AOnOutsideGridClick({
      aFinishEditing: () => this.aFinishEdit
    });

    this.aActivateView = new AActivateView({});
    this.aDeactivateView = new ADeactivateView();

    this.aSelNextProp = new ASelNextProp({
      propCursor: () => this.propCursor,
      props: () => this.propReorder,
      aSelProp: () => this.aSelProp
    });
    this.aSelPrevProp = new ASelPrevProp({
      propCursor: () => this.propCursor,
      props: () => this.propReorder,
      aSelProp: () => this.aSelProp
    });

    this.aSelProp = new ASelProp({
      aSelCell: () => this.aSelCell
    });

    this.aSelNextRec = new ASelNextRec({
      records: () => this.records,
      recCursor: () => this.recCursor,
      aSelRec: () => this.aSelRec
    });
    this.aSelPrevRec = new ASelPrevRec({
      records: () => this.records,
      recCursor: () => this.recCursor,
      aSelRec: () => this.aSelRec
    });

    this.aSelRec = new ASelRec({
      aSelCell: () => this.aSelCell
    });

    this.aSelCell = new ASelCell({
      recCursor: () => this.recCursor,
      propCursor: () => this.propCursor,
      propReorder: () => this.propReorder,
      dataTable: () => this.dataView.dataTable,
      editing: () => this.editing,
      aStartEditing: () => this.aStartEdit,
      aFinishEditing: () => this.aFinishEdit,
      form: () => this.form,
      mediator: this.mediator
    });

    this.propCursor = new PropCursor({});

    this.propReorder = new PropReorder({
      properties: () => this.props,
      initPropIds: this.P.propIds
    });

    this.subscribeMediator();
  }

  subscribeMediator() {
    this.mediator.listen((action, sender) => {
      if (isType(action, DataViewActions.selectFirstCell)) {
        this.aSelCell.doSelFirst();
      } else if (isType(action, DataViewActions.selectCellByIdx)) {
        this.aSelCell.doByIdx(action.payload.rowIdx, action.payload.columnIdx);
      } else if (isType(action, DataViewActions.selectNextColumn)) {
        this.aSelNextProp.do();
        this.makeSelectedCellVisible();
      } else if (isType(action, DataViewActions.selectPrevColumn)) {
        this.aSelPrevProp.do();
        this.makeSelectedCellVisible();
      } else if (isType(action, DataViewActions.selectNextRow)) {
        this.aSelNextRec.do();
        this.makeSelectedCellVisible();
      } else if (isType(action, DataViewActions.selectPrevRow)) {
        this.aSelPrevRec.do();
        this.makeSelectedCellVisible();
      } else if (isType(action, TableViewActions.makeCellVisibleById)) {
        // TODO: Move this to its own method.
        const columnIdx = this.propReorder.getIndexById(
          action.payload.columnId
        );
        const rowIdx = this.dataView.dataTable.getRecordIndexById(
          action.payload.rowId
        );
        if (columnIdx !== undefined && rowIdx !== undefined) {
          this.mediator.dispatch(
            TableViewActions.makeCellVisibleByIdx({ rowIdx, columnIdx })
          );
        }
      }
    });
  }

  @action.bound makeSelectedCellVisible() {
    const rowId = this.dataView.recCursor.selId;
    const columnId = this.propCursor.selId;
    if (rowId && columnId) {
      this.mediator.dispatch(
        TableViewActions.makeCellVisibleById({ rowId, columnId })
      );
    }
  }

  // TODO: Change to abstractions
  aOnCellClick: AOnCellClick;
  aOnNoCellClick: AOnNoCellClick;
  aOnGridKeyDown: AOnGridKeyDown;
  aOnEditorKeyDown: AOnEditorKeyDown;
  aOnScroll: AOnScroll;
  aOnOutsideGridClick: AOnOutsideGridClick;
  aActivateView: IAActivateView;
  aDeactivateView: IADeactivateView;
  aSelNextProp: IASelNextProp;
  aSelPrevProp: IASelPrevProp;
  aSelProp: IASelProp;
  aSelNextRec: IASelNextRec;
  aSelPrevRec: IASelPrevRec;
  aSelRec: IASelRec;
  aSelCell: IASelCell;
  propCursor: IPropCursor;
  propReorder: IPropReorder;

  mediator: IDataViewMediator;

  type: IViewType.Table = IViewType.Table;

  get dataView() {
    return unpack(this.P.dataView);
  }

  get form() {
    return this.dataView.form;
  }

  get editing() {
    return this.dataView.editing;
  }

  get props() {
    return this.dataView.props;
  }

  get records() {
    return this.dataView.records;
  }

  get aStartEdit() {
    return this.dataView.aStartEditing;
  }

  get aFinishEdit() {
    return this.dataView.aFinishEditing;
  }

  get availViews() {
    return this.dataView.availViews;
  }

  get recCursor() {
    return this.dataView.recCursor;
  }
}
