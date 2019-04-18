import { L } from "../../utils/types";

interface IAFinishEditing {
  do(): void;
}

export class AOnOutsideGridClick {
  constructor(
    public P: {
      aFinishEditing: L<IAFinishEditing>;
    }
  ) {}

  do(event: any) {
    this.P.aFinishEditing().do();
  }
}
