export function ValueBox<T>() {
  let _active = false;
  let _value: T | undefined;

  function get() {
    if (_active) return _value!;
    throw new Error("Value not set.");
  }

  function set(value: T) {
    if(_active) throw new Error("Value already set.");
    _value = value;
    _active = true;
  }

  function clear() {
    _value = undefined;
    _active = false;
  }

  get.get = get;
  get.set = set;
  get.clear = clear;

  return get;
}
