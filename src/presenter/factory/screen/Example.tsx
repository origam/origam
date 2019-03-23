import axios from "axios";
import { parseScreenXml } from "src/common/ScreenXml";
import { collectElements } from "./collect";
import { ScreenFactory } from "./ScreenFactory";

export async function exampleScreenFactory() {
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
}

