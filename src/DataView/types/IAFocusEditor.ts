export interface IAFocusEditor {
  do(): void;
  listenForRefocus(cb: () => void): () => void;
}