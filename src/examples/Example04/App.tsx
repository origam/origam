import axios from "axios";
import { observable } from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import { IScreen } from "src/presenter/types/IScreenPresenter";
import { Screen } from "../../presenter/ReactUI/screenElements/Screen";
import { parseScreenXml } from "src/common/ScreenXml";
import { buildScreen } from "src/Application";


@observer
class App extends React.Component {
  @observable screen: IScreen | undefined;

  async componentDidMount() {
    const resp = await axios.get("/screen03.xml");
    const screenXml = parseScreenXml(resp.data);
    
    this.screen = buildScreen(screenXml);
    
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
