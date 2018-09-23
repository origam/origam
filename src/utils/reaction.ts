import { observable, autorun, action, reaction } from "mobx";

export const reactionRuntimeInfo = new class ReactionRuntimeInfo {
  public info = new Set();

  public add(...info: any[]) {
    for (const infoItem of info) {
      this.info.add(infoItem);
    }
  }

  public clear() {
    this.info.clear();
  }
}();

/*
class OrderedReactionSlot {
  constructor(parent, order) {
    this.parent = parent;
    this.order = order;
  }

  scheduler({ order }) {
    if (!order) {
      order = 0;
    }
    return {
      scheduler: fn => {
        this.parent.schedule(this.order, order, fn);
      }
    };
  }

  autorun(effect, options) {
    if (!options) {
      options = {};
    }
    return autorun(effect, {...options, ...this.scheduler(options)})
  }

  reaction(data, effect, options) {
    if (!options) {
      options = {};
    }
    return reaction(data, effect, {...options, ...this.scheduler(options)})
  }  
}

export class OrderedEffects {
  @observable
  scheduledReactions = [];

  start() {
    autorun(() => {
      if (this.scheduledReactions.length === 0) return;
      const reactions = [...this.scheduledReactions];
      this.scheduledReactions.length = 0;
      reactions.sort((a, b) => {
        if (a.bigOrder > b.bigOrder) return 1;
        if (a.bigOrder < b.bigOrder) return -1;
        if (a.smallOrder > b.smallOrder) return 1;
        if (a.smallOrder < b.smallOrder) return -1;
        return 0;
      });
      for (let re of reactions) {
        re.fn();
      }
    });
  }

  newSlot({ order }) {
    if (!order) {
      order = 0;
    }
    return new OrderedReactionSlot(this, order);
  }

  @action.bound
  schedule(bigOrder, smallOrder, fn) {
    this.scheduledReactions.push({ bigOrder, smallOrder, fn });
  }
}
*/