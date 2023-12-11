export class EventHandler {

  callBacks: (() => void)[] = [];

  add(callBack: () => void){
    if(!this.callBacks.includes(callBack)){
      this.callBacks.push(callBack);
    }
  }

  remove(callBack: () => void){
    this.callBacks.remove(callBack);
  }

  set(callBack: () => void){
    this.clear();
    this.add(callBack);
  }

  clear(){
    this.callBacks.length = 0;
  }

  call(){
    for (const callBack of this.callBacks) {
      callBack();
    }
  }
}