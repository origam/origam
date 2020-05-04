export enum ILifetime {
  PerDependency,
  PerLifetimeScope,
  Single,
}

class Registration<TInstance> {
  regCreator?: () => TInstance;
  regCreatorEx?: (container: Container) => TInstance;
  regClass?: new (...args: any[]) => TInstance;

  typeSymbol?: ITypeSymbol<TInstance>;
  instancePerDependency?: boolean;
  instancePerLifetimeScope?: boolean;
  instancePerNamedLifetimeScope?: string;
  singleInstance?: boolean;

  onPreparing: Array<(args: IOnPreparingArgs) => void> = [];
  onActivating: Array<(args: IOnActivatingArgs<TInstance>) => void> = [];
  onActivated: Array<(args: IOnActivatedArgs<TInstance>) => void> = [];
  onRelease: Array<() => void> = [];
}

class Registrator<TInstance> implements IRegistrator<TInstance> {
  constructor(private registration: Registration<TInstance>) {}

  transientInstance(): IRegistrator<TInstance> {
    this.registration.instancePerDependency = true;
    return this;
  }

  singleInstance(): IRegistrator<TInstance> {
    this.registration.singleInstance = true;
    return this;
  }

  scopedInstance(name?: string): IRegistrator<TInstance> {
    if (name) {
      this.registration.instancePerNamedLifetimeScope = name;
    } else {
      this.registration.instancePerLifetimeScope = true;
    }
    return this;
  }

  onActivated(handler: (args: IOnActivatedArgs<TInstance>) => void): IRegistrator<TInstance> {
    this.registration.onActivated.push(handler);
    return this;
  }

  onActivating(handler: (args: IOnActivatingArgs<TInstance>) => void): IRegistrator<TInstance> {
    this.registration.onActivating.push(handler);
    return this;
  }

  onPreparing(handler: (args: IOnPreparingArgs) => void): IRegistrator<TInstance> {
    this.registration.onPreparing.push(handler);
    return this;
  }

  onRelease(handler: () => void): IRegistrator<TInstance> {
    this.registration.onRelease.push(handler);
    return this;
  }
}

export class Container implements IContainer {
  constructor(private options: { defaultLifetime: ILifetime }) {}

  private registrations: Map<ITypeSymbol<any>, Array<Registration<any>>> = new Map();
  private instances: Map<ITypeSymbol<any>, any> = new Map();
  private transientInstances: any[] = [];
  private disposeEvents = new WeakMap<any, Array<() => void>>();

  private parent?: Container;
  private children: Container[] = [];
  private scopeName?: string;

  private isDisposed = false;

  scheduledOnActivated: Array<() => void> = [];

  register<TInstance>(
    sym: ITypeSymbol<TInstance>,
    creator?: () => TInstance,
    creatorEx?: (container: Container) => TInstance
  ): IRegistrator<TInstance> {
    const registration = new Registration<TInstance>();
    registration.typeSymbol = sym;
    registration.regCreator = creator;
    registration.regCreatorEx = creatorEx;
    if (!this.registrations.has(sym)) {
      this.registrations.set(sym, []);
    }
    const registrations = this.registrations.get(sym);
    if (registrations) registrations.push(registration);
    const registrator = new Registrator(registration);
    return registrator;
  }

  registerClass<TInstance>(
    sym: ITypeSymbol<TInstance>,
    regClass: new (...args: any[]) => TInstance
  ): IRegistrator<TInstance> {
    const registration = new Registration<TInstance>();
    registration.typeSymbol = sym;
    registration.regClass = regClass;
    if (!this.registrations.has(sym)) {
      this.registrations.set(sym, []);
    }
    const registrations = this.registrations.get(sym);
    if (registrations) registrations.push(registration);
    const registrator = new Registrator(registration);
    return registrator;
  }

  checkDisposed() {
    if (this.isDisposed) {
      debugger;
      throw new Error("Trying to resolve from a disposed container.");
    }
  }

  resolveAll<TInstance>(sym: ITypeSymbol<TInstance>): TInstance[] {
    this.checkDisposed();
    pushCurrentContainer(this);
    try {
      const registrations = this.findAllRegistrations(sym);
      return registrations.map((registration) => this.resolveByRegistration(registration));
    } finally {
      popCurrentContainer();
    }
  }

  resolve<TInstance>(sym: ITypeSymbol<TInstance>): TInstance {
    this.checkDisposed();
    pushCurrentContainer(this);
    try {
      const registration = this.findRegistration(sym);
      if (registration) {
        return this.resolveByRegistration(registration);
      } else {
        debugger;
        throw new Error("No registration for symbol " + sym.symName);
      }
    } finally {
      popCurrentContainer();
    }
  }

  resolveByRegistration<TInstance>(registration: Registration<TInstance>) {
    for (let h of registration.onPreparing) h({ container: this });
    if (registration.instancePerDependency) {
      return this.providePerDependency(registration);
    } else if (registration.singleInstance) {
      return this.provideSingle(registration);
    } else if (registration.instancePerLifetimeScope) {
      return this.providePerLifetimeScope(registration);
    } else if (registration.instancePerNamedLifetimeScope) {
      return this.providePerNamedLifetimeScope(registration);
    } else if (this.options.defaultLifetime === ILifetime.PerDependency) {
      return this.providePerDependency(registration);
    } else if (this.options.defaultLifetime === ILifetime.PerLifetimeScope) {
      return this.providePerLifetimeScope(registration);
    } else if (this.options.defaultLifetime === ILifetime.Single) {
      return this.provideSingle(registration);
    } else {
      // This branch never entered?
      return this.providePerDependency(registration);
    }
  }

  resolveFromCreator<TInstance>(creator: (...args: any[]) => TInstance) {
    this.checkDisposed();
    pushCurrentContainer(this);
    try {
      const instance = creator();
      _registeredScopes.set(instance, this);
      // this.transientInstances.push(instance);
      return creator();
    } finally {
      popCurrentContainer();
    }
  }

  newFromRegistration<TInstance>(registration: Registration<TInstance>) {
    // console.log("NewFromRegistration", this.scopeName, registration.typeSymbol);
    if (registration.regClass) {
      return new registration.regClass();
    }
    if (registration.regCreator) {
      return registration.regCreator();
    }
    if (registration.regCreatorEx) {
      return registration.regCreatorEx(this);
    }
    debugger;
    throw new Error("Neither class nor creator registered.");
  }

  providePerDependency<TInstance>(registration: Registration<TInstance>) {
    let instance = this.newFromRegistration(registration);
    _registeredScopes.set(instance, this);
    for (let h of registration.onActivating)
      h({
        container: this,
        instance,
        replaceInstance(newInstance) {
          instance = newInstance;
        },
      });
    for (let h of registration.onActivated) {
      getBottomContainer().scheduledOnActivated.push(() => h({ container: this, instance }));
    }
    // this.transientInstances.push(instance);
    if (registration.onRelease.length > 0) {
      this.disposeEvents.set(instance, registration.onRelease);
    }
    return instance;
  }

  provideSingle(registration: Registration<any>) {
    const container = this.findRootContainer();
    return this.provideFromContainer(container, registration);
  }

  providePerLifetimeScope(registration: Registration<any>) {
    const container = this;
    return this.provideFromContainer(container, registration);
  }

  providePerNamedLifetimeScope(registration: Registration<any>) {
    const container = this.findFirstNamedContainer(registration.instancePerNamedLifetimeScope!);
    if (container) {
      return this.provideFromContainer(container, registration);
    } else {
      throw new Error(`Scope named ${registration.instancePerNamedLifetimeScope} not opened.`);
    }
  }

  provideFromContainer<TInstance>(container: Container, registration: Registration<TInstance>) {
    // console.log("ProvideFromContainer", container.scopeName, registration.typeSymbol);
    if (container.instances.has(registration.typeSymbol!)) {
      return container.instances.get(registration.typeSymbol!)!;
    } else {
      let instance = this.newFromRegistration(registration);
      _registeredScopes.set(instance, this);
      for (let h of registration.onActivating)
        h({
          container: this,
          instance,
          replaceInstance(newInstance) {
            instance = newInstance;
          },
        });
      container.instances.set(registration.typeSymbol!, instance);
      for (let h of registration.onActivated) {
        getBottomContainer().scheduledOnActivated.push(() => h({ container: this, instance }));
      }
      return instance;
    }
  }

  resolveOptional<TInstance>(sym: ITypeSymbol<TInstance>): TInstance | undefined {
    throw new Error("Method not implemented.");
  }

  beginLifetimeScope(scopeName?: string): Container {
    //console.log("Entering lifetime scope:", scopeName);
    const container = new Container(this.options);
    container.scopeName = scopeName;
    container.parent = this;
    this.children.push(container);
    return container;
  }

  dispose() {
    // TODO: Dispose registered objects
    //console.log("Disposing lifetime scope:", this.scopeName);
    const parent = this.parent;
    if (this.children.length > 0) {
      console.log("Disposing container with children?");
    }
    for (let instance of this.instances.values()) {
      const disposeEvent = this.disposeEvents.get(instance);
      if (disposeEvent) {
        for (let h of disposeEvent) h();
      }
      console.log('Disposing cached', instance)
      if (instance.dispose) instance.dispose();
    }
    this.instances.clear();
    for (let instance of this.transientInstances) {
      console.log('Disposing transient', instance)
      if (instance.dispose) instance.dispose();
    }
    this.transientInstances.length = 0;
    this.isDisposed = true;
    if (this.parent) {
      const idx = this.parent.children.findIndex((item) => item === this);
      if (idx > -1) this.parent.children.splice(idx, 1);
      this.parent = undefined;
    }
    return parent;
  }

  disposeWithChildren() {
    for (let child of this.children) {
      child.disposeWithChildren();
    }
    this.dispose();
  }

  findAllRegistrations<TInstance>(sym: ITypeSymbol<TInstance>): Array<Registration<any>> {
    let registrations: Array<Registration<any>> = [];
    if (this.parent) {
      registrations.push(...this.parent.findAllRegistrations(sym));
    }
    if (this.registrations.has(sym)) {
      registrations.push(...this.registrations.get(sym)!);
    }
    return registrations;
  }

  findRegistration<TInstance>(sym: ITypeSymbol<TInstance>): Registration<any> | undefined {
    if (this.registrations.has(sym)) {
      return this.registrations.get(sym)!.slice(-1)[0];
    }
    if (this.parent) {
      return this.parent.findRegistration(sym);
    } else {
      return;
    }
  }

  findRootContainer(): Container {
    if (this.parent) return this.parent.findRootContainer();
    return this;
  }

  findFirstNamedContainer(name: string): Container | undefined {
    if (this.scopeName === name) return this;
    if (this.parent) return this.parent.findFirstNamedContainer(name);
    return;
  }

  resolveFlowFinished() {
    for (let h of this.scheduledOnActivated) h();
    this.scheduledOnActivated.length = 0;
  }
}

export interface IContainer {
  register<TInstance>(
    sym: ITypeSymbol<TInstance>,
    creator?: () => TInstance,
    creatorEx?: (container: Container) => TInstance
  ): IRegistrator<TInstance>;

  resolve<TInstance>(sym: ITypeSymbol<TInstance>): TInstance;
  resolveAll<TInstance>(sym: ITypeSymbol<TInstance>): TInstance[];
  resolveOptional<TInstance>(sym: ITypeSymbol<TInstance>): TInstance | undefined;
  resolveFromCreator<TInstance>(creator: (...args: any[]) => TInstance): TInstance;

  beginLifetimeScope(scopeName?: string): IContainer;
  dispose(): void;
}

export interface IRegistrator<TInstance> {
  transientInstance(): IRegistrator<TInstance>;
  singleInstance(): IRegistrator<TInstance>;
  scopedInstance(name?: string): IRegistrator<TInstance>;
  onActivated(handler: (args: IOnActivatedArgs<TInstance>) => void): IRegistrator<TInstance>;
  onActivating(handler: (args: IOnActivatingArgs<TInstance>) => void): IRegistrator<TInstance>;
  onPreparing(handler: (args: IOnPreparingArgs) => void): IRegistrator<TInstance>;
  onRelease(handler: () => void): IRegistrator<TInstance>;
}

export interface IOnPreparingArgs {
  container: IContainer;
}

export interface IOnActivatingArgs<TInstance> {
  container: IContainer;
  instance: TInstance;
  replaceInstance(newInstance: TInstance): void;
}

export interface IOnActivatedArgs<TInstance> {
  container: IContainer;
  instance: TInstance;
}

export interface ITypeSymbol<TInstance> {
  (): TInstance;
  symName: string;
  injectAsGetter?: boolean;
  injectAsCreator?: boolean;
  usingContainer?: Container;
  injectEnumeration?: boolean;
  enumerationCacheKey: any;
}

let _currentContainerStack: Container[] = new Array(10);
let _currentContainerStackTop = -1;

export function getTopContainer() {
  return _currentContainerStack[_currentContainerStackTop];
}

export function getBottomContainer() {
  return _currentContainerStack[0];
}

export function pushCurrentContainer(currentContainer: Container) {
  _currentContainerStackTop++;
  _currentContainerStack[_currentContainerStackTop] = currentContainer;
}

export function popCurrentContainer() {
  const container = _currentContainerStack[_currentContainerStackTop];
  _currentContainerStack[_currentContainerStackTop] = undefined!;
  _currentContainerStackTop--;
  if (_currentContainerStackTop === -1) {
    container.resolveFlowFinished();
  }
  return container;
}

export function TypeSymbol<TInstance>(symName: string): ITypeSymbol<TInstance> {
  const resolve = (): TInstance => {
    const container = getTopContainer();
    if (!container) throw new Error("No resolution flow active.");
    return container.resolve<TInstance>(resolve as any);
  };
  resolve.symName = symName;
  resolve.toString = () => `TypeSymbol <${symName}>`;
  resolve.enumerationCacheKey = {};
  return resolve;
}

export function Func<TInstance>(tys: ITypeSymbol<TInstance>) {
  const container = getTopContainer();
  if (!container) throw new Error("No resolution flow active.");
  const resolve = (): (() => TInstance) => {
    return () => container.resolve<TInstance>(tys as any);
  };
  resolve.symName = tys.symName;
  resolve.toString = () => `Func <${tys.toString()}>`;
  return resolve;
}

/*
export function Enumeration<TInstance>(tys: ITypeSymbol<TInstance>) {
  const resolve = (): TInstance[] => {
    const container = getTopContainer();
    if (!container) throw new Error("No resolution flow active.");
    return container.resolveAll(tys);
  };
  resolve.symName = tys.symName;
  resolve.toString = () => `Enumeration <${tys.toString()}>`;
  return resolve;
}*/

export function Getter<TInstance>(tys: ITypeSymbol<TInstance>) {
  tys.injectAsGetter = true;
  return tys;
}

export function InjectContainer() {
  const container = getTopContainer();
  if (!container) throw new Error("No resolution flow active.");
  return container;
}

const _registeredScopes = new WeakMap<any, Container>();
export function scopeFor(instance: any): Container | undefined {
  return _registeredScopes.get(instance);
}
