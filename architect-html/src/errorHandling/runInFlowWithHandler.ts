/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { ErrorDialogController } from '@errors/ErrorDialog';
import { handleError } from '@errors/handleError';
import { flow } from 'mobx';

type GeneratorFunction = (...args: any[]) => Generator;
type GeneratorInput = Generator | GeneratorFunction;
type ActionInput = (() => Promise<any>) | (() => void);

export type FlowHandlerInput = { action: ActionInput } | { generator: GeneratorInput };

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

export function runInFlowWithHandler(controller: ErrorDialogController) {
  return function inner(args: FlowHandlerInput) {
    if ('action' in args) {
      return wrapInFlowWithHandler({
        controller: controller,
        action: args.action,
      })();
    } else if ('generator' in args) {
      return runGeneratorInFlowWithHandler({
        controller: controller,
        generator: args.generator,
      });
    } else {
      throw new Error('Invalid input, need an action or a generator to run.');
    }
  };
}

function runGeneratorInFlowWithHandler(args: {
  controller: ErrorDialogController;
  generator: GeneratorInput;
}) {
  return flow(function* runWithHandler() {
    try {
      const generator = typeof args.generator === 'function' ? args.generator() : args.generator;
      return yield* generator;
    } catch (e) {
      yield* handleError(args.controller)(e);
      throw e;
    }
  })();
}
