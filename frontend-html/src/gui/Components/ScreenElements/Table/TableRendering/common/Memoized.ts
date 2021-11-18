export function Memoized<T>(valueFn: () => T) {

  let _computed = false;
  let _value: T | undefined;

  function get(): T {
    if (_computed) return _value!;
    _computed = true;
    return (_value = valueFn());
  }

  function clear() {
    _value = undefined;
    _computed = false;
  }

  get.get = get;
  get.clear = clear;

  return get;
}
