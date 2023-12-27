/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { IRecordInfo } from "./types/IRecordInfo";
import { getApi } from "model/selectors/getApi";
import { action, observable } from "mobx";
import { getSessionId } from "model/selectors/getSessionId";

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
    sessionId?: string;
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
    rowId: string | undefined,
    sessionId: string | undefined
  ) {
    return (
      menuId !== this.displayedFor.menuId ||
      dataStructureEntityId !== this.displayedFor.dataStructureEntityId ||
      rowId !== this.displayedFor.rowId ||
      sessionId !== this.displayedFor.sessionId
    );
  }

  hasValidLoadedFor() {
    return (
      this.displayedFor.menuId &&
      this.displayedFor.dataStructureEntityId &&
      this.displayedFor.rowId &&
      this.displayedFor.sessionId === getSessionId(this)
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
    rowId: string,
    sessionId: string,
  ) {
    if(this.recordInfoExpanded){
      return;
    }
    this.recordInfoExpanded = true;
    this.recordAuditExpanded = false;
    this.triggerInfoSectionExpand();
    yield*this.loadRecordInfo(menuId, dataStructureEntityId, rowId, sessionId);
  }

  *onSelectedRowMaybeChanged(
    menuId: string,
    dataStructureEntityId: string,
    rowId: string,
    sessionId: string
  ) {
    if (this.willLoadNewInfo(menuId, dataStructureEntityId, rowId, sessionId)) {
      if (this.recordInfoExpanded) {
        yield*this.loadRecordInfo(menuId, dataStructureEntityId, rowId, sessionId);
      }
    }
    this.displayedFor = {
      menuId,
      dataStructureEntityId,
      rowId,
      sessionId
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
  }

  *onSidebarInfoSectionExpanded() {
    if(this.recordInfoExpanded){
      return;
    }
    this.recordInfoExpanded = true;
    this.recordAuditExpanded = false;
    if (this.hasValidLoadedFor()) {
      yield*this.loadRecordInfo(
        this.displayedFor.menuId!,
        this.displayedFor.dataStructureEntityId!,
        this.displayedFor.rowId!,
        this.displayedFor.sessionId!
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
    rowId: string,
    sessionId: string
  ): any {
    const api = getApi(this);
    this.info = [];
    const rawInfo = yield api.getRecordInfo({
      MenuId: menuId,
      DataStructureEntityId: dataStructureEntityId,
      RowId: rowId,
      SessionFormIdentifier: sessionId,
    });
    if(rawInfo.cell.map){
      this.info = rawInfo.cell.map(
        (infoCellStruct: any) => infoCellStruct["#text"]
      );
    }
    else{
      this.info = [rawInfo.cell["#text"]];
    }
  }

  parent?: any;
}
