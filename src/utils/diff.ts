import uuid from "uuid";

export function diff<T>(s0: Set<T>, s1: Set<T>) {
  return {
    del: new Set(Array.from(s0).filter(it => !s1.has(it))),
    add: new Set(Array.from(s1).filter(it => !s0.has(it))),
    keep: new Set(Array.from(s0).filter(it => s1.has(it)))
  };
}

const a1 = [];
for (let i = 0; i < 20; i++) {
  a1.push(uuid.v4());
}
const a2 = [...a1];

for (let i = 0; i < 3; i++) {
  a1.splice(Math.ceil(Math.random() * a1.length), 1);
  a2.splice(Math.ceil(Math.random() * a1.length), 1);
}

console.time("diff");
let dre;
for (let i = 0; i < 100; i++) {
  dre = diff(new Set(a1), new Set(a2));
}
console.timeEnd("diff");
console.log(dre);