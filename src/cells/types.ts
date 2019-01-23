import { ICellValue } from "src/Grid/types2";
import { IRecordId, IFieldId } from "src/DataTable/types";

export interface IFormEditorProps {
  value: ICellValue | undefined;
  editingRecordId: IRecordId | undefined;
  editingFieldId: IFieldId;
  onDefaultKeyDown?: (event: any) => void;
  onDefaultClick?: (event: any) => void;
  onDataCommit?: (
    dirtyValue: ICellValue,
    editingRecordId: IRecordId,
    editingFieldId: IFieldId
  ) => void;
}
