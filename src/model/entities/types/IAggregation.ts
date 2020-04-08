export interface IAggregation {
  ColumnName: string;
  AggregationType: "Sum" | "Avg" | "Min" | "Max";
}
