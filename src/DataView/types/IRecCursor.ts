export interface IRecCursor {
  selId: string | undefined;
  isSelected: boolean;
  setSelId(id: string): void;
}
