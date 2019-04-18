export interface IAnouncer {
  inform(message: string): () => void;
  resetInform(): void;
  message: undefined | string;
}
