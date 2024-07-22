import { IArchitectApi } from "src/API/IArchitectApi.ts";
import React from "react";

export const ArchitectApiContext = React.createContext<IArchitectApi | null>(null);


interface ArchitectApiProviderProps {
  api: IArchitectApi;
  children: React.ReactNode;
}

export const ArchitectApiProvider: React.FC<ArchitectApiProviderProps> = ({ api, children }) => {
  return (
    <ArchitectApiContext.Provider value={api}>
      {children}
    </ArchitectApiContext.Provider>
  );
};