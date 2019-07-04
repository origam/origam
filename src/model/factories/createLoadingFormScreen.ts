import { LoadingFormScreen } from '../FormScreen';
import { FormScreenLifecycle } from '../FormScreenLifecycle';

export function createLoadingFormScreen() {
  return new LoadingFormScreen({formScreenLifecycle: new FormScreenLifecycle()});
}