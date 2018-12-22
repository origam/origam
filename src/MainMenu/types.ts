export interface IMainMenu {
  reactMenu: React.ReactNode;

  isSectionExpanded(id: string): boolean;
  handleSectionClick(event: any, id: string): void;
  start(): void;
}