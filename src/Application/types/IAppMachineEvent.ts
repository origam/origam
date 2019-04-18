export type IAppMachineEvent = {
  type: "SUBMIT_LOGIN";
  userName: string;
  password: string;
} | {
  type: "DONE";
} | {
  type: "FAILED";
} | {
  type: "LOGOUT";
} | {
  type: "TOKEN_LOADED";
};
