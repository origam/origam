export interface IMainMenu {
  isSectionExpanded(id: string): boolean;
  handleSectionClick(event: any, id: string): void;
}