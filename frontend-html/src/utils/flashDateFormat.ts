export function flf2mof(flf: string) {
  return flf
    .replace(/YYYYY/g, "YYYY")
    .replace(/E/g, "d")
    .replace(/A/g, "a")
    .replace(/H/g, "k")
    .replace(/J/g, "H")
    .replace(/K/g, "h")
    .replace(/N/g, "m")
    .replace(/S/g, "s")
    .replace(/Q/g, "S");
}
