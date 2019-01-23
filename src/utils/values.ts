export function text2bool(t: string) {
  if (t === "true") {
    return true;
  }
  if (t === "false") {
    return false;
  }
  throw new Error(`Unknown boolean variable ${t}`);
}