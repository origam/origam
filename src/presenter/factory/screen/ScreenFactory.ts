import { IScreenFactory } from "src/presenter/types/IScreenFactory";
import * as ScreenBp from "../../types/IInfScreenBlueprints";
import {
  IScreen,
  IDataView,
  ITabs,
  ISplitter,
  ISpecificDataView,
  IViewType
} from "../../types/IScreenPresenter";
import {
  IUIScreenTreeNode,
  IUIFormRoot
} from "src/presenter/types/IUIScreenBlueprints";
import { ITableView } from "src/presenter/types/ITableViewPresenter/ITableView";
import { IFormView } from "src/presenter/types/IFormViewPresenter/IFormView";
import { computed, observable, action } from "mobx";
import { IFormField } from "src/presenter/types/IFormViewPresenter/IFormField";
import { IToolbar } from "src/presenter/types/ITableViewPresenter/IToolbar";
import { ITable } from "src/presenter/types/ITableViewPresenter/ITable";
import { IScrollState } from "src/presenter/types/ITableViewPresenter/IScrollState";
import { ICells } from "src/presenter/types/ITableViewPresenter/ICells";
import { IOrderByDirection } from "src/presenter/types/ITableViewPresenter/IOrderByDirection";
import { ICell } from "src/presenter/types/ITableViewPresenter/ICell";
import { IFormField as ITableFormField } from "src/presenter/types/ITableViewPresenter/ICursor";

class Screen implements IScreen {
  constructor(
    uiStructure: IUIScreenTreeNode[],
    dataViewsMap: Map<string, IDataView>
  ) {
    this.uiStructure = uiStructure;
    this.dataViewsMap = dataViewsMap;
  }

  cardTitle: string = "Test screen";
  screenTitle: string = "Test screen";
  isLoading: boolean = false;
  isFullScreen: boolean = true;
  uiStructure: IUIScreenTreeNode[];
  dataViewsMap: Map<string, IDataView> = new Map();
  tabPanelsMap: Map<string, ITabs> = new Map();
  splittersMap: Map<string, ISplitter> = new Map();
}

class DataView implements IDataView {
  constructor(id: string, availableViews: ISpecificDataView[]) {
    this.id = id;
    this.availableViews = availableViews;
    this.activeViewType = IViewType.FormView;
  }

  id: string;

  @computed get activeView(): ISpecificDataView | undefined {
    return this.availableViews.find(v => v.type === this.activeViewType);
  }

  @observable activeViewType: IViewType;
  availableViews: ISpecificDataView[];

  setActiveViewType(viewType: IViewType): void {
    throw new Error("Method not implemented.");
  }
}

export class FormView implements IFormView {
  constructor(uiStructure: IUIFormRoot[]) {
    this.uiStructure = uiStructure;
  }

  type: IViewType.FormView = IViewType.FormView;
  uiStructure: IUIFormRoot[];
  fields: Map<string, IFormField> = new Map();
  toolbar: IToolbar | undefined;
}

export class TableView implements ITableView {
  constructor(table: ITable) {
    this.table = table;
  }

  type: IViewType.TableView = IViewType.TableView;
  toolbar: IToolbar | undefined;
  table: ITable;
}

export class Table implements ITable {
  constructor(cells: ICells, tableFormField: ITableFormField) {
    this.cells = cells;
    this.cursor = tableFormField;
  }
  isLoading: boolean = false;
  filterSettingsVisible: boolean = false;
  scrollState: IScrollState = new ScrollState(0, 0);
  cells: ICells;
  cursor: ITableFormField;
}

export class Cells implements ICells {
  @observable columnReordering: string[] = [];

  CW = 80;
  RH = 20;

  get columnCount() {
    return 100;
  }

  get rowCount() {
    return 1000;
  }

  fixedColumnCount = 2;

  get contentWidth() {
    return this.getColumnRight(this.columnCount - 1);
  }

  get contentHeight() {
    return this.getRowBottom(this.rowCount - 1);
  }

  getRowTop(rowIdx: number) {
    return rowIdx * this.RH;
  }

  getRowHeight(rowIdx: number) {
    return this.RH;
  }

  getRowBottom(rowIdx: number) {
    return rowIdx * this.RH + this.RH;
  }

  getColumnLeft(colIdx: number) {
    return colIdx * this.CW;
  }

  getColumnWidth(colIdx: number) {
    return this.CW;
  }

  getColumnRight(colIdx: number) {
    return this.CW * colIdx + this.CW;
  }

  getHeader(colIdx: number) {
    return {
      label: `Col ${colIdx}`,
      orderBy: {
        direction: IOrderByDirection.NONE,
        order: 2
      },
      filter: {}
    };
  }

  getCell(rowIdx: number, colIdx: number): ICell {
    return {
      type: "TextCell",
      value: `${rowIdx}-${colIdx}`,
      isLoading: false,
      isInvalid: false,
      isReadOnly: false,
      isRowCursor: false,
      isCellCursor: false,
      onChange(event: any, val: string) {
        return;
      }
    };
  }
}

class ScrollState implements IScrollState {
  constructor(scrollTop: number, scrollLeft: number) {
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }
  @observable scrollTop = 0;
  @observable scrollLeft = 0;

  @action.bound
  setScrollOffset(event: any, scrollTop: number, scrollLeft: number): void {
    console.log("scroll event: ", scrollTop, scrollLeft);
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }
}

export class ScreenFactory implements IScreenFactory {
  getScreen(blueprint: ScreenBp.IScreen): IScreen {
    const dataViewsBp = Array.from(blueprint.dataViewsMap);
    const dataViewsEnt = dataViewsBp.map(([id, view]) => {
      const availableViews = view.availableViews
        .map(availV => {
          switch (availV.type) {
            case IViewType.FormView:
              return new FormView(availV.uiStructure);
            case IViewType.TableView:
              return new TableView(
                new Table(new Cells(), {
                  field: undefined,
                  rowIndex: 0,
                  columnIndex: 0,
                  isEditing: false
                })
              );
          }
        })
        .filter(item => item !== undefined);
      return [
        id,
        new DataView(view.initialView, availableViews as FormView[])
      ] as [string, IDataView];
    });
    return new Screen(blueprint.uiStructure, new Map(dataViewsEnt));
  }
}
