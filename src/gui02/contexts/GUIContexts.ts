import React from "react";

export const CtxPanelVisibility = React.createContext<{ isVisible: boolean }>({
  isVisible: false
});
