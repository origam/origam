import {Machine, State} from "xstate";
import axios from "axios";
import xmlJs from "xml-js";
import {
    IFormScreenMachine,
    IFormScreen,
    IScreenContentFactory
} from "./types";
import {Interpreter, interpret} from "xstate/lib/interpreter";
import {action, computed} from "mobx";
import {start} from "xstate/lib/actions";
import {ML} from "../../utils/types";
import {unpack} from "../../utils/objects";
import {IApi} from "../../Api/IApi";
import {
    startDataViews,
    activateInitialViewTypes
} from "../../DataView/DataViewActions";

export class FormScreenMachine implements IFormScreenMachine {
    constructor(
        public P: {
            menuItemId: string;
            formScreen: ML<IFormScreen>;
            screenContentFactory: ML<IScreenContentFactory>;
            api: ML<IApi>;
        }
    ) {
        this.interpreter = interpret(this.definition);
        this.interpreter.onTransition(
            action((state: State<any>, event: any) => {
                this.state = state;
            })
        );
        this.state = this.interpreter.state;
    }

    definition = Machine(
        {
            initial: "loadScreen",
            states: {
                loadScreen: {
                    on: {
                        DONE: "screenLoaded",
                        FAILED: "loadingFailed"
                    },
                    invoke: {src: "loadScreen"}
                },
                screenLoaded: {
                    on: {
                        "": [
                            {
                                cond: "isSessioned",
                                target: "createSession"
                            },
                            {
                                cond: "isNotSessioned",
                                target: "startDataViews"
                            }
                        ]
                    }
                },
                loadingFailed: {},
                createSession: {
                    invoke: {src: "createSession"},
                    on: {
                        DONE: "startDataViews"
                    }
                },
                startDataViews: {
                    onEntry: "startDataViews"
                },
                loadSessionData: {
                    invoke: {src: "loadSessionData"}
                }
            }
        },
        {
            guards: {
                isSessioned: (ctx, event) => this.formScreen.isSessioned,
                isNotSessioned: (stx, event) => !this.formScreen.isSessioned
            },
            actions: {
                startDataViews: (ctx, event) => {
                    this.formScreen.dispatch(startDataViews());
                    this.formScreen.dispatch(activateInitialViewTypes());
                }
            },
            services: {
                loadScreen: (ctx, event) => (send, onEvent) => {
                    /*axios
                      .get("/screen03.xml")
                      .then(response => {
                        return xmlJs.xml2js(response.data, {
                          addParent: true,
                          alwaysChildren: true
                        });
                      })*/
                    this.api
                        .getScreen(this.menuItemId)
                        .then(
                            action((xmlObj: any) => {
                                console.log("Loaded", xmlObj);
                                const content = this.screenContentFactory.create(xmlObj);
                                this.formScreen.setDataViews(content.dataViews);
                                this.formScreen.setUIStructure(content.screenUI);
                                // this.formScreen.isSessioned = content.isSessioned;
                                send("DONE");
                            })
                        )
                        .catch(
                            action(error => {
                                console.error(error);
                                send("FAILED");
                            })
                        );

                    return;
                },
                createSession: (ctx, event) => (send, onEvent) => {
                    this.api
                        .createSession({
                            MenuId: this.menuItemId,
                            Parameters: {},
                            InitializeStructure: true
                        })
                        .then(
                            action((sessionId: string) => {
                                console.log("createSessionResponse:", sessionId);
                                this.formScreen.sessionId = sessionId;
                                send("DONE");
                            })
                        )
                        .catch(action(error => {
                            console.log(error)
                            throw error
                        }))
                },
                loadSessionData: (ctx, event) => (send, onEvent) => {
                    // this.api.
                }
            }
        }
    );

    interpreter: Interpreter<any, any, any>;

    state: State<any>;

    @computed get stateValue() {
        return this.state.value;
    }

    @action.bound start() {
        this.interpreter.start();
    }

    @action.bound stop() {
        this.interpreter.stop();
    }

    get menuItemId() {
        return this.P.menuItemId;
    }

    get formScreen() {
        return unpack(this.P.formScreen);
    }

    get screenContentFactory() {
        return unpack(this.P.screenContentFactory);
    }

    get api() {
        return unpack(this.P.api);
    }
}
