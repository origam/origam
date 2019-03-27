#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.Schema.WorkflowModel;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace Origam.Server.Doc
{
    public class DocStateWorkflow : AbstractDoc
    {
        public DocStateWorkflow(XmlWriter writer)
            : base(writer)
        {
        }

        public override string FilterName
        {
            get { return "stateWorkflow"; }
        }

        public override string Name
        {
            get { return "State Workflows"; }
        }

        public override bool UseDiagrams
        {
            get
            {
                return true;
            }
        }

        public override DiagramConnectorType ConnectorType
        {
            get
            {
                return DiagramConnectorType.StateMachine;
            }
        }

        public override ISchemaItemProvider RootProvider()
        {
            SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            return ss.GetProvider(typeof(StateMachineSchemaItemProvider));
        }

        public override List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            if (!CheckElementId(writer, elementId)) return null;

            StateMachine sm = ps.SchemaProvider.RetrieveInstance(typeof(StateMachine), new ModelElementKey(new Guid(elementId))) as StateMachine;
            List<DiagramConnection> connections = new List<DiagramConnection>();
            List<DiagramElement> elements = new List<DiagramElement>();
            if (sm.Field != null && sm.DynamicStatesLookup == null)
            {
                DiagramElements(sm, elements, connections);
            }
            #region body
            DocTools.WriteStartBody(bodyElement, writer, this.Title(elementId, documentation, ps), "State Workflow", DocTools.ImagePath(sm), sm.Id);
            // summary
            WriteSummary(writer, documentation, sm);
            // Ancestors
            WriteAncestors(writer, sm);
            // Packages
            WritePackages(writer, sm);
            // Diagram
            WriteDiagram(writer, elements);
            // dynamic states
            WriteDynamicStates(writer, sm);
            // event handlers
            if (sm.Events.Count > 0)
            {
                WriteEventHandlers(writer, sm);
            }
            // end body
            writer.WriteEndElement();
            #endregion
            return connections;
        }

        private void WriteEventHandlers(XmlWriter writer, StateMachine sm)
        {
            List<string> columnNames = new List<string>();
            columnNames.Add("Name");
            columnNames.Add("Sequential Workflow");
            columnNames.Add("Conditions");

            DocTools.WriteSectionStart(writer, "Event Handlers");
            writer.WriteStartElement("table");
            DocTools.WriteTableHeader(writer, columnNames);
            foreach (StateMachineEvent ev in sm.Events)
            {
                #region tr
                writer.WriteStartElement("tr");
                #region td name
                writer.WriteStartElement("td");
                writer.WriteAttributeString("class", "title");
                writer.WriteString(ev.Name);
                writer.WriteEndElement();
                #endregion
                #region td workflow
                writer.WriteStartElement("td");
                // name
                writer.WriteStartElement("p");
                DocTools.WriteElementLink(writer, ev.Action, new DocSequentialWorkflow(null).FilterName);
                writer.WriteEndElement();
                // long description p
                writer.WriteElementString("p", "Parameters:");
                writer.WriteStartElement("ul");
                foreach (StateMachineEventParameterMapping param in ev.ParameterMappings)
                {
                    writer.WriteElementString("li",
                        (param.Field == null ? "Complete row" : param.Field.Name)
                        + (param.Type == WorkflowEntityParameterMappingType.Original ? " (original value)" : "")
                        + " -> "
                        + param.ContextStore.Name
                        );
                }
                // end ul
                writer.WriteEndElement();
                // end td
                writer.WriteEndElement();
                #endregion
                #region td conditions
                writer.WriteStartElement("td");
                writer.WriteElementString("p", ev.Type.ToString());
                if (ev.OldState != null)
                {
                    writer.WriteElementString("p", "Old State: " + StateName(ev.OldState, sm));
                }
                if (ev.NewState != null)
                {
                    writer.WriteElementString("p", "New State: " + StateName(ev.NewState, sm));
                }
                if (ev.FieldDependencies.Count > 0)
                {
                    StringBuilder fields = new StringBuilder();
                    int i = 0;
                    foreach (StateMachineEventFieldDependency dependency in ev.FieldDependencies)
                    {
                        if (i > 0)
                        {
                            fields.Append(", ");
                        }
                        fields.Append(dependency.Field.Name);
                        i++;
                    }
                    writer.WriteElementString("p", "On change of fields: " + fields.ToString());
                }
                if (!string.IsNullOrEmpty(ev.Roles))
                {
                    writer.WriteElementString("p", "Roles: " + ev.Roles);
                }
                if (!string.IsNullOrEmpty(ev.Features))
                {
                    writer.WriteElementString("p", "Features: " + ev.Features);
                }
                writer.WriteEndElement();
                #endregion
                // tr
                writer.WriteEndElement();
                #endregion
            }
            // end table
            writer.WriteEndElement();
            // end Events section
            writer.WriteEndElement();
        }

        private void WriteDynamicStates(XmlWriter writer, StateMachine sm)
        {
            if (sm.DynamicStatesLookup != null)
            {
                DocTools.WriteSectionStart(writer, "Dynamic States");
                writer.WriteStartElement("p");
                writer.WriteString("Dynamic States ");
                DocTools.WriteElementLink(writer, sm.DynamicStatesLookup.ListDataStructure, new DocDataStructure(null).FilterName);
                writer.WriteEndElement();
                writer.WriteStartElement("p");
                writer.WriteString("Dynamic Operations ");
                DocTools.WriteElementLink(writer, sm.DynamicOperationsLookup.ListDataStructure, new DocDataStructure(null).FilterName);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        private void DiagramElements(StateMachine sm, List<DiagramElement> elements, List<DiagramConnection> connections)
        {
            DocEntity docEntity = new DocEntity(null);

            int lastTop = 0;
            elements.Add(new DiagramElement(sm.Id.ToString(), 
                (sm.Entity.Caption == "" ? sm.Entity.Name : sm.Entity.Caption) 
                + " – " 
                + (sm.Field.Caption == "" ? sm.Field.Name : sm.Field.Caption),
                null, this.FilterName, lastTop, 0, "stateMachine"));
            List<StateMachineState> addedStates = new List<StateMachineState>();
            lastTop = AddStateToDiagram(sm, elements, connections, docEntity, lastTop + 200, 0, addedStates, sm);

            Dictionary<int, List<DiagramElement>> dict = GroupDiagramLevels(elements);

            // there is a 200 px vertical space between items which is nice for complex diagrams
            // but if there is just 1 item on the same row it takes too much space so we
            // will move all items 100 px up if there is just 1 state on the level
            int maxItemsOnRow = 0;
            int offset = 0;
            foreach (KeyValuePair<int, List<DiagramElement>> item in dict)
            {
                if (item.Value.Count > maxItemsOnRow)
                {
                    maxItemsOnRow = item.Value.Count;
                }

                if (item.Value.Count > 1)
                {
                    foreach (DiagramElement child in item.Value)
                    {
                        child.Top -= offset;
                    }
                }
                DiagramElement firstItem = item.Value[0];
                if (item.Value.Count == 1 && firstItem.Top != 0)
                {
                    List<DiagramConnection> outgoingConnections = OutgoingConnections(connections, firstItem);
                    if (outgoingConnections.Count == 0
                        || (outgoingConnections.Count == 1 &&
                        GetElement(elements, outgoingConnections[0].Target).Top >= firstItem.Top))
                    {
                        // we move the item up only if there is 1 outgoing connection
                        // and it is not a backwards connection at the same time
                        offset += 100;
                    }
                    firstItem.Top -= offset;
                }
            }

            CenterDiagram(dict, maxItemsOnRow, 800);
        }

        private static void CenterDiagram(Dictionary<int, List<DiagramElement>> dict, int maxItemsOnRow, int minWidth)
        {
            // center the state diagram by the most number of items e.g.
            //               [ state 1 ]
            //      [ state 2 ]       [ state 3 ]
            //                [ final ]

            int width = maxItemsOnRow * 300;
            if (width < minWidth) width = minWidth;
            foreach (KeyValuePair<int, List<DiagramElement>> item in dict)
            {
                int localOffset = (width / 2) - (item.Value.Count * 150);
                foreach (DiagramElement child in item.Value)
                {
                    child.Left += localOffset;

                    if (child.Width == 0)
                    {
                        Font font = new Font("Arial", 12, FontStyle.Regular, GraphicsUnit.Pixel);
                        Size textSize = TextRenderer.MeasureText(child.Label, font);
                        int itemWidth = textSize.Width;
                        child.Left -= (itemWidth / 2);
                    }
                }
            }
        }

        private static DiagramElement GetElement(List<DiagramElement> list, string id)
        {
            foreach (DiagramElement item in list)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }

        private static List<DiagramConnection> OutgoingConnections(List<DiagramConnection> connections, DiagramElement firstItem)
        {
            List<DiagramConnection> result = new List<DiagramConnection>();
            foreach (DiagramConnection connection in connections)
            {
                if (connection.Source == firstItem.Id.ToString())
                {
                    result.Add(connection);
                }
            }
            return result;
        }
        
        private static int IncomingConnections(List<DiagramConnection> connections, DiagramElement firstItem)
        {
            int incomingConnections = 0;
            foreach (DiagramConnection connection in connections)
            {
                if (connection.Target == firstItem.Id.ToString())
                {
                    incomingConnections++;
                }
            }
            return incomingConnections;
        }

        private static int AddStateToDiagram(AbstractSchemaItem parent, List<DiagramElement> elements,
            List<DiagramConnection> connections, DocEntity docEntity, int lastTop, int left, 
            List<StateMachineState> addedStates, StateMachine sm)
        {
            // start with initial states
            foreach (StateMachineState state in ChildrenStates(parent))
            {
                if (state.Type == StateMachineStateType.Initial)
                {
                    connections.Add(new DiagramConnection(state.Id.ToString(),
                        parent.Id.ToString(),
                        state.Id.ToString(),
                        "Initial",
                        DiagramConnectionAnchorType.AutoDefault,
                        DiagramConnectionAnchorType.AutoDefault,
                        null, null, 25));

                    left = AddState(elements, lastTop, left, addedStates, state, sm);
                }
            }

            List<StateMachineState> addedStates2;
            do
            {
                addedStates2 = AddOperationStates(elements, connections, ref lastTop, ref left, addedStates, sm);
                addedStates.AddRange(addedStates2);
            } while (addedStates2.Count > 0);

            return lastTop;
        }

        private static ArrayList ChildrenStates(AbstractSchemaItem parent)
        {
            ArrayList result = new ArrayList();
            foreach (StateMachineState state in parent.ChildItemsByType(StateMachineState.ItemTypeConst))
            {
                if (state.Type == StateMachineStateType.Group)
                {
                    result.AddRange(state.ChildItemsByType(StateMachineState.ItemTypeConst));
                }
                else
                {
                    result.Add(state);
                }
            }
            return result;
        }

        private static List<StateMachineState> AddOperationStates(List<DiagramElement> elements, List<DiagramConnection> connections,
            ref int lastTop, ref int left, List<StateMachineState> addedStates, StateMachine sm)
        {
            lastTop += 200;
            left = 0;
            List<StateMachineState> addedStates2 = new List<StateMachineState>();
            foreach (StateMachineState state in addedStates)
            {
                List<StateMachineOperation> operations = new List<StateMachineOperation>();
                StateMachineState currentState = state;
                while (currentState.ParentItem is StateMachine || currentState.ParentItem is StateMachineState)
                {
                    foreach (StateMachineOperation op in currentState.Operations)
                    {
                        operations.Add(op);
                    }
                    if (currentState.ParentItem is StateMachine)
                    {
                        break;
                    }
                    currentState = currentState.ParentItem as StateMachineState;
                }
                foreach (StateMachineOperation operation in operations)
                {
                    StateMachineState targetState = operation.TargetState;
                    if (targetState.DefaultSubstate != null)
                    {
                        targetState = targetState.DefaultSubstate;
                    }

                    int curviness = 50;
                    string source = state.Id.ToString();
                    string target = targetState.Id.ToString();
                    bool found = false;
                    bool foundReverse = false;
                    foreach (DiagramConnection existingConnection in connections)
                    {
                        if (! foundReverse && existingConnection.Source == target
                            && existingConnection.Target == source)
                        {
                            foundReverse = true;
                            DiagramElement sourceElement = GetElement(elements, source);
                            DiagramElement targetElement = GetElement(elements, target);
                            //if (targetElement.Top == sourceElement.Top && existingConnection.Curviness > 0)
                            //{
                                // we reverse the curve if this is a backwards facing connection
                                // so it does not intersect with the forwards facing one
                                // but that is only a problem on horizontal connections
                                curviness = -existingConnection.Curviness;
                            //}
                        }
                        if (!found && existingConnection.Source == source
                            && existingConnection.Target == target)
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        connections.Add(new DiagramConnection(operation.Id.ToString(),
                            source,
                            target,
                            operation.Name,
                            DiagramConnectionAnchorType.AutoDefault,
                            DiagramConnectionAnchorType.AutoDefault,
                            null, null, curviness));
                    }

                    if (!addedStates.Contains(targetState)
                        && !addedStates2.Contains(targetState))
                    {
                        left = AddState(elements, lastTop, left, addedStates2, targetState, sm);
                    }
                }
            }
            return addedStates2;
        }

        private static int AddState(List<DiagramElement> elements, int lastTop, int left, 
            List<StateMachineState> addedStates, StateMachineState state, StateMachine sm)
        {
            string name = state.Name;
            if (sm.Field.DefaultLookup != null)
            {
                name = StateName(state, sm);
            }

            addedStates.Add(state);
            elements.Add(new DiagramElement(state.Id.ToString(), name, null, null, lastTop, left, "state"));
            left += 300;
            return left;
        }

        private static string StateName(StateMachineState state, StateMachine sm)
        {
            string transactionId = null;
            IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
            string name = ls.GetDisplayText((Guid)sm.Field.DefaultLookup.PrimaryKey["Id"], state.Value, transactionId).ToString();
            return name;
        }
    }
}
