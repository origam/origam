export interface IPropCursor {
  selId: string | undefined;
  isSelected: boolean;
  setSelId(id: string | undefined): void;
}
