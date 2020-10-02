#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using wf = Origam.Schema.WorkflowModel;
using System.Collections;
using Origam.Schema.EntityModel;

namespace Origam.Server.Doc
{
    public class DocSequentialWorkflow : AbstractDoc
    {
        private const int VERTICAL_SPACE = 60;
        private const int HORIZONTAL_SPACE = 60;
        private const int WIDTH = 350;
        private const int LEFT = 10;

        public DocSequentialWorkflow(XmlWriter writer)
            : base(writer)
        {
        }

        public override string FilterName
        {
            get { return "sequentialWorkflow"; }
        }

        public override string Name
        {
            get { return "Sequential Workflows"; }
        }

        public override bool UseDiagrams
        {
            get
            {
                return true;
            }
        }

        public override ISchemaItemProvider RootProvider()
        {
            SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            return ss.GetProvider(typeof(wf.WorkflowSchemaItemProvider));
        }

        public override List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            if (!CheckElementId(writer, elementId)) return null;

            wf.Workflow wf = ps.SchemaProvider.RetrieveInstance(typeof(wf.Workflow), new ModelElementKey(new Guid(elementId))) as wf.Workflow;
            List<DiagramConnection> connections = new List<DiagramConnection>();
            List<DiagramElement> elements = new List<DiagramElement>();
            DiagramElements(wf, elements, connections);
            #region body
            DocTools.WriteStartBody(bodyElement, writer, this.Title(elementId, documentation, ps), "Sequential Workflow", DocTools.ImagePath(wf), wf.Id);
            // summary
            WriteSummary(writer, documentation, wf);
            // Ancestors
            WriteAncestors(writer, wf);
            // Packages
            WritePackages(writer, wf);
            // Diagram
            DocTools.WriteDivStart(writer, "workflowStepsContainer");
            WriteDiagram(writer, elements);
            writer.WriteEndElement();
            // end body
            writer.WriteEndElement();
            #endregion
            return connections;
        }

        private void DiagramElements(wf.Workflow wf, List<DiagramElement> elements, List<DiagramConnection> connections)
        {
            int lastTop = 0;
            int left = LEFT;
            elements.Add(new DiagramElement(wf.Id.ToString(), "START", null, this.FilterName, lastTop, -12, 0, "sequentialWorkflow"));
            lastTop += VERTICAL_SPACE;
            AddNextSteps(wf, elements, connections, ref lastTop, ref left, 0, true);
            
            // center
            int maxRight = 0;
            foreach (DiagramElement element in elements)
            {
                int right = element.Left + element.Width;
                if (right > maxRight)
                {
                    maxRight = right;
                }
            }
            CenterDiagram(elements, maxRight);
        }

        private void AddNextSteps(wf.IWorkflowStep parent, List<DiagramElement> elements,
            List<DiagramConnection> connections, ref int lastTop, ref int left, int depth,
            bool blockContent)
        {
            int column = 0;
            ArrayList next = NextSteps(parent, blockContent);
            foreach (wf.IWorkflowStep step in next)
            {
                if (column > 0)
                {
                    left = (column * WIDTH) + (column * HORIZONTAL_SPACE);
                }
                int top = lastTop;
                
                AddStep(elements, connections, ref top, ref left, depth, step);
                column++;
            }
        }

        private static void CenterDiagram(List<DiagramElement> elements, int parentWidth)
        {
            Dictionary<int, List<DiagramElement>> dict = GroupDiagramLevels(elements);
            foreach (KeyValuePair<int, List<DiagramElement>> item in dict)
            {
                foreach (DiagramElement element in item.Value)
                {
                    int offset = (parentWidth / 2) 
                        - (item.Value.Count * (element.Width / 2))
                        - ((item.Value.Count - 1) * (HORIZONTAL_SPACE / 2));
                    element.Left += offset;
                }
            }
        }

        private static ArrayList NextSteps(wf.IWorkflowStep parent, bool blockContent)
        {
            wf.IWorkflowStep parentItem = parent;
            if (! blockContent)
            {
                parentItem = (wf.IWorkflowStep)parent.ParentItem;
            }

            ArrayList result = new ArrayList();
            foreach (wf.IWorkflowStep step in parentItem.ChildItemsByType(wf.AbstractWorkflowStep.CategoryConst))
            {
                if (blockContent && step.Dependencies.Count == 0)
                {
                    result.Add(step);
                }
                else
                {
                    foreach (wf.WorkflowTaskDependency dependency in step.Dependencies)
                    {
                        if (dependency.Task.PrimaryKey.Equals(parent.PrimaryKey))
                        {
                            result.Add(step);
                        }
                    }
                }
            }

            result.Sort();
            return result;
        }

        private void AddStep(List<DiagramElement> elements, List<DiagramConnection> connections, ref int lastTop, ref int left, int depth, wf.IWorkflowStep step)
        {
            string stepId = step.PrimaryKey["Id"].ToString();

            // check if we added the step already
            foreach (DiagramElement exisingElement in elements)
            {
                if (exisingElement.Id.Equals(stepId))
                {
                    return;
                }
            }

            DiagramElement element = new DiagramElement(stepId, StepName(step), null, null, lastTop, left, "workflowStep");
            element.Height = 20;
            element.Width = (WIDTH - (depth * 20));
            element.Class = "workflowStep";

            // add workflow block content
            if (step is wf.IWorkflowBlock)
            {
                element.Class = "workflowBlock";
                element.Label = "<div class=\"title\">" + step.Name + "</div>";
                int innerTop = 0;
                int innerLeft = 0;
                AddNextSteps(step, element.Children, connections, ref innerTop, ref innerLeft, depth + 1, true);
                lastTop += element.Height;
                // center
                CenterDiagram(element.Children, element.Width);
            }
            
            lastTop += VERTICAL_SPACE;
            AddNextSteps(step, elements, connections, ref lastTop, ref left, depth, false);

            elements.Add(element);

            ArrayList dependencies = step.Dependencies;
            if (dependencies.Count > 0)
            {
                foreach (wf.WorkflowTaskDependency dependency in dependencies)
                {
                    connections.Add(new DiagramConnection(dependency.Id.ToString(),
                        dependency.WorkflowTaskId.ToString(),
                        step.PrimaryKey["Id"].ToString(),
                        null,
                        DiagramConnectionAnchorType.AutoDefault,
                        DiagramConnectionAnchorType.AutoDefault,
                        null, null, 25));
                }
            }
            else if(step.ParentItem is wf.IWorkflow)
            {
                // set initial connector
                connections.Add(new DiagramConnection(Guid.NewGuid().ToString(),
                    step.ParentItem.Id.ToString(),
                    step.PrimaryKey["Id"].ToString(),
                    null,
                    DiagramConnectionAnchorType.AutoDefault,
                    DiagramConnectionAnchorType.AutoDefault,
                    null, null, 25));
            }
        }

        private string StepName(wf.IWorkflowStep step)
        {
            wf.WorkflowCallTask wct = step as wf.WorkflowCallTask;
            wf.ServiceMethodCallTask smct = step as wf.ServiceMethodCallTask;
            wf.UIFormTask uit = step as wf.UIFormTask;
            wf.WorkflowReference wr = GetWorkflowReference(step);
            TransformationReference tr = GetTransformationReference(step);

            if (wct != null)
            {
                ISchemaItem item = wct.Workflow;
                return DocTools.ImageLink(item, this.FilterName);
            }
            if (wr != null)
            {
                ISchemaItem item = wr.Workflow;
                return DocTools.ImageLink(item, this.FilterName);
            }
            if (tr != null)
            {
                ISchemaItem item = tr.Transformation;
                return DocTools.ImageLink(item, new DocTransformation(null).FilterName);
            }
            if (uit != null)
            {
                return DocTools.ImageLink(uit.Screen, new DocScreen(null).FilterName);
            }
            string spanClass = "workflowStepName";
            string imageClass = "workflowStepImage";
            if (step.Name.Length > 30)
            {
                spanClass = "workflowStepNameWide";
                imageClass = "workflowStepImageWide";
            }
            return DocTools.Image(step, imageClass) + "<span class=\"" + spanClass + "\">" + step.Name + "</span>";
        }

        public void WriteUITasks(XmlWriter writer, IDocumentationService documentation, wf.IWorkflowBlock block)
        {
            List<Guid> processedBlocks = new List<Guid>();
            int chapterNumber = 0;
            WriteUITasks(writer, documentation, block, ref chapterNumber, processedBlocks);
        }

        private void WriteUITasks(XmlWriter writer, IDocumentationService documentation, wf.IWorkflowBlock block,
            ref int chapterNumber, List<Guid> processedBlocks)
        {
            Guid blockId = (Guid)block.PrimaryKey["Id"];
            if (processedBlocks.Contains(blockId))
            {
                // this would turn into an infinite loop - return
                return;
            }
            processedBlocks.Add(blockId);

            ArrayList sortedList = new ArrayList(block.ChildItemsByType(wf.AbstractWorkflowStep.CategoryConst));
            sortedList.Sort();

            foreach (wf.IWorkflowStep step in sortedList)
            {
                wf.UIFormTask uiTask = step as wf.UIFormTask;

                if (uiTask != null)
                {
                    chapterNumber++;
                    string description = documentation.GetDocumentation((Guid)step.PrimaryKey["Id"], DocumentationType.USER_WFSTEP_DESCRIPTION);
                    if(string.IsNullOrEmpty(description)) description = step.Name;

                    DocTools.WriteSectionStart(writer, string.Format("Step {0}: {1}",
                        chapterNumber,
                        description
                        ));

                    WriteSummary(writer, documentation, step);
                    writer.WriteStartElement("p");
                    writer.WriteString("For a complete screen description see ");
                    DocTools.WriteElementLink(writer, uiTask.Screen, new DocScreen(null).FilterName);
                    writer.WriteString(".");
                    writer.WriteEndElement();
                    // section end
                    writer.WriteEndElement();
                }

                if (step is wf.IWorkflowBlock)
                {
                    WriteUITasks(writer, documentation, step as wf.IWorkflowBlock, ref chapterNumber, processedBlocks);
                }

                if (step is wf.WorkflowCallTask)
                {
                    WriteUITasks(writer, documentation, (step as wf.WorkflowCallTask).Workflow, ref chapterNumber, processedBlocks);
                }

                wf.WorkflowReference wr = GetWorkflowReference(step);
                if (wr != null)
                {
                    WriteUITasks(writer, documentation, wr.Workflow, ref chapterNumber, processedBlocks);
                }
            }
        }

        private static wf.WorkflowReference GetWorkflowReference(wf.IWorkflowStep step)
        {
            ISchemaItem result = GetServiceParameter(step, "WorkflowService", "ExecuteWorkflow", "Workflow");
            if (result is wf.WorkflowReference)
            {
                return (wf.WorkflowReference)result;
            }
            return null;
        }

        private static TransformationReference GetTransformationReference(wf.IWorkflowStep step)
        {
            ISchemaItem result = GetServiceParameter(step, "DataTransformationService", "Transform", "XslScript");
            if (result is TransformationReference)
            {
                return (TransformationReference)result;
            }
            return null;
        }

        private static ISchemaItem GetServiceParameter(wf.IWorkflowStep step, string serviceName, string methodName, string parameterName)
        {
            ISchemaItem result = null;
            wf.ServiceMethodCallTask smct = step as wf.ServiceMethodCallTask;
            if (smct != null && smct.Service != null && smct.ServiceMethod != null
                && smct.Service.Name == serviceName && smct.ServiceMethod.Name == methodName)
            {
                wf.ServiceMethodCallParameter par = (wf.ServiceMethodCallParameter)smct.GetChildByName(parameterName);
                if (par != null && par.ChildItems.Count > 0)
                {
                    result = par.ChildItems[0];
                }
            }
            return result;
        }

    }
}
