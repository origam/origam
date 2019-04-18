import { ITable, IScrollState, ICells, IFormField } from "./types";
import { ML } from "../../../../utils/types";
import { unpack } from "../../../../utils/objects";
import {
  IDataViewMediator,
  IListener
} from "../../../../DataView/types/IDataViewMediator";
import * as DataViewActions from "../../../../DataView/DataViewActions";
import { action } from "mobx";
import * as TableViewActions from "../../../../DataView/TableView/TableViewActions";

export class TableViewTable implements ITable {
  constructor(
    public P: {
      scrollState: ML<IScrollState>;
      cells: ML<ICells>;
      cursor: ML<IFormField>;
      mediator: ML<IDataViewMediator>;
    }
  ) {}

  isLoading: boolean = false;
  filterSettingsVisible: boolean = false;

  @action.bound
  onCellClick(event: any, rowIdx: number, columnIdx: number) {
    this.mediator.dispatch(
      DataViewActions.selectCellByIdx({ rowIdx, columnIdx })
    );
    this.mediator.dispatch(
      TableViewActions.makeCellVisibleByIdx({ rowIdx, columnIdx })
    );
  }

  @action.bound
  onKeyDown(event: any) {
    switch (event.key) {
      case "ArrowUp":
        this.mediator.dispatch(DataViewActions.selectPrevRow());
        event.preventDefault();
        break;
      case "ArrowDown":
        this.mediator.dispatch(DataViewActions.selectNextRow());
        event.preventDefault();
        break;
      case "ArrowLeft":
        this.mediator.dispatch(DataViewActions.selectPrevColumn());
        event.preventDefault();
        break;
      case "ArrowRight":
        this.mediator.dispatch(DataViewActions.selectNextColumn());
        event.preventDefault();
        break;
    }
  }

  listenMediator(listener: IListener) {
    return this.mediator.listen(listener);
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
}
