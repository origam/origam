export interface IAggregation {
  ColumnName: string;
  AggregationType: AggregationType;
}

export enum AggregationType{ SUM="SUM", AVG="AVG", MIN="MIN", MAX="MAX"}

export function aggregationTypeParse(candidate: any){
  switch (candidate) {
    case "SUM": return AggregationType.SUM;
    case "AVG": return AggregationType.AVG;
    case "MIN": return AggregationType.MIN;
    case "MAX": return AggregationType.MAX;
    default: throw new Error("Cannot map \""+candidate+"\" to AggregationType")
  }
}
