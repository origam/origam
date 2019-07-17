import { getApplication } from './getApplication';

export function getDialogStack(ctx: any) {
  return getApplication(ctx).dialogStack;
}