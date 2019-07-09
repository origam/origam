import { IFormDataManager, CFormDataManager } from "./types/IFormDataManager";

export class FormDataManager implements IFormDataManager {
  $type: typeof CFormDataManager = CFormDataManager;
}
