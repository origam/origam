import { IFormPanelView, CFormPanelView } from './types/IFormPanelView';

export class FormPanelView implements IFormPanelView {
  $type: typeof CFormPanelView = CFormPanelView;

  parent?: any;
}