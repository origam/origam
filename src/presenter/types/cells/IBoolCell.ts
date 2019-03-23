export interface IBoolCell {
  type: "BoolCell";
  value: boolean;
  onChange(event: any, value: boolean): void;
}