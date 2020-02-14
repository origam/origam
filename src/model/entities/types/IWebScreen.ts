export interface IWebScreen {
  $type_IWebScreen: 1;

  reload(): void;
  setReloader(reloader: IReloader | null): void;
}

export interface IReloader {
  reload(): void;
}

export const isIWebScreen = (o: any): o is IWebScreen => o.$type_IWebScreen;
