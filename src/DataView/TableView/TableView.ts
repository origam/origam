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
import { stopDispatch } from "../DataViewMediator";
import * as TableViewActions from "./TableViewActions";


export class TableView implements ITableView {
  constructor(
    public P: {
      propIds?: string[];
      dataView: ML<DataView>;
      mediator: IDataViewMediator;
    }
  ) {
    this.mediator = P.mediator;
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

  mediator: IDataViewMediator;

  type: IViewType.Table = IViewType.Table;

  /* ACTIONS */
  aOnCellClick = new AOnCellClick({ aSelCell: () => this.aSelCell });
  aOnNoCellClick = new AOnNoCellClick({
    aFinishEditing: () => this.aFinishEdit
  });
  aOnGridKeyDown = new AOnGridKeyDown({});
  aOnEditorKeyDown = new AOnEditorKeyDown({}); // TODO
  aOnScroll = new AOnScroll({}); // TODO
  aOnOutsideGridClick = new AOnOutsideGridClick({
    aFinishEditing: () => this.aFinishEdit
  });

  activateView = new AActivateView({ availViews: () => this.availViews });

  aSelNextProp = new ASelNextProp({
    propCursor: () => this.propCursor,
    props: () => this.propReorder,
    aSelProp: () => this.aSelProp
  });
  aSelPrevProp = new ASelPrevProp({
    propCursor: () => this.propCursor,
    props: () => this.propReorder,
    aSelProp: () => this.aSelProp
  });

  aSelProp = new ASelProp({
    aSelCell: () => this.aSelCell   
  });

  aSelNextRec = new ASelNextRec({
    records: () => this.records,
    recCursor: () => this.recCursor,
    aSelRec: () => this.aSelRec
  });
  aSelPrevRec = new ASelPrevRec({
    records: () => this.records,
    recCursor: () => this.recCursor,
    aSelRec: () => this.aSelRec
  });

  aSelRec = new ASelRec({
    aSelCell: () => this.aSelCell
  });


  aSelCell = new ASelCell({
    recCursor: () => this.recCursor,
    propCursor: () => this.propCursor,
    propReorder: () => this.propReorder,
    dataTable: () => this.dataView.dataTable,
    editing: () => this.editing,
    aStartEditing: () => this.aStartEdit,
    aFinishEditing: () => this.aFinishEdit,
    form: () => this.form
  });

  propCursor = new PropCursor({});

  propReorder = new PropReorder({
    props: () => this.props,
    initPropIds: this.P.propIds
  });

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
