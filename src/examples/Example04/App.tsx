import axios from "axios";
import { observable } from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import { IScreen } from "src/presenter/types/IScreenPresenter";
import { Screen } from "../../presenter/ReactUI/screenElements/Screen";
import { parseScreenXml } from "src/common/ScreenXml";
import { collectElements } from "src/presenter/factory/screen/collect";
import { ScreenFactory } from "src/presenter/factory/screen/ScreenFactory";

/*
async function getScreen() {
  const response = await axios.get("/screen03.xml");
  const { data } = response;
  const scrObj = xmlJs.xml2js(data, {
    compact: false,
    alwaysChildren: true,
    addParent: true,
    alwaysArray: true
  });

  const modReprs = new Map();
  const modExhs = new Set();
  const modelParam = collectModelElements(scrObj, modReprs, modExhs);
  const model = createModel(modelParam);
  console.log(modelParam, model)

  const reprs = new Map();
  const exhs = new Set();
  const infReprs = new Map();
  const infExhs = new Set();
  const screenParam = collectScreenElements(
    scrObj,
    reprs,
    exhs,
    infReprs,
    infExhs
  );
  const presenterFactory = new PresenterFactory();
  const screen = presenterFactory.createScreen(model, screenParam);

  console.log(screenParam, screen);
  
  return screen;
}*/

@observer
class App extends React.Component {
  @observable screen: IScreen | undefined;

  async componentDidMount() {
    const resp = await axios.get("/screen03.xml");
    const screenXml = parseScreenXml(resp.data);
    console.log(screenXml);
    const reprs = new Map();
    const exhs = new Set();
    const infReprs = new Map();
    const infExhs = new Set();
    const elements = collectElements(screenXml, reprs, exhs, infReprs, infExhs);
    console.log(elements)
    const screenFactory = new ScreenFactory();
    const screen = screenFactory.getScreen(elements);
    console.log(screen)
    this.screen = screen;
  }

  public render() {
    return (
      <>
        <div
          className="App"
          style={{
            boxSizing: "border-box",
            // height: "800px",
            // width: "1100px",
            width: "100vw",
            height: "100vh",
            display: "flex",
            flexDirection: "row",
            overflow: "hidden",
            border: "1px solid black"
          }}
        >
          <div style={{ width: "100%", height: "100%" }}>
            {this.screen && <Screen controller={this.screen} />}
          </div>
        </div>
      </>
    );
  }
}

export default App;
