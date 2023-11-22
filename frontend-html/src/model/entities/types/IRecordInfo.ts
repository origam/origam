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

export interface IRecordInfoData {
}

export enum IAuditLogActionType {
  New = 4,
  Delete = 8,
  Change = 16,
  Clone = 32
}

export interface IRecordInfo extends IRecordInfoData {
  $type_IRecordInfo: 1;

  info: string[];
  audit: any;

  addInfoSectionExpandHandler(fn: () => void): void;

  addAuditSectionExpandHandler(fn: () => void): void;

  onOpenRecordInfoClick(
    event: any,
    menuId: string,
    dataStructureEntityId: string,
    rowId: string,
    sessionId: string,
  ): Generator;

  onOpenRecordAuditClick(
    event: any,
    menuId: string,
    dataStructureEntityId: string,
    rowId: string
  ): Generator;

  onSelectedRowMaybeChanged(
    menuId: string,
    dataStructureEntityId: string,
    rowId: string | undefined,
    sessionId: string
  ): Generator;

  onSidebarInfoSectionCollapsed(): Generator;

  onSidebarInfoSectionExpanded(): Generator;

  onSidebarAuditSectionExpanded(): Generator;

  parent?: any;
}
