import React from "react";
import { observable, computed, action } from "mobx";
import { observer, Observer } from "mobx-react";
import { VBox } from "../../Components/ScreenElements/VBox";
import { HSplit, HSplitPanel } from "../../Components/ScreenElements/HSplit";
import { VSplitPanel, VSplit } from "../../Components/ScreenElements/VSplit";
import { Label } from "../../Components/ScreenElements/Label";
import { DataView } from "../../Components/ScreenElements/DataView";

import {
  TabbedPanel,
  TabHandle,
  TabBody
} from "../../Components/ScreenElements/TabbedPanel";
import { Box } from "../../Components/ScreenElements/Box";
import { FormView } from "./FormView/FormView";
import { FormRoot } from "../../Components/ScreenElements/FormRoot";
import { FormSection } from "../../Components/ScreenElements/FormSection";
import { Table } from "../../Components/ScreenElements/Table/Table";
import { IGridDimensions, IOrderByDirection } from "../../Components/ScreenElements/Table/types";
import { SimpleScrollState } from "../../Components/ScreenElements/Table/SimpleScrollState";
import bind from "bind-decorator";
import { Header } from "../../Components/ScreenElements/Table/Header"
import { CellRenderer } from "../../Components/ScreenElements/Table/CellRenderer";
import { findUIChildren, findBoxes, findUIRoot } from "../../../xmlInterpreters/screenXml";

class GridDimensions implements IGridDimensions {
  rowCount = 100;
  columnCount = 50;
  get contentWidth() {
    return this.getColumnRight(this.columnCount - 1);
  }

  get contentHeight() {
    return this.getRowBottom(this.rowCount - 1);
  }

  getColumnLeft(columnIndex: number): number {
    return columnIndex * 100;
  }

  getColumnWidth(columnIndex: number): number {
    return 100;
  }

  getColumnRight(columnIndex: number): number {
    return this.getColumnLeft(columnIndex) + this.getColumnWidth(columnIndex);
  }

  getRowTop(rowIndex: number): number {
    return rowIndex * 20;
  }

  getRowHeight(rowIndex: number): number {
    return 20;
  }

  getRowBottom(rowIndex: number): number {
    return this.getRowTop(rowIndex) + this.getRowHeight(rowIndex);
  }
}

class HeaderRenderer {
  @bind
  renderHeader(args: { columnIndex: number; columnWidth: number }) {
    return (
      <Header
        key={args.columnIndex}
        width={args.columnWidth}
        label={`Col ${args.columnIndex}`}
        orderingDirection={IOrderByDirection.NONE}
        orderingOrder={0}
      />
    );
  }
}

@observer
class TabbedPanelHelper extends React.Component<{
  boxes: any[];
  nextNode: (node: any) => React.ReactNode;
}> {
  @observable activePanelId: string =
    this.props.boxes.length > 0 ? this.props.boxes[0].attributes.Id : "";

  @action.bound activateTab(tabId: string) {
    this.activePanelId = tabId;
  }

  render() {
    const { boxes, nextNode } = this.props;
    return (
      <TabbedPanel
        handles={boxes.map(box => (
          <TabHandle
            isActive={this.activePanelId === box.attributes.Id}
            label={box.attributes.Name}
            onClick={() => this.activateTab(box.attributes.Id)}
          />
        ))}
      >
        {boxes.map(box => (
          <Observer>
            {() => (
              <TabBody
                isActive={computed(
                  () => this.activePanelId === box.attributes.Id
                ).get()}
              >
                {findUIChildren(box).map(child => nextNode(child))}
              </TabBody>
            )}
          </Observer>
        ))}
      </TabbedPanel>
    );
  }
}

export class TestTable extends React.Component {
  gDim = new GridDimensions();
  scrollState = new SimpleScrollState(0, 0);
  headerRenderer = new HeaderRenderer();
  cellRenderer = new CellRenderer();

  render() {
    const self = this;
    return (
      <Table
        gridDimensions={self.gDim}
        scrollState={self.scrollState}
        editingRowIndex={undefined}
        editingColumnIndex={undefined}
        isEditorMounted={false}
        fixedColumnCount={0}
        isLoading={false}
        renderHeader={self.headerRenderer.renderHeader}
        renderCell={self.cellRenderer.renderCell}
        renderEditor={() => null}
      />
    );
  }
}

@observer
export class FormScreenBuilder extends React.Component<{
  xmlWindowObject: any;
}> {
  buildScreen() {
    const self = this;
    function recursive(xso: any) {
      switch (xso.attributes.Type) {
        case "HSplit":
          return (
            <HSplit handleClassName="screenSplitterHandle">
              {findUIChildren(xso).map((child, idx) => (
                <HSplitPanel id={idx} key={idx}>
                  {recursive(child)}
                </HSplitPanel>
              ))}
            </HSplit>
          );
        case "VSplit":
          return (
            <VSplit handleClassName="screenSplitterHandle">
              {findUIChildren(xso).map((child, idx) => (
                <VSplitPanel id={idx} key={idx}>
                  {recursive(child)}
                </VSplitPanel>
              ))}
            </VSplit>
          );
        case "Label":
          console.log(xso);
          return (
            <Label
              height={parseInt(xso.attributes.Height, 10)}
              text={xso.attributes.Name}
            />
          );
        case "VBox":
          return (
            <VBox
              height={
                xso.attributes.Height
                  ? parseInt(xso.attributes.Height, 10)
                  : undefined
              }
            >
              {findUIChildren(xso).map(child => recursive(child))}
            </VBox>
          );
        case "Grid":
          return (
            <DataView
              height={
                xso.attributes.Height
                  ? parseInt(xso.attributes.Height, 10)
                  : undefined
              }
            >
              {/*DataView: {xso.attributes.Id}*/}
              {/*}
              <FormView>
                <FormRoot>
                  <FormSection
                    x={10}
                    y={30}
                    width={300}
                    height={120}
                    title="Testing section"
                  >
                    sdas
                  </FormSection>

                  <FormSection
                    x={1000}
                    y={30}
                    width={300}
                    height={120}
                    title="Testing section"
                  >
                    sdas
                  </FormSection>
                  
                </FormRoot>
            </FormView>*/}

              <TestTable />
            </DataView>
          );
        case "Tab":
          return (
            <TabbedPanelHelper boxes={findBoxes(xso)} nextNode={recursive} />
          );

        case "Box":
          return (
            <Box>{findUIChildren(xso).map(child => recursive(child))}</Box>
          );
        default:
          console.log("Unknown node:", xso);
          return null;
      }
    }

    const uiRoot = findUIRoot(this.props.xmlWindowObject);
    return recursive(uiRoot);
  }

  render() {
    return this.buildScreen();
  }
}
