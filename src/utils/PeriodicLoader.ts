import { flow } from "mobx";
import { interpret, createMachine } from "xstate";
import { PubSub } from "./events";

export class PeriodicLoader {
  private timeoutHandle: any;

  constructor(private loadFunction: () => Generator, private getChSuccessfulApi: () => PubSub<{}>) {
    this.interpreter.start();
  }

  interpreter = interpret(
    createMachine(
      {
        id: "periodicLoader",
        initial: "INITIALIZED",
        states: {
          INITIALIZED: {
            on: {
              START: {
                target: "WAIT_PERIOD_REST",
              },
            },
          },
          PERFORMING_LOAD: {
            invoke: {
              src: "svcLoadFunction",
              onDone: "WAIT_PERIOD_REST",
              onError: "FAILED",
            },
          },
          FAILED: {
            invoke: {
              src: "svcSuccessfulApiSubs",
            },
            on: {
              SOME_API_SUCCESS: "PERFORMING_LOAD",
            },
            after: {
              60000: "PERFORMING_LOAD",
            },
          },
          WAIT_PERIOD_REST: {
            after: {
              REST_DELAY: "PERFORMING_LOAD",
            },
          },
        },
      },
      {
        actions: {},
        services: {
          svcLoadFunction: (ctx, event) => async () => {
            this.t0 = new Date().valueOf();
            await flow(this.loadFunction)();
            this.t1 = new Date().valueOf();
          },
          svcSuccessfulApiSubs: (ctx, event) => (callback, onReceive) => {
            return this.getChSuccessfulApi().subscribe(() => {
              this.interpreter.send("SOME_API_SUCCESS");
            });
          },
        },
        delays: {
          REST_DELAY: (ctx, event) => {
            return Math.max(0, this.refreshIntervalMs - (this.t1 - this.t0));
          },
        },
      }
    ),
    { devTools: true }
  );

  t0 = 0;
  t1 = 0;
  refreshIntervalMs = 0;

  *start(refreshIntervalMs: number) {
    if (refreshIntervalMs <= 0) return;
    this.refreshIntervalMs = refreshIntervalMs;
    this.t0 = 0;
    this.t1 = 0;
    if (!this.interpreter.state?.matches?.("INITIALIZED")) {
      this.interpreter.stop();
      this.interpreter.start();
    }
    this.interpreter.send("START");
  }

  *stop() {
    this.interpreter.stop();
  }
}
