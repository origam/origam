import { action, computed, observable } from "mobx";
import { ICursor } from "src/model/entities/cursor/types/ICursor";
import { IDataTable } from "src/model/entities/data/types/IDataTable";
import { IProperties } from "src/model/entities/data/types/IProperties";
import { IFormView as IModFormView } from "src/model/entities/specificView/form/types/IFormView";
import { ITableView as IModTableView } from "src/model/entities/specificView/table/types/ITableView";
import { IDataViews } from "src/model/entities/specificViews/types/IDataViews";
import { IModel } from "src/model/types/IModel";
import { IFormField } from "src/presenter/types/IFormViewPresenter/IFormField";
import { IFormView as IPresFormView } from "src/presenter/types/IFormViewPresenter/IFormView";
import { IScreenFactory } from "src/presenter/types/IScreenFactory";
import { ICell } from "src/presenter/types/ITableViewPresenter/ICell";
import { ICells } from "src/presenter/types/ITableViewPresenter/ICells";
import { IFormField as ITableFormField } from "src/presenter/types/ITableViewPresenter/ICursor";
import { IOrderByDirection } from "src/presenter/types/ITableViewPresenter/IOrderByDirection";
import { IScrollState } from "src/presenter/types/ITableViewPresenter/IScrollState";
import { ITable } from "src/presenter/types/ITableViewPresenter/ITable";
import { ITableView as IPresTableView } from "src/presenter/types/ITableViewPresenter/ITableView";
import { IToolbar } from "src/presenter/types/ITableViewPresenter/IToolbar";
import {
  IUIFormRoot,
  IUIScreenTreeNode
} from "src/presenter/types/IUIScreenBlueprints";
import * as ScreenBp from "../../types/IInfScreenBlueprints";
import {
  IDataView as IPresDataView,
  IScreen,
  ISpecificDataView,
  ISplitter,
  ITabs,
  IToolbarButtonState,
  IViewType,
  IViewTypeBtn
} from "../../types/IScreenPresenter";
import { ITableField } from "../../types/ITableViewPresenter/ITableField";
import { IForm } from "src/model/entities/form/types/IForm";

class Screen implements IScreen {
  constructor(
    uiStructure: IUIScreenTreeNode[],
    dataViewsMap: Map<string, IPresDataView>,
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
  dataViewsMap: Map<string, IPresDataView> = new Map();
  tabPanelsMap: Map<string, ITabs> = new Map();
  splittersMap: Map<string, ISplitter> = new Map();
}

class DataView implements IPresDataView {
  constructor(
    id: string,
    defaultActiveView: IViewType,
    availableViews: ISpecificDataView[],
    public modDataViews: IDataViews
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
    this.modDataViews.activateView(viewType);
    this.activeViewType = viewType;
  }
}

export class FormView implements IPresFormView {
  constructor(
    uiStructure: IUIFormRoot[],
    toolbar: IToolbar | undefined,
    public modFormView: IModFormView
  ) {
    this.uiStructure = uiStructure;
    this.toolbar = toolbar;
  }

  type: IViewType.FormView = IViewType.FormView;
  uiStructure: IUIFormRoot[];
  toolbar: IToolbar | undefined;

  @computed
  get fields(): Map<string, IFormField> {
    const { dataTable, reorderedProperties, cursor } = this.modFormView;
    return new Map(reorderedProperties.items.map(prop => {
      const value = cursor.selRowId
        ? dataTable.getValueById(cursor.selRowId, prop.id)
        : "";
      return [
        prop.id,
        {
          isLoading: false,
          isInvalid: false,
          isReadOnly: prop.isReadOnly,
          type: "TextCell",
          value
        }
      ];
    }) as Array<[string, IFormField]>);
  }
}

export class TableView implements IPresTableView {
  constructor(table: ITable, toolbar: IToolbar | undefined) {
    this.table = table;
    this.toolbar = toolbar;
  }

  type: IViewType.TableView = IViewType.TableView;
  toolbar: IToolbar | undefined;
  table: ITable;
}

export class DefaultToolbar implements IToolbar {
  constructor(public btnsViews: IViewTypeBtn[], public label: string) {}

  dataView: IPresDataView | undefined;

  isLoading = false;
  isError = true;
  isFiltered = true;
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
  constructor(
    public type: IViewType,
    public getDataView: () => IPresDataView
  ) {}

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
    public reorderedProperties: IProperties,
    public getForm: () => IForm | undefined
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
    const colId = this.dataTable.getColumnIdByIndex(colIdx);
    const isCellCursor =
      this.cursor.selRowIdx === rowIdx && this.cursor.selColIdx === colIdx;
    const isRowCursor = this.cursor.selRowIdx === rowIdx;
    let value;
    if (this.cursor.isEditing && isRowCursor) {
      const form = this.getForm();
      value = form && colId && form.getValue(colId);
    } else {
      value = this.dataTable.getValueByIndex(rowIdx, colIdx);
    }
    return {
      type: "TextCell",
      value: value !== undefined ? value : "Unknown value",
      isLoading: false,
      isInvalid: false,
      isReadOnly: false,
      isRowCursor,
      isCellCursor,
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

export class TableFormField implements ITableFormField {
  constructor(
    public cursor: ICursor,
    public dataTable: IDataTable,
    public getForm: () => IForm | undefined
  ) {}

  @computed get field(): ITableField | undefined {
    const rowId = this.cursor.selRowId;
    const colId = this.cursor.selColId;
    const isEditing = this.cursor.isEditing;

    let value;
    if (rowId && colId) {
      if (isEditing) {
        const form = this.getForm();
        value = form ? form.getValue(colId) : "";
      } else {
        value = this.dataTable.getValueById(rowId, colId) || "";
      }
    } else {
      value = "";
    }

    const property = colId && this.dataTable.getColumnById(colId);
    const isReadOnly = property ? property.isReadOnly : false;

    return {
      isLoading: false,
      isInvalid: false,
      isReadOnly,
      type: "TextCell",
      value,
      onChange: (event: any, value: string) => {
        console.log(value);
        if (colId) {
          const form = this.getForm();
          form && form.setDirtyValue(colId, value);
        }
      }
    };
  }

  @computed get columnIndex(): number {
    return this.cursor.selColIdxReo || 0;
  }

  @computed get rowIndex(): number {
    return this.cursor.selRowIdx || 0;
  }

  @computed get isEditing(): boolean {
    return this.cursor.isEditing;
  }
}

export class ScreenFactory implements IScreenFactory {
  constructor(public model: IModel) {}

  getScreen(blueprint: ScreenBp.IScreen): IScreen {
    const dataViewsBp = Array.from(blueprint.dataViewsMap);
    const dataViewsEnt = dataViewsBp.map(([id, view]) => {
      const modDataViews = this.model.getDataViews({ dataViewId: view.id });
      if (!modDataViews) {
        throw new Error("Data view model not found: " + view.id);
      }
      const availableViews: ISpecificDataView[] = view.availableViews
        .map(availV => {
          switch (availV.type) {
            /* FORM */
            case IViewType.FormView: {
              const modFormView = modDataViews.byType(IViewType.FormView) as (
                | IModFormView
                | undefined);
              if (!modFormView) {
                throw new Error("No form view.");
              }
              let toolbar: IToolbar | undefined = undefined;
              if (!availV.isHeadless) {
                toolbar = new DefaultToolbar(
                  [
                    new DefaultViewButton(IViewType.FormView, () => dataView),
                    new DefaultViewButton(IViewType.TableView, () => dataView)
                  ],
                  view.name
                );
              }
              return new FormView(availV.uiStructure, toolbar, modFormView);
            }
            /* TABLE */
            case IViewType.TableView: {
              const modTableView = modDataViews.byType(IViewType.TableView) as (
                | IModTableView
                | undefined);
              if (!modTableView) {
                throw new Error("No table view.");
              }
              const toolbar: IToolbar = new DefaultToolbar(
                [
                  new DefaultViewButton(IViewType.FormView, () => dataView),
                  new DefaultViewButton(IViewType.TableView, () => dataView)
                ],
                view.name
              );
              const tableView: IPresTableView = new TableView(
                new Table(
                  new Cells(
                    modTableView.dataTable,
                    modTableView.cursor,
                    modTableView.reorderedProperties,
                    () => modTableView.form
                  ),
                  new TableFormField(
                    modTableView.cursor,
                    modTableView.dataTable,
                    () => modTableView.form
                  ),
                  modTableView.cursor
                ),
                toolbar
              );
              return tableView;
            }
          }
        })
        .filter(item => item !== undefined) as ISpecificDataView[];
      const dataView = new DataView(
        view.id,
        view.initialView,
        availableViews as ISpecificDataView[],
        modDataViews
      );
      return [id, dataView] as [string, IPresDataView];
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
