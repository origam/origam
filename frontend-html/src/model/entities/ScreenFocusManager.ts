import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { getActivePerspective } from "model/selectors/DataView/getActivePerspective";
import { EventHandler } from "utils/events";
import { findStopping } from "xmlInterpreters/xmlUtils";
import { IPanelViewType } from "model/entities/types/IPanelViewType";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { isCycleSectionsShortcut } from "utils/keyShortcuts";
import {compareTabIndexOwners} from "./TabIndexOwner";

export class ScreenFocusManager {
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

  private get dataViews(){
    return getFormScreen(this).dataViews;
  }

  public get dataViewModelInstanceIdToFocusAfterOpening(){
    let sortedDataViews = Array.from(this.dataViews).sort(compareTabIndexOwners);
    const lowestTabIndexDataView = sortedDataViews[0];
    return lowestTabIndexDataView.modelInstanceId;
  }

  dispose(){
    window.removeEventListener("focus", this.boundOnFocus, true);
    window.removeEventListener("keydown", this.boundOnKeyDown, true);
  }

  private onKeyDown(event: KeyboardEvent){
    if (isCycleSectionsShortcut(event))  {
      const activeDataViewModelInstanceId = this.getDataViewId(document.activeElement);
      const {currentDatView, nextDataView} = this.getNextVisibleDataView(activeDataViewModelInstanceId)
      const currentTablePanelView = getTablePanelView(currentDatView);
      currentTablePanelView?.setEditing(false);

      const perspective = getActivePerspective(nextDataView);
      if (perspective === IPanelViewType.Form) {
        nextDataView.formFocusManager.forceAutoFocus();
      }
      else if (perspective === IPanelViewType.Table) {
        nextDataView.gridFocusManager.focusTableIfNeeded();
      }
    }
  }

  private getNextVisibleDataView(dataViewModelInstanceId: string | null){
    const visibleDataViews=  this.dataViews.filter(x => this.isVisible(x.modelInstanceId));
    if(!dataViewModelInstanceId){
      return {
        currentDatView: undefined,
        nextDataView: visibleDataViews[0]
      };
    }
    const currentIndex = visibleDataViews.findIndex(x => x.modelInstanceId === dataViewModelInstanceId);
    const nextDataView = currentIndex == visibleDataViews.length - 1
      ? visibleDataViews[0]
      : visibleDataViews[currentIndex + 1];
    return {
      currentDatView: visibleDataViews[currentIndex],
      nextDataView: nextDataView
    };
  }

  private getDataViewId(element: Element | null){
    let parent = element;
    while(parent){
      if(parent.id.startsWith("dataView_")){
        return parent.id.substring(9);
      }
      if(parent.id.startsWith("editor_dataView_")){
        return parent.id.substring(16);
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

  setFocus() {
    const managerWithOpenEditor =
      this.dataViews.some(x => x.formFocusManager.lastFocused || x.gridFocusManager.activeEditor);
    if (!managerWithOpenEditor) {
      const formScreen = getFormScreen(this.parent);
      if (formScreen.rootDataViews.length === 1) {
        formScreen.rootDataViews[0].gridFocusManager.focusTableIfNeeded();
      }
    }
  }

  async activeEditorCloses(){
    for (const dataView of this.dataViews) {
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

