import { IScreenFactory } from "src/presenter/types/IScreenFactory";
import * as ScreenBp from "../../types/IInfScreenBlueprints";
import {
  IScreen,
  IDataView,
  ITabs,
  ISplitter,
  ISpecificDataView,
  IViewType,
  IViewTypeBtn,
  IToolbarButtonState
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
import { IDataTable } from "src/model/entities/data/types/IDataTable";
import { IModel } from "src/model/types/IModel";
import { ICursor } from "src/model/entities/cursor/types/ICursor";
import { IProperties } from "src/model/entities/data/types/IProperties";

class Screen implements IScreen {
  constructor(
    uiStructure: IUIScreenTreeNode[],
    dataViewsMap: Map<string, IDataView>,
    tabPanelsMap: Map<string, ITabs>
  ) {
    this.uiStructure = uiStructure;
    this.dataViewsMap = dataViewsMap;
    this.tabPanelsMap = tabPanelsMap;
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
  constructor(
    id: string,
    defaultActiveView: IViewType,
    availableViews: ISpecificDataView[]
  ) {
    this.id = id;
    this.availableViews = availableViews;
    this.activeViewType = defaultActiveView;
  }

  id: string;

  @computed get activeView(): ISpecificDataView | undefined {
    return this.availableViews.find(v => v.type === this.activeViewType);
  }

  @observable activeViewType: IViewType;
  availableViews: ISpecificDataView[];

  setActiveViewType(viewType: IViewType): void {
    this.activeViewType = viewType;
  }
}

export class FormView implements IFormView {
  constructor(uiStructure: IUIFormRoot[], toolbar: IToolbar | undefined) {
    this.uiStructure = uiStructure;
    this.toolbar = toolbar;
  }

  type: IViewType.FormView = IViewType.FormView;
  uiStructure: IUIFormRoot[];
  fields: Map<string, IFormField> = new Map();
  toolbar: IToolbar | undefined;
}

export class TableView implements ITableView {
  constructor(table: ITable, toolbar: IToolbar | undefined) {
    this.table = table;
    this.toolbar = toolbar;
  }

  type: IViewType.TableView = IViewType.TableView;
  toolbar: IToolbar | undefined;
  table: ITable;
}

export class DefaultToolbar implements IToolbar {
  constructor(public btnsViews: IViewTypeBtn[]) {}

  dataView: IDataView | undefined;

  isLoading = false;
  isError = true;
  isFiltered = true;
  label = "Example table view";
  recordNo = "17";
  recordTotal = "29";
  btnAdd = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
  btnCopy = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
  btnDelete = {
    isEnabled: false,
    isActive: false,
    isVisible: true
  };
  btnFilter = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
  btnFilterDropdown = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
  btnFirst = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
  btnPrev = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
  btnNext = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
  btnLast = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
  btnMoveDown = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
  btnMoveUp = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
  btnSettings = {
    isEnabled: true,
    isActive: false,
    isVisible: true
  };
}

export class DefaultViewButton implements IViewTypeBtn {
  constructor(public type: IViewType, public getDataView: () => IDataView) {}

  @action.bound handleClick(event: any) {
    this.getDataView().setActiveViewType(this.type);
  }

  @computed get btn(): IToolbarButtonState {
    return {
      isEnabled: true,
      isVisible: true,
      isActive: this.getDataView().activeViewType === this.type,
      onClick: this.handleClick
    };
  }
}

export class Table implements ITable {
  constructor(
    cells: ICells,
    tableFormField: ITableFormField,
    public cursor: ICursor
  ) {
    this.cells = cells;
    this.tableFormField = tableFormField;
  }
  isLoading: boolean = false;
  filterSettingsVisible: boolean = false;
  scrollState: IScrollState = new ScrollState(0, 0);
  cells: ICells;
  tableFormField: ITableFormField;

  @action.bound
  onKeyDown(event: any) {
    console.log(event.key);
    event.preventDefault();
    switch (event.key) {
      case "ArrowUp":
        this.cursor.selectPrevRow();
        break;
      case "ArrowDown":
        this.cursor.selectNextRow();
        break;
      case "ArrowLeft":
        this.cursor.selectPrevColumn();
        break;
      case "ArrowRight":
        this.cursor.selectNextColumn();
        break;
    }
  }
}

export class Cells implements ICells {
  constructor(
    dataTable: IDataTable,
    public cursor: ICursor,
    public reorderedProperties: IProperties
  ) {
    this.dataTable = dataTable;
  }

  dataTable: IDataTable;
  @observable columnReordering: string[] = [];

  CW = 80;
  RH = 20;

  @computed get reorderedPropertyIndices() {
    return this.reorderedProperties.items
      .map(prop => this.dataTable.getColumnIndexById(prop.id))
      .filter(idx => idx !== undefined) as number[];
  }

  get columnCount() {
    return this.reorderedProperties.count;
  }

  get rowCount() {
    return this.dataTable.visibleRecordCount;
  }

  @observable fixedColumnCount = 2;

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
    // TODO: It would be more pure to store property labes in the view layer.
    const property = this.reorderedProperties.byIndex(colIdx);
    return {
      label: property ? property.name : "Unknown property",
      orderBy: {
        direction: IOrderByDirection.NONE,
        order: 2
      },
      filter: {}
    };
  }

  getCell(rowIdx: number, reorderedColIdx: number): ICell {
    const colIdx = this.reorderedPropertyIndices[reorderedColIdx];
    const value = this.dataTable.getValueByIndex(rowIdx, colIdx);
    return {
      type: "TextCell",
      value: value !== undefined ? value : "Unknown value",
      isLoading: false,
      isInvalid: false,
      isReadOnly: false,
      isRowCursor: this.cursor.selRowIdx === rowIdx,
      isCellCursor:
        this.cursor.selRowIdx === rowIdx && this.cursor.selColumnIdx === colIdx,
      onCellClick: (event: any) => {
        this.cursor.selectCellByIdx(rowIdx, colIdx);
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

export class TabbedPanel implements ITabs {
  constructor(activeTabId: string) {
    this.activeTabId = activeTabId;
  }

  @observable activeTabId: string;

  @action.bound
  onHandleClick(event: any, handleId: string) {
    this.activeTabId = handleId;
  }
}

export class ScreenFactory implements IScreenFactory {
  constructor(public model: IModel) {}

  getScreen(blueprint: ScreenBp.IScreen): IScreen {
    const dataViewsBp = Array.from(blueprint.dataViewsMap);
    const dataViewsEnt = dataViewsBp.map(([id, view]) => {
      const availableViews: ISpecificDataView[] = view.availableViews
        .map(availV => {
          switch (availV.type) {
            /* FORM */
            case IViewType.FormView: {
              let toolbar: IToolbar | undefined = undefined;
              if (!availV.isHeadless) {
                toolbar = new DefaultToolbar([
                  new DefaultViewButton(IViewType.FormView, () => dataView),
                  new DefaultViewButton(IViewType.TableView, () => dataView)
                ]);
              }
              return new FormView(availV.uiStructure, toolbar);
            }
            /* TABLE */
            case IViewType.TableView: {
              const modTableView = this.model.getTableView({
                dataViewId: view.id
              });
              if (!modTableView) {
                throw new Error("No table view.");
              }
              const toolbar: IToolbar = new DefaultToolbar([
                new DefaultViewButton(IViewType.FormView, () => dataView),
                new DefaultViewButton(IViewType.TableView, () => dataView)
              ]);

              return new TableView(
                new Table(
                  new Cells(
                    modTableView.dataTable,
                    modTableView.cursor,
                    modTableView.reorderedProperties
                  ),
                  {
                    field: undefined,
                    rowIndex: 0,
                    columnIndex: 0,
                    isEditing: false
                  },
                  modTableView.cursor
                ),
                toolbar
              );
            }
          }
        })
        .filter(item => item !== undefined) as ISpecificDataView[];
      const dataView = new DataView(
        view.id,
        view.initialView,
        availableViews as ISpecificDataView[]
      );
      return [id, dataView] as [string, IDataView];
    });

    const tabPanelsBp = Array.from(blueprint.tabPanelsMap);
    const tabPanelsEnt = tabPanelsBp.map(([id, panel]) => {
      return [id, new TabbedPanel(panel.activeTabId)] as [string, ITabs];
    });
    return new Screen(
      blueprint.uiStructure,
      new Map(dataViewsEnt),
      new Map(tabPanelsEnt)
    );
  }
}
