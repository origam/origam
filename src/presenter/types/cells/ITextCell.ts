export interface ITextCell {
  type: "TextCell"
  value: string;
  onChange?(event: any, value: string): void;
}