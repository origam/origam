import { Machine, interpret, State } from "xstate";
import { ILoggedUser } from "../LoggedUser/types/ILoggedUser";
import { IApplicationMachine } from "./types/IApplicationMachine";
import { IAClearUserInfo } from "../LoggedUser/types/IAClearUserInfo";
import { IAnouncer } from "./types/IAnouncer";
import { IAppMachineEvent } from "./types/IAppMachineEvent";
import { IAppMachineStateSchema } from "./types/IAppMachineStateSchema";
import { IAppMachineState } from "./types/IAppMachineState";
import { action, observable, computed } from "mobx";
import { Interpreter } from "xstate/lib/interpreter";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { IApi } from "../Api/IApi";
import { IMainMenu, IMainMenuFactory } from "../MainMenu/types";
import { buildMainMenu } from "../MainMenu/factory";
import { AxiosResponse, AxiosError } from "axios";
import jwt from "jsonwebtoken";

export class ApplicationMachine implements IApplicationMachine {
  constructor(
    public P: {
      anouncer: ML<IAnouncer>;
      api: ML<IApi>;
      mainMenu: ML<IMainMenu>;
      aClearUserInfo: ML<IAClearUserInfo>;
      loggedUser: ML<ILoggedUser>;
      mainMenuFactory: ML<IMainMenuFactory>;
    }
  ) {
    this.interpreter = interpret(this.definition);
    this.interpreter.onTransition(
      action((state: State<any>) => {
        this.state = state;
        console.log(state);
      })
    );
    this.state = this.interpreter.state;
  }

  definition = Machine<{}, IAppMachineStateSchema, IAppMachineEvent>(
    {
      initial: IAppMachineState.PASS_XSTATE_INIT,
      states: {
        PASS_XSTATE_INIT: {
          invoke: {
            src: "passXStateInit",
            onDone: { target: IAppMachineState.GET_CREDENTIALS }
          }
        },
        GET_CREDENTIALS: {
          on: {
            SUBMIT_LOGIN: IAppMachineState.DO_LOGIN,
            TOKEN_LOADED: IAppMachineState.DO_LOAD_MENU
          },
          onEntry: "useStoredToken",
          onExit: "resetInform"
        },
        DO_LOGIN: {
          on: {
            DONE: IAppMachineState.DO_LOAD_MENU,
            FAILED: IAppMachineState.GET_CREDENTIALS
          },
          invoke: {
            src: "doLogin"
          }
        },
        DO_LOAD_MENU: {
          on: {
            DONE: IAppMachineState.LOGGED_IN,
            FAILED: IAppMachineState.GET_CREDENTIALS
          },
          invoke: {
            src: 'doLoadMenu'
          }
        },
        LOGGED_IN: {
          on: {
            LOGOUT: IAppMachineState.LOGOUT
          }
        },
        LOGOUT: {
          on: {
            DONE: IAppMachineState.GET_CREDENTIALS,
            FAILED: IAppMachineState.GET_CREDENTIALS
          },
          invoke: {
            src: 'doLogout'
          }
        }
      }
    },
    {
      actions: {
        resetInform: () => this.anouncer.resetInform(),
        clearUserInfo: () => this.aClearUserInfo.do(),
        useStoredToken: () => {
          const storedToken = window.sessionStorage.getItem(
            "origamAccessToken"
          );
          if (storedToken) {
            this.api.setAccessToken(storedToken);
            this.setLoggedUserFromToken(storedToken);
            this.interpreter.send({ type: "TOKEN_LOADED" });
          }
        }
      },
      services: {
        passXStateInit: () => Promise.resolve(),
        doLogin: (ctx, event) => (send, onEvent) => {
          this.api
            .login({
              UserName: event.userName,
              Password: event.password
            })
            .then(
              action((token: string) => {
                this.api.setAccessToken(token);
                window.sessionStorage.setItem("origamAccessToken", token);
                this.setLoggedUserFromToken(token);
                send("DONE");
              })
            )
            .catch(
              action((error: AxiosError) => {
                console.log(error);
                this.anouncer.inform("Login failed.");
                send("FAILED");
              })
            );

          // TODO Cancel request
          return () => 0;
        },
        doLoadMenu: (ctx, event) => (send, onEvent) => {
          this.api
            .getMenu()
            .then(
              action((menuObj: any) => {
                const menu = this.mainMenuFactory.create(menuObj);
                console.log(menu);
                this.mainMenu.setItems(menu);
                send("DONE");
              })
            )
            .catch(action((error: AxiosError) => {
              console.log(error);
              this.anouncer.inform("Could not get menu.");
              this.aClearUserInfo.do();
              send("FAILED");
            }));
          // TODO: Cancel requiest
          return () => 0;
        },
        doLogout: (ctx, event) => action((send: any, onEvent: any) => {
          const logoutPromise = this.api.logout();
          this.aClearUserInfo.do();
          logoutPromise
            .then(action(() => send("DONE")))
            .catch(action(() => send("FAILED")));
          // TODO: Cancellation?
        })
      }
    }
  );

  interpreter: Interpreter<any, any, any>;
  @observable state: State<any, any>;

  @action.bound setLoggedUserFromToken(token: any) {
    const decodedToken = jwt.decode(token);
    const userName =
      decodedToken !== null
        ? (decodedToken as {
            [key: string]: string;
          })["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]
        : undefined;
    if (userName) {
      console.log(userName);
      this.loggedUser.setUserName(userName);
    }
  }

  @action.bound start() {
    this.interpreter.start();
  }

  @action.bound stop() {
    this.interpreter.stop();
  }

  @action.bound submitLogin(userName: string, password: string) {
    this.interpreter.send({ type: "SUBMIT_LOGIN", userName, password });
  }

  @action.bound done() {
    this.interpreter.send({ type: "DONE" });
  }

  @action.bound failed() {
    this.interpreter.send({ type: "FAILED" });
  }

  @action.bound logout() {
    this.interpreter.send({ type: "LOGOUT" });
  }

  @computed get stateValue(): IAppMachineState {
    return this.state.value as IAppMachineState;
  }

  @computed get isWorking() {
    switch (this.stateValue) {
      case IAppMachineState.DO_LOGIN:
      case IAppMachineState.DO_LOAD_MENU:
        return true;
      default:
        return false;
    }
  }

  get anouncer() {
    return unpack(this.P.anouncer);
  }

  get api() {
    return unpack(this.P.api);
  }

  get mainMenu() {
    return unpack(this.P.mainMenu);
  }

  get aClearUserInfo() {
    return unpack(this.P.aClearUserInfo);
  }

  get loggedUser() {
    return unpack(this.P.loggedUser);
  }

  get mainMenuFactory() {
    return unpack(this.P.mainMenuFactory);
  }
}
