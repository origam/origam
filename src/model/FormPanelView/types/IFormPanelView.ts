export const CFormPanelView = "CFormPanelView";

export interface IFormPanelViewData {

}

export interface IFormPanelView extends IFormPanelViewData {
  $type: typeof CFormPanelView;

  parent?:any;
}