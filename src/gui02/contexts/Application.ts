import React from 'react';
import { IApplication } from 'model/entities/types/IApplication';

export const CtxApplication = React.createContext<IApplication | undefined>(undefined);