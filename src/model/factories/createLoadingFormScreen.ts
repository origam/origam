
import { FormScreenLifecycle } from '../entities/FormScreenLifecycle';
import { LoadingFormScreen } from '../entities/FormScreen';

export function createLoadingFormScreen() {
  return new LoadingFormScreen({formScreenLifecycle: new FormScreenLifecycle()});
}