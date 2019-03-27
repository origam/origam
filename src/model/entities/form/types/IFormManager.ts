export interface IFormManager {
  initFormIfNeeded(): void;
  destroyForm(): void;
  submitForm(): void;
}