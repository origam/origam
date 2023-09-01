import { FormFocusManager } from "model/entities/FormFocusManager";
import { GridFocusManager } from "model/entities/GridFocusManager";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { getActivePerspective } from "model/selectors/DataView/getActivePerspective";
import { getDataView } from "model/selectors/DataView/getDataView";
import { EventHandler } from "utils/events";
import { findStopping } from "xmlInterpreters/xmlUtils";

export class ScreenFocusManager {
  private gridManagers: GridFocusManager[] = [];
  private formManagers: FormFocusManager[] = [];
  focusOutsideOfGridEditor = new EventHandler<FocusEvent>();
  parent: any;
  private visibleDataViews = new Map<string, string[]>();
  private boundOnFocus = this.onFocus.bind(this);
  private boundOnKeyDown = this.onKeyDown.bind(this);
  private uiRoot: any;

  constructor(uiRoot: any) {
    this.uiRoot = uiRoot;
    window.addEventListener("focus", this.boundOnFocus, true);
    window.addEventListener("keydown", this.boundOnKeyDown, true);
  }

  dispose(){
    window.removeEventListener("focus", this.boundOnFocus, true);
    window.removeEventListener("keydown", this.boundOnKeyDown, true);
  }

  private onKeyDown(event: KeyboardEvent){
    if(event.key === "F6"){
      console.log("F6");
      const activeDataViewModelInstanceId = this.getDataViewId(document.activeElement);
      const gridManager = this.getNextVisibleGridManager(activeDataViewModelInstanceId)
      gridManager.focusTableIfNeeded();
    }
  }

  private getNextVisibleGridManager(dataViewModelInstanceId: string | null){
    const visibleGridManagers=  this.gridManagers.filter(x => this.isVisible(x.dataViewModelInstanceId));
    if(!dataViewModelInstanceId){
      return visibleGridManagers[0];
    }
    const currentIndex = visibleGridManagers.findIndex(x => x.dataViewModelInstanceId === dataViewModelInstanceId);
    return currentIndex == visibleGridManagers.length - 1
      ? visibleGridManagers[0]
      : visibleGridManagers[currentIndex + 1];
  }

  private getDataViewId(element: Element | null){
    let parent = element;
    while(parent){
      if(parent.id.startsWith("dataView_")){
        return parent.id.substring(9);
      }
      parent = parent.parentElement;
    }
    return null;
  }

  private onFocus(event: FocusEvent) {
    if(!event.relatedTarget){
      const target = event.target as HTMLElement;
      if(!this.isGridEditor(target)){
        this.focusOutsideOfGridEditor.trigger(event);
      }
    }
  }

  private isGridEditor(element: HTMLElement){
    let parent = element.parentElement;
    while(parent){
      if(parent.id === "form-field-portal"){
        return true;
      }
      parent = parent.parentElement;
    }
    return false;
  }

  registerGridFocusManager(manager:GridFocusManager) {
    this.gridManagers.push(manager);
  }
  registerFormFocusManager(manager:FormFocusManager) {
    this.formManagers.push(manager);
  }

  setFocus() {
    const managerWithOpenEditor =
      this.gridManagers.some(x => x.activeEditor) ||
      this.formManagers.some(x => x.lastFocused);
    if (!managerWithOpenEditor) {
      const formScreen = getFormScreen(this.parent);
      if (formScreen.rootDataViews.length === 1) {
        formScreen.rootDataViews[0].gridFocusManager.focusTableIfNeeded();
      }
    }
  }

  async activeEditorCloses(){
    for (const gridManager of this.gridManagers) {
      const dataView = getDataView(gridManager);
      if(dataView.isFormViewActive())
      {
        await dataView.formFocusManager.activeEditorCloses();
      }
      else if (dataView.isTableViewActive())
      {
        await dataView.gridFocusManager.activeEditorCloses();
      }
    }
  }

  private isVisible(dataViewModelInstanceId: string){
    const tabs = this.findTabsAmongParents(dataViewModelInstanceId);
    for (const tab of tabs) {
      const visibleDataViewsOnTheTab = this.visibleDataViews.get(tab.attributes.Id);
      if(visibleDataViewsOnTheTab && !visibleDataViewsOnTheTab.includes(dataViewModelInstanceId)){
        return false;
      }
    }
    return true;
  }

  private findTabsAmongParents(dataViewModelInstanceId: string){
    const dataViewNode =
      findStopping(this.uiRoot, node => node.attributes?.["ModelInstanceId"] === dataViewModelInstanceId)
        .find(element => element);

    const tabs = [];
    let parentNode = dataViewNode;
    while(parentNode){
      if(parentNode.attributes?.["Type"] === "Tab"){
        tabs.push(parentNode);
      }
      parentNode = parentNode.parent;
    }
    return tabs;
  }

  public setVisibleDataViews(parentId: string, dataViewIds: string[]) {
    this.visibleDataViews.set(parentId, dataViewIds);
  }
}

