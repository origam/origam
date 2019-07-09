import { ITableDataManager, CTableDataManager } from './types/ITableDataManager';

export class TableDataManager implements ITableDataManager {
  $type: typeof CTableDataManager = CTableDataManager;
  
}