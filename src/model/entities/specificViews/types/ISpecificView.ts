export interface ISpecificView {
  type: string;
  activateView(): void;
  deactivateView(): void;
}