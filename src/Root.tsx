import React from 'react';
import {Provider} from 'mobx-react';
import {CMain} from 'gui/connections/CMain';
import {IApplication} from 'model/entities/types/IApplication';


export const Root: React.FC<{application: IApplication}> = (props) => (
  <Provider application={props.application}>
    <CMain />
  </Provider>
);