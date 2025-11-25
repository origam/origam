import { Result } from '@api/IArchitectApi.ts';
import { GridEditorState } from '@editors/gridEditor/GridEditorState.ts';

export class XsltEditorState extends GridEditorState {
  *validate(): Generator<Promise<Result>, Result, Result> {
    return yield this.architectApi.validateTransformation(this.editorNode.origamId);
  }
}
