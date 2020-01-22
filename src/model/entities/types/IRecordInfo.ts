export interface IRecordInfoData {}

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
    rowId: string
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
    rowId: string | undefined
  ): Generator;

  onSidebarInfoSectionCollapsed(): Generator;
  onSidebarInfoSectionExpanded(): Generator;
  onSidebarAuditSectionExpanded(): Generator;

  parent?: any;
}
