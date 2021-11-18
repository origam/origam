import {IRecordInfo} from "./types/IRecordInfo";
import {getApi} from "model/selectors/getApi";
import {action, observable} from "mobx";

export class RecordInfo implements IRecordInfo {
  $type_IRecordInfo: 1 = 1;

  @observable.ref info: any[] = [];
  @observable.ref audit: any = undefined;

  @observable recordInfoExpanded = false;
  @observable recordAuditExpanded = false;

  displayedFor: {
    menuId?: string;
    dataStructureEntityId?: string;
    rowId?: string;
  } = {};

  idgen = 0;

  infoSectionExpandHandlers = new Map<number, () => void>();
  addInfoSectionExpandHandler(fn: () => void) {
    const myid = this.idgen++;
    this.infoSectionExpandHandlers.set(myid, fn);
    return () => this.infoSectionExpandHandlers.delete(myid);
  }

  @action.bound
  triggerInfoSectionExpand() {
    Array.from(this.infoSectionExpandHandlers.values()).forEach(fn => fn());
  }

  auditSectionExpandHandlers = new Map<number, () => void>();
  addAuditSectionExpandHandler(fn: () => void) {
    const myid = this.idgen++;
    this.auditSectionExpandHandlers.set(myid, fn);
    return () => this.auditSectionExpandHandlers.delete(myid);
  }

  @action.bound
  triggerAuditSectionExpand() {
    Array.from(this.auditSectionExpandHandlers.values()).forEach(fn => fn());
  }

  willLoadNewInfo(
    menuId: string | undefined,
    dataStructureEntityId: string | undefined,
    rowId: string | undefined
  ) {
    return (
      menuId !== this.displayedFor.menuId ||
      dataStructureEntityId !== this.displayedFor.dataStructureEntityId ||
      rowId !== this.displayedFor.rowId
    );
  }

  hasValidLoadedFor() {
    return (
      this.displayedFor.menuId &&
      this.displayedFor.dataStructureEntityId &&
      this.displayedFor.rowId
    );
  }

  *onSidebarInfoSectionCollapsed() {
    this.recordInfoExpanded = false;
    this.recordAuditExpanded = false;
    this.info = [];
    this.audit = undefined;
  }

  *onOpenRecordInfoClick(
    event: any,
    menuId: string,
    dataStructureEntityId: string,
    rowId: string
  ) {
    this.recordInfoExpanded = true;
    this.recordAuditExpanded = false;
    this.triggerInfoSectionExpand();
    //if (this.willLoadNewInfo(menuId, dataStructureEntityId, rowId)) {
    yield* this.loadRecordInfo(menuId, dataStructureEntityId, rowId);
    //}
  }

  *onSelectedRowMaybeChanged(
    menuId: string,
    dataStructureEntityId: string,
    rowId: string
  ) {
    if (this.willLoadNewInfo(menuId, dataStructureEntityId, rowId)) {
      if (this.recordInfoExpanded) {
        yield* this.loadRecordInfo(menuId, dataStructureEntityId, rowId);
      }
      if (this.recordAuditExpanded) {
        yield* this.loadRecordAudit(menuId, dataStructureEntityId, rowId);
      }
    }
    this.displayedFor = {
      menuId,
      dataStructureEntityId,
      rowId
    };
  }

  *onOpenRecordAuditClick(
    event: any,
    menuId: string,
    dataStructureEntityId: string,
    rowId: string
  ) {
    this.recordAuditExpanded = true;
    this.recordInfoExpanded = false;
    this.triggerAuditSectionExpand();
    //if (this.willLoadNewInfo(menuId, dataStructureEntityId, rowId)) {
    yield* this.loadRecordAudit(menuId, dataStructureEntityId, rowId);
    //}
  }

  *onSidebarInfoSectionExpanded() {
    this.recordInfoExpanded = true;
    this.recordAuditExpanded = false;
    if (this.hasValidLoadedFor()) {
      yield* this.loadRecordInfo(
        this.displayedFor.menuId!,
        this.displayedFor.dataStructureEntityId!,
        this.displayedFor.rowId!
      );
    }
  }

  *onSidebarAuditSectionExpanded() {
    this.recordInfoExpanded = false;
    this.recordAuditExpanded = true;
    if (this.hasValidLoadedFor()) {
    }
  }

  *loadRecordInfo(
    menuId: string,
    dataStructureEntityId: string,
    rowId: string
  ) {
    const api = getApi(this);
    this.info = [];
    const rawInfo = yield api.getRecordInfo({
      MenuId: menuId,
      DataStructureEntityId: dataStructureEntityId,
      RowId: rowId
    });

    const info = rawInfo.cell.map(
      (infoCellStruct: any) => infoCellStruct["#text"]
    );
    this.info = info;
  }

  *loadRecordAudit(
    menuId: string,
    dataStructureEntityId: string,
    rowId: string
  ) {
    const api = getApi(this);
    this.audit = undefined;
    const audit = yield api.getRecordAudit({
      MenuId: menuId,
      DataStructureEntityId: dataStructureEntityId,
      RowId: rowId
    });
    console.log(audit);
  }

  parent?: any;
}
