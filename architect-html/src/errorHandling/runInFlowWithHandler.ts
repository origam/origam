import { flow } from "mobx";
import { handleError } from "src/errorHandling/handleError.tsx";
import { ErrorDialogController } from "src/errorHandling/ErrorDialog.tsx";

type GeneratorFunction = (...args: any[]) => Generator;
type GeneratorInput = Generator | GeneratorFunction;
type ActionInput = (() => Promise<any>) | (() => void);

export type FlowHandlerInput =
  | { action: ActionInput }
  | { generator: GeneratorInput };

export function wrapInFlowWithHandler(args: {
  controller: ErrorDialogController;
  action: ActionInput;
}) {
  return flow(function* runWithHandler() {
    try {
      yield args.action();
    } catch (e) {
      yield* handleError(args.controller)(e);
      throw e;
    }
  });
}

export function runInFlowWithHandler(controller: ErrorDialogController){
  return function inner(args: FlowHandlerInput){
     if ('action' in args) {
    return wrapInFlowWithHandler({
      controller: controller,
      action: args.action
    })();
  } else if ('generator' in args) {
    return runGeneratorInFlowWithHandler({
      controller: controller,
      generator: args.generator
    });
  } else {
    throw new Error('Invalid input, need an action or a generator to run.');
  }
  }
}

function runGeneratorInFlowWithHandler(args: {
  controller: ErrorDialogController;
  generator: GeneratorInput;
}) {
  return flow(function* runWithHandler() {
    try {
      const generator = typeof args.generator === 'function'
        ? args.generator()
        : args.generator;
      return yield* generator;
    } catch (e) {
      yield* handleError(args.controller)(e);
      throw e;
    }
  })();
}