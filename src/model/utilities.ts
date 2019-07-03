export function getParentOrSelfOfType<T>(
  node: any,
  con: { new (...args: any[]): T }
): T {
  let current = node;
  while (current !== undefined) {
    if (current.constructor.name === con.name) {
      return current as T;
    }
    current = current.parent;
  }
  throw new Error("No such parent found.");
}
