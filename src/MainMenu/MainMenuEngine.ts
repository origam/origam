import { observable, action } from "mobx";

export class MainMenuEngine {

  @observable public expandedSections = new Map();

  @action.bound
  public expandSection(id: string) {
    this.expandedSections.set(id, true);
  }

  @action.bound
  public collapseSection(id: string) {
    this.expandedSections.delete(id);
  }

  public isSectionExpanded(id: string): boolean {
    return this.expandedSections.has(id);
  }

  @action.bound
  public handleSectionClick(event: any, id: string) {
    if (this.isSectionExpanded(id)) {
      this.collapseSection(id);
    } else {
      this.expandSection(id);
    }
    console.log(Array.from(this.expandedSections));
  }


}