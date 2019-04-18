export interface ILoggedUser {
  setUserName(userName: string): void;
  resetUserName(): void;
  userName: string;
}
