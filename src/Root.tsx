import React from 'react';
import {Provider} from 'mobx-react';
import {CMain} from 'gui02/connections/CMain';
import {IApplication} from 'model/entities/types/IApplication';


export const Root: React.FC<{application: IApplication}> = (props) => (
  <Provider application={props.application}>
    {/*<Main />*/}
    <CMain />
  </Provider>
);