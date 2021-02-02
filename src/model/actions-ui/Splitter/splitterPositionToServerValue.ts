
export function splitterPositionToRatio(position: number) {
  return Math.round(position / window.screen.height * 1000_000);
}

export function splitterPositionFromRatio(serverStoredValue: number | undefined){
  if(!serverStoredValue){
    return serverStoredValue;
  }
  const isScreenSizePositionRatio = serverStoredValue > 1000; // this will work up to 8k 
  if(isScreenSizePositionRatio){
    return Math.round(serverStoredValue * window.screen.height / 1000_000) 
  }
  else
  {
    return undefined;
  }
}
