import 'url-search-params-polyfill';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import App from './App';
// import App from './App-ScreendefXml';
import './index.css';
import "./styles/screenComponents.scss";

import { configure } from 'mobx';
import { reactionRuntimeInfo } from './utils/reaction';

import {main} from './screenInterpreter/interpreter';

import "./menuInterpreter/interpreter";

configure({
  computedRequiresReaction: true,
  enforceActions: true,
  reactionScheduler(fn) {
    fn();
    reactionRuntimeInfo.clear();
  }
});


ReactDOM.render(
  <App />,
  document.getElementById('root') as HTMLElement
);

/*
main().then(reactTree => {
  ReactDOM.render(
    reactTree,
    document.getElementById('root') as HTMLElement
  );
})
*/