import { L } from "../../utils/types";

interface IAFinishEditing {
  do(): void;
}


export class AOnNoCellClick {
  constructor(public P: {
    aFinishEditing: L<IAFinishEditing>,
  }) { }

  do(event: any) {
    this.P.aFinishEditing().do();
  }
}
