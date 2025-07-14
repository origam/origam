/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import {
  Container,
  Func,
  getCreatorStack,
  getScopePath,
  ILifetime,
  IRegistrator,
  ITypeSymbol,
  popCurrentContainer,
  pushCurrentContainer,
  TypeSymbol,
} from "./Container";

interface IDog {
  dogName: string;
}

const IDog = TypeSymbol<IDog>("IDog");

class Dog implements IDog {
  dogName = "Jamie";
}

interface ICat {
  catName: string;
  garden: IGarden;
}

const ICat = TypeSymbol<ICat>("ICat");

class Cat implements ICat {
  constructor(public garden = IGarden()) {
  }

  catName = "Garfield";
}

interface IFish {
  fishName: string;
  garden: IGarden;
}

const IFish = TypeSymbol<IFish>("IFish");

class Fish implements IFish {
  constructor(public garden = IGarden()) {
  }

  fishName = "Nemo";
}

interface IGarden {
  size: number;
}

const IGarden = TypeSymbol<IGarden>("IGarden");

class Garden implements IGarden {
  size = 150;
}

interface IHouse {
  houseCity: string;
  dog: IDog;
  cat: ICat;
  fish: IFish;
}

const IHouse = TypeSymbol<IHouse>("IHouse");

class House implements IHouse {
  constructor(public dog = IDog(), public cat = ICat(), public fish = IFish()) {
  }

  houseCity = "Kladno";
}

class A {
  constructor(public b = IB(), public c = IC()) {
  }
}

const IA = TypeSymbol<A>("IA");

class B {
}

const IB = TypeSymbol<B>("IB");

class C {
}

const IC = TypeSymbol<C>("IC");

class X {
  constructor(public a = Func(IA)()) {
  }
}

const IX = TypeSymbol<X>("IX");

class PluginA {
  k = 0;
}

class PluginB {
  k = 1;
}

class PluginC {
  k = 2;
}

interface IPlugin {
  k: number;
}

const IPlugin = TypeSymbol<{ k: number }>("IPlugin");
const IPluginA = TypeSymbol<any>("IPluginA");
const IPluginB = TypeSymbol<any>("IPluginB");
const IPluginC = TypeSymbol<any>("IPluginC");

const IPluginEnumeration = TypeSymbol<IPlugin[]>("IPluginEnumeration");

class PluginConsumer {
  constructor(public plugins = IPluginEnumeration()) {
  }
}

const IPluginConsumer = TypeSymbol<PluginConsumer>("IPluginConsumer");

function createContainerTransient() {
  const container = new Container({defaultLifetime: ILifetime.PerDependency});
  container.registerClass(IDog, Dog);
  container.registerClass(ICat, Cat);
  container.registerClass(IHouse, House);
  container.registerClass(IGarden, Garden);
  container.registerClass(IFish, Fish);
  return container;
}

function createContainerSingle() {
  const container = new Container({defaultLifetime: ILifetime.Single});
  container.registerClass(IDog, Dog);
  container.registerClass(ICat, Cat);
  container.registerClass(IHouse, House);
  container.register(IGarden, () => new Garden());
  container.register(IFish, () => new Fish());
  return container;
}

function createContainerScoped() {
  const container = new Container({defaultLifetime: ILifetime.PerLifetimeScope});
  container.registerClass(IDog, Dog);
  return container;
}

function createContainerSingleWithScopedRegistration() {
  const container = new Container({defaultLifetime: ILifetime.Single});
  container.registerClass(IDog, Dog).scopedInstance("myScope");
  container.registerClass(ICat, Cat);
  container.registerClass(IHouse, House);
  container.registerClass(IGarden, Garden);
  container.registerClass(IFish, Fish);
  return container;
}

describe("Container", () => {
  it("provides transient instances", () => {
    const container = createContainerTransient();

    const dogs = [container.resolve(IDog), container.resolve(IDog), container.resolve(IDog)];
    expect(dogs[0].dogName).toBe("Jamie");
    expect(dogs[1].dogName).toBe("Jamie");
    expect(dogs[2].dogName).toBe("Jamie");
    expect(dogs[0]).not.toBe(dogs[1]);
    expect(dogs[0]).not.toBe(dogs[2]);
  });

  it("provides single instances", () => {
    const container1 = createContainerSingle();
    const dogs = [container1.resolve(IDog), container1.resolve(IDog)];
    const container2 = container1.beginLifetimeScope();
    dogs.push(container2.resolve(IDog), container2.resolve(IDog));

    expect(dogs[0].dogName).toBe("Jamie");
    expect(dogs[1].dogName).toBe("Jamie");
    expect(dogs[2].dogName).toBe("Jamie");
    expect(dogs[3].dogName).toBe("Jamie");
    expect(dogs[0]).toBe(dogs[1]);
    expect(dogs[0]).toBe(dogs[2]);
    expect(dogs[0]).toBe(dogs[3]);
  });

  it("provides scoped instances", () => {
    const container1 = createContainerScoped();
    const dogs = [container1.resolve(IDog), container1.resolve(IDog)];
    const container2 = container1.beginLifetimeScope();
    dogs.push(container2.resolve(IDog), container2.resolve(IDog));

    expect(dogs[0].dogName).toBe("Jamie");
    expect(dogs[1].dogName).toBe("Jamie");
    expect(dogs[2].dogName).toBe("Jamie");
    expect(dogs[3].dogName).toBe("Jamie");
    expect(dogs[0]).toBe(dogs[1]);
    expect(dogs[2]).toBe(dogs[3]);
    expect(dogs[0]).not.toBe(dogs[2]);
  });

  it("provides instances scoped by scope name", () => {
    const container1 = createContainerSingleWithScopedRegistration();
    const container2 = container1.beginLifetimeScope();
    const container3 = container2.beginLifetimeScope("myScope");
    const container4 = container3.beginLifetimeScope();

    const d = container4.resolve(IDog);

    expect(() => container1.resolve(IDog)).toThrow();
    expect(() => container2.resolve(IDog)).toThrow();

    const dogs = [
      container3.resolve(IDog),
      container3.resolve(IDog),
      container4.resolve(IDog),
      container4.resolve(IDog),
    ];

    expect(dogs[0].dogName).toBe("Jamie");
    expect(dogs[1].dogName).toBe("Jamie");
    expect(dogs[2].dogName).toBe("Jamie");
    expect(dogs[3].dogName).toBe("Jamie");

    expect(dogs[0]).toBe(dogs[1]);
    expect(dogs[0]).toBe(dogs[2]);
    expect(dogs[0]).toBe(dogs[3]);
  });

  it("allows obtaining current scope path", () => {
    class K {
      constructor() {
        expect(getScopePath()).toEqual([c4, c3, c2, c1]);
      }
    }

    const IK = TypeSymbol<K>("IK");

    const c1 = new Container({defaultLifetime: ILifetime.Single});
    c1.registerClass(IK, K);
    const c2 = c1.beginLifetimeScope("l1");
    const c3 = c2.beginLifetimeScope("l2");
    const c4 = c3.beginLifetimeScope("l3");
    c4.resolve(IK);
  });

  it("resolves instances by calling their symbol", () => {
    const container = createContainerTransient();
    pushCurrentContainer(container);

    const dogs = [IDog(), IDog(), IDog()];
    expect(dogs[0].dogName).toBe("Jamie");
    expect(dogs[1].dogName).toBe("Jamie");
    expect(dogs[2].dogName).toBe("Jamie");
    expect(dogs[0]).not.toBe(dogs[1]);
    expect(dogs[0]).not.toBe(dogs[2]);

    popCurrentContainer();
  });

  it("recursively resolves dependencies in single scope", () => {
    const container = createContainerSingle();
    const house = container.resolve(IHouse);

    expect(house.houseCity).toBe("Kladno");
    expect(house.dog.dogName).toBe("Jamie");
    expect(house.cat.catName).toBe("Garfield");
    expect(house.fish.fishName).toBe("Nemo");
    expect(house.cat.garden.size).toBe(150);
    expect(house.fish.garden.size).toBe(150);
    expect(house.fish.garden).toBe(house.cat.garden);
  });

  it("recursively resolves dependencies in transient scope", () => {
    const container = createContainerTransient();
    const house = container.resolve(IHouse);

    expect(house.houseCity).toBe("Kladno");
    expect(house.dog.dogName).toBe("Jamie");
    expect(house.cat.catName).toBe("Garfield");
    expect(house.fish.fishName).toBe("Nemo");
    expect(house.cat.garden.size).toBe(150);
    expect(house.fish.garden.size).toBe(150);
    expect(house.fish.garden).not.toBe(house.cat.garden);
  });

  it("resolves can resolve all instances for all registrations of one symbol", () => {
    const container = new Container({defaultLifetime: ILifetime.Single});
    container.registerClass(IPluginConsumer, PluginConsumer);
    container.register(IPluginEnumeration, undefined, (cont) => cont.resolveAll(IPlugin));
    container.registerClass(IPlugin, PluginA).transientInstance();
    container.registerClass(IPlugin, PluginB).transientInstance();
    container.registerClass(IPlugin, PluginC).transientInstance();

    const pc = container.resolve(IPluginConsumer);

    expect(pc.plugins[0].k).toBe(0);
    expect(pc.plugins[1].k).toBe(1);
    expect(pc.plugins[2].k).toBe(2);
  });

  it("resolves can resolve all instances for all registrations of one symbol - allows symbol redirect", () => {
    const container = new Container({defaultLifetime: ILifetime.Single});
    container.registerClass(IPluginConsumer, PluginConsumer);
    container.register(IPluginEnumeration, undefined, (cont) => cont.resolveAll(IPlugin));
    container.register(IPlugin, IPluginA).transientInstance();
    container.register(IPlugin, IPluginB).transientInstance();
    container.register(IPlugin, IPluginC).transientInstance();

    container.registerClass(IPluginA, PluginA);
    container.registerClass(IPluginB, PluginB);
    container.registerClass(IPluginC, PluginC);

    const pc = container.resolve(IPluginConsumer);

    expect(pc.plugins[0].k).toBe(0);
    expect(pc.plugins[1].k).toBe(1);
    expect(pc.plugins[2].k).toBe(2);
  });

  it("allows to inject lazy object creators", () => {
    const container = new Container({defaultLifetime: ILifetime.Single});
    const callOrder: any[] = [];

    container
      .register(IA, () => {
        callOrder.push("CA");
        return new A();
      })
      .onActivated(() => {
        callOrder.push("AA");
      });
    container
      .register(IB, () => {
        callOrder.push("CB");
        return new B();
      })
      .onActivated(() => {
        callOrder.push("AB");
      });
    container
      .register(IC, () => {
        callOrder.push("CC");
        return new C();
      })
      .onActivated(() => {
        callOrder.push("AC");
      });

    container
      .register(IX, () => {
        callOrder.push("CX");
        return new X();
      })
      .onActivated(() => {
        callOrder.push("AX");
      });
    const x = container.resolve(IX);

    expect(callOrder).toEqual(["CX", "AX"]);
    x.a();
    expect(callOrder).toEqual(["CX", "AX", "CA", "CB", "CC", "AB", "AC", "AA"]);
    x.a();
    expect(callOrder).toEqual(["CX", "AX", "CA", "CB", "CC", "AB", "AC", "AA"]);
  });

  it("passes given args to func creator", () => {
    class X {
      constructor(public y = Func(IY)()) {
      }
    }

    class Y {
      constructor(public k: number, public l: number) {
      }
    }

    const IX = TypeSymbol<X>("IX");
    const IY = TypeSymbol<Y>("IY");

    const $cont = new Container({defaultLifetime: ILifetime.PerLifetimeScope});
    $cont.register(IY, (k: number, l: number) => new Y(k, l));
    $cont.registerClass(IX, X);

    const x = $cont.resolve(IX);
    const y = x.y(10, 13);
    expect(y.k).toBe(10);
    expect(y.l).toBe(13);
  });

  it("holds instance created by automatic factory with arguments", () => {
    class X {
      constructor(public y = Func(IY)()) {
      }
    }

    class Y {
      constructor(public k: number, public l: number, public z = IZ()) {
      }
    }

    class Z {
      constructor(public y = Func(IY)()) {
      }
    }

    const IX = TypeSymbol<X>("IX");
    const IY = TypeSymbol<Y>("IY");
    const IZ = TypeSymbol<Z>("IZ");

    const $cont = new Container({defaultLifetime: ILifetime.PerLifetimeScope});
    $cont.register(IY, (k: number, l: number) => new Y(k, l));
    $cont.registerClass(IX, X);
    $cont.registerClass(IZ, Z);

    const x = $cont.resolve(IX);
    const y = x.y(10, 13);
    expect(y.z.y().k).toBe(10);
    expect(y.z.y().l).toBe(13);
  });

  it("allows for using factories which open new lifetime scopes.", () => {
    class X {
      constructor(public y = INewY()) {
      }
    }

    class Y {
      constructor(public k: number, public l: number, public z = IZ()) {
      }
    }

    class Z {
      constructor(public y = Func(IY)()) {
      }
    }

    function NewY($cont: Container) {
      return (k: number, l: number) => $cont.beginLifetimeScope("YScope").resolve(IY, k, l);
    }

    const IX = TypeSymbol<X>("IX");
    const IY = TypeSymbol<Y>("IY");
    const INewY = TypeSymbol<(k: number, l: number) => Y>("INewY");
    const IZ = TypeSymbol<Z>("IZ");

    const $cont = new Container({defaultLifetime: ILifetime.PerLifetimeScope});
    $cont.registerClass(IZ, Z).scopedInstance("YScope");
    $cont.registerClass(IY, Y);
    $cont.register(INewY, undefined, NewY);
    $cont.registerClass(IX, X);

    const x = $cont.resolve(IX);
    const y1 = x.y(10, 13);
    expect(y1.z.y().k).toBe(10);
    expect(y1.z.y().l).toBe(13);

    const y2 = x.y(19, 17);
    expect(y2.z.y().k).toBe(19);
    expect(y2.z.y().l).toBe(17);
  });

  it("can switch scopes by a function provided during registration.", () => {
    class X {
      constructor() {
      }
    }

    const IX1 = TypeSymbol<X>("IX1");
    const IX2 = TypeSymbol<X>("IX2");

    const $cont = new Container({defaultLifetime: ILifetime.PerLifetimeScope});

    function topmostScope(scopeName: string) {
      return ($cont: Container) => {
        const scopePath = getScopePath($cont);
        scopePath.reverse();
        return scopePath.find((item) => item.scopeName === scopeName);
      };
    }

    const x1 = new X();
    const x2 = new X();

    $cont.register(IX1, () => x1).scopedInstance("Scope1");
    // IX2 resolves in topmost Scope1 regardless how many times the scope is nested
    $cont.register(IX2, () => x2).scopedInstance(topmostScope("Scope1"));

    const $c1 = $cont.beginLifetimeScope("Scope1");
    const $c2 = $c1.beginLifetimeScope("Scope1");
    const $c3 = $c2.beginLifetimeScope("Scope1");

    const rx1a = $c3.resolve(IX1);
    const rx1b = $c3.resolve(IX1);
    const rx2a = $c3.resolve(IX2);
    const rx2b = $c3.resolve(IX2);
    expect(rx1a).not.toBe(rx2a);
    expect(rx1a).toBe(rx1b);
    expect(rx2a).toBe(rx2b);
    expect($c1.instances.get(IX2)).toBe(rx2a);
    expect($c3.instances.get(IX1)).toBe(rx1a);
  });
});

describe("Container lifecycle event", () => {
  it("runs onPreparing before resolving the instance", () => {
    const container = new Container({defaultLifetime: ILifetime.PerDependency});
    const callOrder: any[] = [];

    container
      .register(IA, () => {
        callOrder.push("CA");
        return new A();
      })
      .onPreparing(() => {
        callOrder.push("PA");
      });
    container
      .register(IB, () => {
        callOrder.push("CB");
        return new B();
      })
      .onPreparing(() => {
        callOrder.push("PB");
      });
    container.register(IC, () => new C());

    container.resolve(IA);

    expect(callOrder).toEqual(["PA", "CA", "PB", "CB"]);
  });

  it("runs onActivating right after the instance has been resolved", () => {
    const container = new Container({defaultLifetime: ILifetime.PerDependency});
    const callOrder: any[] = [];

    container
      .register(IA, () => {
        callOrder.push("CA");
        return new A();
      })
      .onActivating(() => {
        callOrder.push("AA");
      });
    container
      .register(IB, () => {
        callOrder.push("CB");
        return new B();
      })
      .onActivating(() => {
        callOrder.push("AB");
      });
    container
      .register(IC, () => {
        callOrder.push("CC");
        return new C();
      })
      .onActivating(() => {
        callOrder.push("AC");
      });

    container.resolve(IA);

    expect(callOrder).toEqual(["CA", "CB", "AB", "CC", "AC", "AA"]);
  });

  it("runs onActivated after the whole resolution flow has finished.", () => {
    const container = new Container({defaultLifetime: ILifetime.PerDependency});
    const callOrder: any[] = [];

    container
      .register(IA, () => {
        callOrder.push("CA");
        return new A();
      })
      .onActivated(() => {
        callOrder.push("AA");
      });
    container
      .register(IB, () => {
        callOrder.push("CB");
        return new B();
      })
      .onActivated(() => {
        callOrder.push("AB");
      });
    container
      .register(IC, () => {
        callOrder.push("CC");
        return new C();
      })
      .onActivated(() => {
        callOrder.push("AC");
      });

    container.resolve(IA);

    expect(callOrder).toEqual(["CA", "CB", "CC", "AB", "AC", "AA"]);
  });

  it("runs onRelease when scope gets disposed.", () => {
    class K {
      id = "k";

      constructor(public l = IL(), public m = IM()) {
      }
    }

    const IK = TypeSymbol<K>("IK");

    class L {
      id = "l";

      constructor(m = IM()) {
      }
    }

    const IL = TypeSymbol<K>("IL");

    class M {
      id = "m";
    }

    const IM = TypeSymbol<K>("IM");
    const callOrder: any = [];

    const cont = new Container({defaultLifetime: ILifetime.PerLifetimeScope});
    cont.registerClass(IK, K).onRelease(({instance}) => {
      callOrder.push(instance.id);
    });
    cont.registerClass(IL, L).onRelease(({instance}) => {
      callOrder.push(instance.id);
    });
    cont.registerClass(IM, M).onRelease(({instance}) => {
      callOrder.push(instance.id);
    });
    cont.resolve(IK);

    cont.disposeWithChildren();

    expect(callOrder).toEqual(["m", "l", "k"]);
  });

  it("allows maintaining reference to an instance in some collection.", () => {
    let idgen = 1;

    class K {
      id = idgen++;

      constructor() {
      }
    }

    class KCollection {
      items = new Map<number, K>();

      put(k: K) {
        this.items.set(k.id, k);
      }

      del(k: K) {
        this.items.delete(k.id);
      }
    }

    const IKCollection = TypeSymbol<KCollection>("IKCollection");
    const IK = TypeSymbol<K>("IK");

    interface ICollection<TInstance> {
      put(obj: TInstance): void;

      del(obj: TInstance): void;
    }

    function maintainInCollection<TInstance>(collectionSym: ITypeSymbol<ICollection<TInstance>>) {
      return (reg: IRegistrator<TInstance>) => {
        return reg
          .onActivating(({instance, container}) => {
            collectionSym().put(instance);
          })
          .onRelease(({instance}) => {
            collectionSym().del(instance);
          });
      };
    }

    const cont = new Container({defaultLifetime: ILifetime.PerLifetimeScope});
    cont.registerClass(IKCollection, KCollection).singleInstance();
    const chcont = cont.beginLifetimeScope();
    chcont.registerClass(IK, K).transientInstance().forward(maintainInCollection(IKCollection));

    const col = cont.resolve(IKCollection);
    expect(col.items.size).toBe(0);

    chcont.resolve(IK);
    chcont.resolve(IK);
    chcont.resolve(IK);

    expect(col.items.size).toBe(3);
    expect(col.items.get(1)!.id).toBe(1);
    expect(col.items.get(2)!.id).toBe(2);
    expect(col.items.get(3)!.id).toBe(3);

    chcont.disposeWithChildren();
    expect(col.items.size).toBe(0);
  });
});

describe("Creator stack", () => {
  it("keeps track of creators to be used for current dependency chain", () => {
    class A {
      constructor(public b = IB()) {
      }
    }

    class B {
      constructor(public c = IC()) {
      }
    }

    class C {
      constructor() {
        expect(getCreatorStack()).toEqual([C, B, A]);
      }
    }

    const IA = TypeSymbol<A>("IA");
    const IB = TypeSymbol<B>("IB");
    const IC = TypeSymbol<C>("IC");

    const cont = new Container({defaultLifetime: ILifetime.PerLifetimeScope});
    cont.registerClass(IA, A);
    cont.registerClass(IB, B);
    cont.registerClass(IC, C);
    cont.resolve(IA);
  });

  it("allows to supply different implementations based on creator stack content", () => {
    class A {
      constructor(public b = IB(), public c = IC(), public d = ID()) {
      }

      v = "a";
    }

    class B {
      constructor(public c = IC()) {
      }

      v = "b";
    }

    class C1 {
      constructor(public c = IC()) {
      }

      v = "c1";
    }

    class C2 {
      v = "c2";
    }

    class D {
      v = "d";
    }

    const IA = TypeSymbol<A>("IA");
    const IB = TypeSymbol<B>("IB");
    const IC = TypeSymbol<any>("IC");
    const IC1 = TypeSymbol<C1>("IC1");
    const IC2 = TypeSymbol<C2>("IC1");
    const ID = TypeSymbol<D>("ID");

    const cont = new Container({defaultLifetime: ILifetime.PerLifetimeScope});
    cont.registerClass(IA, A);
    cont.registerClass(IB, B);
    cont
      .register(IC, () => {
        const consumer = getCreatorStack()[1];
        if (consumer === A) return IC1();
        if (consumer === B) return IC2();
        return ID();
      })
      .transientInstance();
    cont.registerClass(IC1, C1);
    cont.registerClass(IC2, C2);
    cont.registerClass(ID, D);
    const a = cont.resolve(IA);

    expect(a.v).toBe("a");
    expect(a.b.v).toBe("b");
    expect(a.b.c.v).toBe("c2");
    expect(a.c.v).toBe("c1");
    expect(a.c.c.v).toBe("d");
    expect(a.d.v).toBe("d");
  });
});
