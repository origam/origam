import { action } from "mobx";
import { IDataViewMediator02 } from "../../../../DataView/DataViewMediator02";
import * as TableViewActions from "../../../../DataView/TableView/TableViewActions";
import { IAFinishEditing } from "../../../../DataView/types/IAFinishEditing";
import { IEditing } from "../../../../DataView/types/IEditing";
import { unpack } from "../../../../utils/objects";
import { ML } from "../../../../utils/types";
import { ICells, IFormField, IScrollState, ITable } from "./types";
import { ITableViewMachine } from "../../../../DataView/TableView/types/ITableViewMachine";
import { ITableViewMediator } from "../../../../DataView/TableView/TableViewMediator";


export class TableViewTable implements ITable {
  constructor(
    public P: {
      scrollState: ML<IScrollState>;
      cells: ML<ICells>;
      cursor: ML<IFormField>;
      mediator: ML<ITableViewMediator>;
      editing: ML<IEditing>;
      aFinishEditing: ML<IAFinishEditing>;
      isLoading: () => boolean;
    }
  ) {}

  get isLoading(): boolean {
    return this.P.isLoading();
  }
  filterSettingsVisible: boolean = false;

  @action.bound
  onCellClick(event: any, rowIdx: number, columnIdx: number) {
    debugger
    this.mediator.dispatch(TableViewActions.onCellClick({rowIdx, columnIdx}));
  }

  @action.bound onNoCellClick(event: any) {
    console.log("No cell click");
    this.mediator.dispatch(TableViewActions.onNoCellClick());
  }

  @action.bound onOutsideTableClick(event: any) {
    console.log("Outside table click");
    this.mediator.dispatch(TableViewActions.onOutsideTableClick());
  }

  @action.bound
  onKeyDown(event: any) {
    // TODO: dispatch this simply as onKeyDown
    switch (event.key) {
      case "ArrowUp":
        this.mediator.dispatch(TableViewActions.selectPrevRow());
        event.preventDefault();
        break;
      case "ArrowDown":
        this.mediator.dispatch(TableViewActions.selectNextRow());
        event.preventDefault();
        break;
      case "ArrowLeft":
        this.mediator.dispatch(TableViewActions.selectPrevColumn());
        event.preventDefault();
        break;
      case "ArrowRight":
        this.mediator.dispatch(TableViewActions.selectNextColumn());
        event.preventDefault();
        break;
    }
  }

  listenMediator(cb: (event: any) => void): () => void {
    return this.mediator.listen(cb);
  }

  onBeforeRender() {
    return;
  }

  onAfterRender() {
    return;
  }

  get scrollState(): IScrollState {
    return unpack(this.P.scrollState);
  }

  get cells(): ICells {
    return unpack(this.P.cells);
  }

  get cursor(): IFormField {
    return unpack(this.P.cursor);
  }

  get mediator() {
    return unpack(this.P.mediator);
  }

  get editing() {
    return unpack(this.P.editing);
  }

  get aFinishEditing() {
    return unpack(this.P.aFinishEditing);
  }
}
